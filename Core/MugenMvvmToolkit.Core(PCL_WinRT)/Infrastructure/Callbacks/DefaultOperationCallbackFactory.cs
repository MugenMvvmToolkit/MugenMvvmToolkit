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
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Infrastructure.Callbacks
{
    /// <summary>
    ///     Rerpresets the factory that allows to create callback operations.
    /// </summary>
    public class DefaultOperationCallbackFactory : IOperationCallbackFactory
    {
        #region Nested types

        private sealed class AsyncOperationAwaiter<TResult> : IAsyncOperationAwaiter<TResult>, IAsyncOperationAwaiter
        {
            #region Fields

            private readonly IAsyncOperation _asyncOperation;

            #endregion

            #region Constructors

            public AsyncOperationAwaiter(IAsyncOperation asyncOperation)
            {
                Should.NotBeNull(asyncOperation, "asyncOperation");
                _asyncOperation = asyncOperation;
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
                _asyncOperation.ContinueWith(new AwaiterContinuation(continuation));
            }

            #endregion
        }

        private sealed class AwaiterContinuation : IActionContinuation
        {
            #region Fields

            private readonly Action _continuation;

            #endregion

            #region Constructors

            public AwaiterContinuation(Action continuation)
            {
                Should.NotBeNull(continuation, "continuation");
                _continuation = continuation;
            }

            #endregion

            #region Implementation of IContinuation

            public ISerializableCallback ToSerializableCallback()
            {
                return null;
            }

            public void Invoke(IOperationResult result)
            {
                _continuation();
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

        /// <summary>
        ///     Gets the <see cref="IOperationCallbackFactory" />.
        /// </summary>
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

        /// <summary>
        ///     Creates an instance of <see cref="IAsyncOperationAwaiter" />.
        /// </summary>
        public IAsyncOperationAwaiter CreateAwaiter(IAsyncOperation operation, IDataContext context)
        {
            return new AsyncOperationAwaiter<object>(operation);
        }

        /// <summary>
        ///     Creates an instance of <see cref="IAsyncOperationAwaiter{TResult}" />.
        /// </summary>
        public IAsyncOperationAwaiter<TResult> CreateAwaiter<TResult>(IAsyncOperation<TResult> operation, IDataContext context)
        {
            return new AsyncOperationAwaiter<TResult>(operation);
        }

        /// <summary>
        ///     Tries to convert a delegate to an instance of <see cref="ISerializableCallback" />.
        /// </summary>
        public ISerializableCallback CreateSerializableCallback(Delegate @delegate)
        {
            return null;
        }

        #endregion
    }
}