#region Copyright

// ****************************************************************************
// <copyright file="DefaultOperationCallbackFactory.cs">
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
using System.Runtime.CompilerServices;
using System.Threading;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Infrastructure.Callbacks
{
    public class DefaultOperationCallbackFactory : IOperationCallbackFactory
    {
        #region Nested types

        private sealed class AsyncOperationAwaiter<TResult> : IAsyncOperationAwaiter<TResult>, IAsyncOperationAwaiter
        {
            #region Fields

            private readonly IAsyncOperation _asyncOperation;
            private readonly bool _continueOnCapturedContext;

            #endregion

            #region Constructors

            public AsyncOperationAwaiter(IAsyncOperation asyncOperation, bool continueOnCapturedContext)
            {
                Should.NotBeNull(asyncOperation, "asyncOperation");
                _asyncOperation = asyncOperation;
                _continueOnCapturedContext = continueOnCapturedContext;
            }

            #endregion

            #region Implementation of IAsyncOperationAwaiter

            void IAsyncOperationAwaiter.GetResult()
            {
                object result = _asyncOperation.Result.Result;
            }

            public bool IsCompleted
            {
                get { return _asyncOperation.IsCompleted; }
            }

            TResult IAsyncOperationAwaiter<TResult>.GetResult()
            {
                return (TResult)_asyncOperation.Result.Result;
            }

            void INotifyCompletion.OnCompleted(Action continuation)
            {
                _asyncOperation.ContinueWith(new AwaiterContinuation(continuation, _continueOnCapturedContext));
            }

            #endregion
        }

        private sealed class AwaiterContinuation : IActionContinuation
        {
            #region Fields

            private readonly Action _continuation;
            private readonly SynchronizationContext _context;

            #endregion

            #region Constructors

            public AwaiterContinuation(Action continuation, bool continueOnCapturedContext)
            {
                Should.NotBeNull(continuation, "continuation");
                _continuation = continuation;
                if (continueOnCapturedContext)
                    _context = SynchronizationContext.Current;
            }

            #endregion

            #region Implementation of IContinuation

            public ISerializableCallback ToSerializableCallback()
            {
                return null;
            }

            public void Invoke(IOperationResult result)
            {
                if (_context == null || ReferenceEquals(SynchronizationContext.Current, _context))
                    _continuation();
                else
                    _context.Post(state => ((Action)state).Invoke(), _continuation);
            }

            #endregion
        }

        #endregion

        #region Fields

        private static DefaultOperationCallbackFactory _factory;

        #endregion

        #region Constructors

        private DefaultOperationCallbackFactory()
        {
        }

        #endregion

        #region Properties

        public static IOperationCallbackFactory Instance
        {
            get
            {
                if (_factory == null)
                    _factory = new DefaultOperationCallbackFactory();
                return _factory;
            }
        }

        #endregion

        #region Implementation of IOperationCallbackFactory

        public IAsyncOperationAwaiter CreateAwaiter(IAsyncOperation operation, IDataContext context)
        {
            return CreateAwaiterInternal<object>(operation, context);
        }

        public IAsyncOperationAwaiter<TResult> CreateAwaiter<TResult>(IAsyncOperation<TResult> operation, IDataContext context)
        {
            return CreateAwaiterInternal<TResult>(operation, context);
        }

        public ISerializableCallback CreateSerializableCallback(Delegate @delegate)
        {
            return null;
        }

        #endregion

        #region Methods

        private static AsyncOperationAwaiter<TResult> CreateAwaiterInternal<TResult>(IAsyncOperation operation, IDataContext context)
        {
            Should.NotBeNull(operation, "operation");
            if (context == null)
                context = DataContext.Empty;
            bool continueOnCapturedContext;
            if (!context.TryGetData(OpeartionCallbackConstants.ContinueOnCapturedContext, out continueOnCapturedContext))
                continueOnCapturedContext = true;
            return new AsyncOperationAwaiter<TResult>(operation, continueOnCapturedContext);
        }

        #endregion
    }
}
