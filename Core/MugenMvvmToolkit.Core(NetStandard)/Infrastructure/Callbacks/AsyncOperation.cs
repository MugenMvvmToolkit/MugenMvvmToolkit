#region Copyright

// ****************************************************************************
// <copyright file="AsyncOperation.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using JetBrains.Annotations;
using MugenMvvmToolkit.Attributes;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Infrastructure.Callbacks
{
    public class AsyncOperation<TResult> : IAsyncOperation<TResult>, IAsyncOperationInternal, IActionContinuation,
                                           IActionContinuation<TResult>
    {
        #region Fields

        private const int StartedState = 1;
        private readonly List<IAsyncOperationInternal> _continuations;
        private ManualResetEvent _waitHandle;
        private IOperationResult<TResult> _result;
        private int _state;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public AsyncOperation()
            : this(null)
        {
        }

        [Preserve(Conditional = true)]
        public AsyncOperation(IDataContext context)
        {
            _continuations = new List<IAsyncOperationInternal>(2);
            Context = context.ToNonReadOnly();
        }

        #endregion

        #region Methods

        public static bool TrySetResult(IAsyncOperation operation, IOperationResult result)
        {
            var operationInternal = operation as IAsyncOperationInternal;
            if (operationInternal == null)
                return false;
            return operationInternal.SetResult(result, false);
        }

        public void SetResult([NotNull] IOperationResult<TResult> result)
        {
            Should.NotBeNull(result, nameof(result));
            ((IAsyncOperationInternal)this).SetResult(result, true);
        }

        public bool TrySetResult([NotNull] IOperationResult<TResult> result)
        {
            Should.NotBeNull(result, nameof(result));
            return ((IAsyncOperationInternal)this).SetResult(result, false);
        }

        internal virtual IOperationResult<TResult> InvokeInternal(IOperationResult result)
        {
            return OperationResult.Convert<TResult>(result);
        }

        internal virtual ISerializableCallback ToSerializableCallbackInternal()
        {
            List<object> continuationsCallbacks = GetContinuationsCallbacks();
            if (continuationsCallbacks == null)
                return null;
            return new AsyncOperationSerializableCallback(null, null, typeof(TResult).AssemblyQualifiedName, false,
                continuationsCallbacks);
        }

        [CanBeNull]
        internal List<object> GetContinuationsCallbacks()
        {
            if (!IsCompleted)
            {
                lock (_continuations)
                {
                    if (!IsCompleted)
                    {
                        if (_continuations.Count == 0)
                            return null;
                        var list = new List<object>(_continuations.Count);
                        foreach (IAsyncOperationInternal continuation in _continuations)
                        {
                            ISerializableCallback callback = continuation.ToSerializableCallback();
                            if (callback != null)
                                list.Add(callback);
                        }
                        if (list.Count == 0)
                            return null;
                        return list;
                    }
                }
            }
            return null;
        }

        private IAsyncOperationInternal AddContinuation(IAsyncOperationInternal asyncOperation)
        {
            if (!IsCompleted)
            {
                lock (_continuations)
                {
                    if (!IsCompleted)
                    {
                        _continuations.Add(asyncOperation);
                        return asyncOperation;
                    }
                }
            }
            asyncOperation.SetResult(_result, true);
            return asyncOperation;
        }

        private void Invoke(IOperationResult result)
        {
            try
            {
                _result = InvokeInternal(result);
            }
            catch (Exception e)
            {
                Tracer.Error(e.Flatten(false));
                if (result.Exception != null)
                    e = new AggregateException(result.Exception, e);
                else if (e is OperationCanceledException)
                    e = null;
                if (e == null)
                    _result = OperationResult.CreateCancelResult<TResult>(result.Operation, result.Source,
                        result.OperationContext);
                else
                    _result = OperationResult.CreateErrorResult<TResult>(result.Operation, result.Source,
                        e, result.OperationContext);
            }
            finally
            {
                Interlocked.CompareExchange(ref _waitHandle, Empty.CompletedEvent, null);
                _waitHandle.Set();
            }
        }

        private void InitializeHandle()
        {
            var value = new ManualResetEvent(false);
            Interlocked.CompareExchange(ref _waitHandle, value, null);
            if (!ReferenceEquals(value, _waitHandle))
                value.Dispose();
        }

        #endregion

        #region Implementation of IContinuation

        public virtual ISerializableCallback ToSerializableCallback()
        {
            return ToSerializableCallbackInternal();
        }

        void IActionContinuation.Invoke(IOperationResult result)
        {
            ((IAsyncOperationInternal)this).SetResult(result, true);
        }

        void IActionContinuation<TResult>.Invoke(IOperationResult<TResult> result)
        {
            SetResult(result);
        }

        #endregion

        #region Implementation of IAsyncOperationInternal

        bool IAsyncOperationInternal.SetResult(IOperationResult result, bool throwOnError)
        {
            if (Interlocked.Exchange(ref _state, StartedState) == StartedState)
            {
                if (throwOnError)
                    ExceptionManager.ObjectInitialized("AsyncOperation", this);
                return false;
            }
            Invoke(result);
            lock (_continuations)
            {
                // Ensure that all concurrent adds have completed.
            }

            for (int i = 0; i < _continuations.Count; i++)
                _continuations[i].SetResult(_result, true);
            _continuations.Clear();
            return true;
        }

        #endregion

        #region Implementation of IAsyncOperation

        public bool IsCompleted => _result != null;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public IOperationResult<TResult> Result
        {
            get
            {
                Wait();
                return _result;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IOperationResult IAsyncOperation.Result => Result;

        public IDataContext Context { get; }

        public void Wait()
        {
            InitializeHandle();
            _waitHandle.WaitOne();
        }

        public bool Wait(int millisecondsTimeout)
        {
            InitializeHandle();
            return _waitHandle.WaitOne(millisecondsTimeout);
        }

        public IAsyncOperation ContinueWith(IActionContinuation continuationAction)
        {
            return AddContinuation(new AsyncOperationImpl<object, object>(continuationAction, Context));
        }

        public IAsyncOperation<T> ContinueWith<T>(IFunctionContinuation<T> continuationFunction)
        {
            return (IAsyncOperation<T>)AddContinuation(new AsyncOperationImpl<T, object>(continuationFunction, Context));
        }

        public IAsyncOperation ContinueWith(IActionContinuation<TResult> continuationAction)
        {
            return AddContinuation(new AsyncOperationImpl<TResult, object>(continuationAction, Context));
        }

        public IAsyncOperation<TNewResult> ContinueWith<TNewResult>(IFunctionContinuation<TResult, TNewResult> continuationFunction)
        {
            return (IAsyncOperation<TNewResult>)AddContinuation(new AsyncOperationImpl<TNewResult, TResult>(continuationFunction, Context));
        }

        public virtual IOperationCallback ToOperationCallback()
        {
            return new AsyncOperationCallback(this);
        }

        #endregion
    }
}
