#region Copyright

// ****************************************************************************
// <copyright file="AsyncOperation.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
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
using MugenMvvmToolkit.Interfaces.Callbacks;

namespace MugenMvvmToolkit.Infrastructure.Callbacks
{
    /// <summary>
    ///     Represents the async operation.
    /// </summary>
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

        /// <summary>
        ///     Initializes a new instance of the <see cref="AsyncOperation{TResult}" /> class.
        /// </summary>
        public AsyncOperation()
        {
            _continuations = new List<IAsyncOperationInternal>(2);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Sets the result of this operation.
        /// </summary>
        public void SetResult([NotNull] IOperationResult<TResult> result)
        {
            Should.NotBeNull(result, "result");
            ((IAsyncOperationInternal)this).SetResult(result, true);
        }

        /// <summary>
        ///     Tries to sets the result of this operation.
        /// </summary>
        public bool TrySetResult([NotNull] IOperationResult<TResult> result)
        {
            Should.NotBeNull(result, "result");
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

        private T AddContinuation<T>(T asyncOperation)
            where T : IAsyncOperationInternal
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

        /// <summary>
        ///     Tries to convert current operation to an instance of <see cref="ISerializableCallback" />.
        /// </summary>
        public virtual ISerializableCallback ToSerializableCallback()
        {
            return ToSerializableCallbackInternal();
        }

        /// <summary>
        ///     Invokes the action using the specified operation result.
        /// </summary>
        void IActionContinuation.Invoke(IOperationResult result)
        {
            ((IAsyncOperationInternal)this).SetResult(result, true);
        }

        /// <summary>
        ///     Invokes the action using the specified operation result.
        /// </summary>
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

        /// <summary>
        ///     Gets whether the operation has completed.
        /// </summary>
        public bool IsCompleted
        {
            get { return _result != null; }
        }

        /// <summary>
        ///     Gets the operation result.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public IOperationResult<TResult> Result
        {
            get
            {
                Wait();
                return _result;
            }
        }

        /// <summary>
        ///     Gets the operation result.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IOperationResult IAsyncOperation.Result
        {
            get { return Result; }
        }

        /// <summary>
        ///     Waits for the operation to complete execution.
        /// </summary>
        public void Wait()
        {
            InitializeHandle();
            _waitHandle.WaitOne();
        }

        /// <summary>
        ///     Waits for the operation to complete execution.
        /// </summary>
        public bool Wait(int millisecondsTimeout)
        {
            InitializeHandle();
            return _waitHandle.WaitOne(millisecondsTimeout);
        }

        /// <summary>
        ///     Creates a continuation that executes when the target operation completes.
        /// </summary>
        public IAsyncOperation ContinueWith(IActionContinuation continuationAction)
        {
            return AddContinuation(new AsyncOperationImpl<object, object>(continuationAction));
        }

        /// <summary>
        ///     Creates a continuation that executes when the target operation completes.
        /// </summary>
        public IAsyncOperation<T> ContinueWith<T>(IFunctionContinuation<T> continuationFunction)
        {
            return AddContinuation(new AsyncOperationImpl<T, object>(continuationFunction));
        }

        /// <summary>
        ///     Creates a continuation that executes when the target operation completes.
        /// </summary>
        public IAsyncOperation ContinueWith(IActionContinuation<TResult> continuationAction)
        {
            return AddContinuation(new AsyncOperationImpl<TResult, object>(continuationAction));
        }

        /// <summary>
        ///     Creates a continuation that executes when the target operation completes.
        /// </summary>
        public IAsyncOperation<TNewResult> ContinueWith<TNewResult>(
            IFunctionContinuation<TResult, TNewResult> continuationFunction)
        {
            return AddContinuation(new AsyncOperationImpl<TNewResult, TResult>(continuationFunction));
        }

        /// <summary>
        ///     Converts the current <see cref="IAsyncOperation{TResult}" /> to the <see cref="IOperationCallback" />.
        /// </summary>
        public virtual IOperationCallback ToOperationCallback()
        {
            return new AsyncOperationCallback(this);
        }

        #endregion
    }
}