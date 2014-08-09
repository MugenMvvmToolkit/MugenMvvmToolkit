using System;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public class OperationCallbackFactoryMock : IOperationCallbackFactory
    {
        #region Properties

        public Func<Delegate, ISerializableCallback> CreateSerializableCallback { get; set; }

        #endregion

        #region Implementation of IOperationCallbackFactory

        /// <summary>
        ///     Creates an instance of <see cref="IAsyncOperationAwaiter" />.
        /// </summary>
        IAsyncOperationAwaiter IOperationCallbackFactory.CreateAwaiter(IAsyncOperation operation, IDataContext context)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        ///     Creates an instance of <see cref="IAsyncOperationAwaiter{TResult}" />.
        /// </summary>
        IAsyncOperationAwaiter<TResult> IOperationCallbackFactory.CreateAwaiter<TResult>(
            IAsyncOperation<TResult> operation, IDataContext context)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        ///     Tries to convert a delegate to an instance of <see cref="ISerializableCallback" />.
        /// </summary>
        ISerializableCallback IOperationCallbackFactory.CreateSerializableCallback(Delegate @delegate)
        {
            if (CreateSerializableCallback == null)
                return null;
            return CreateSerializableCallback(@delegate);
        }

        #endregion
    }
}