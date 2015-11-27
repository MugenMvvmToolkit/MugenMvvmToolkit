using System;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public class OperationCallbackManagerMock : IOperationCallbackManager
    {
        #region Properties

        public Action<OperationType, object, IOperationCallback, IDataContext> Register { get; set; }

        public Action<object, IOperationResult> SetResult { get; set; }

        #endregion

        #region Implementation of IOperationCallbackManager

        void IOperationCallbackManager.Register(OperationType operation, object source, IOperationCallback callback,
            IDataContext context)
        {
            if (Register == null)
                Tracer.Warn("OperationCallbackManagerMock: Register == null");
            else
                Register(operation, source, callback, context);
        }

        void IOperationCallbackManager.SetResult(IOperationResult result)
        {
            if (SetResult == null)
                Tracer.Warn("OperationCallbackManagerMock: SetResult == null");
            else
                SetResult(result.Source, result);
        }

        #endregion
    }
}
