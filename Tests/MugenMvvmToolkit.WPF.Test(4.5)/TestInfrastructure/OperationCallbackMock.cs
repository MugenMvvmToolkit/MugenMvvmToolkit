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

        /// <summary>
        ///     Gets a value indicating whether the <see cref="IOperationCallback" /> is serializable.
        /// </summary>
        public bool IsSerializable { get; set; }

        /// <summary>
        ///     Invokes the callback using the specified operation result.
        /// </summary>
        void IOperationCallback.Invoke(IOperationResult result)
        {
            Invoke(result);
        }

        #endregion
    }
}