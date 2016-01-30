using System;
using System.Runtime.Serialization;
using MugenMvvmToolkit.Interfaces.Callbacks;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    [DataContract]
    public class SerializableCallbackMock : ISerializableCallback
    {
        #region Properties

        public Func<IOperationResult, object> Invoke { get; set; }

        #endregion

        #region Implementation of ISerializableCallback

        object ISerializableCallback.Invoke(IOperationResult result)
        {
            if (Invoke == null)
                return null;
            return Invoke(result);
        }

        #endregion
    }
}
