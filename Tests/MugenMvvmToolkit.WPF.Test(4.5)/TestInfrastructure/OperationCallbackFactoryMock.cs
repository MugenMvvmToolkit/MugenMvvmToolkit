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

        IAsyncOperationAwaiter IOperationCallbackFactory.CreateAwaiter(IAsyncOperation operation, IDataContext context)
        {
            throw new NotSupportedException();
        }

        IAsyncOperationAwaiter<TResult> IOperationCallbackFactory.CreateAwaiter<TResult>(
            IAsyncOperation<TResult> operation, IDataContext context)
        {
            throw new NotSupportedException();
        }

        ISerializableCallback IOperationCallbackFactory.CreateSerializableCallback(Delegate @delegate)
        {
            if (CreateSerializableCallback == null)
                return null;
            return CreateSerializableCallback(@delegate);
        }

        #endregion
    }
}
