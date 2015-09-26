using System;
using MugenMvvmToolkit.Interfaces.Callbacks;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public class OperationCallbackMock : IOperationCallback
    {
        #region Properties

        public Action<IOperationResult> Invoke { get; set; }

        #endregion

        #region Implementation of IOperationCallback

        public bool IsSerializable { get; set; }

        void IOperationCallback.Invoke(IOperationResult result)
        {
            Invoke(result);
        }

        #endregion
    }
}
