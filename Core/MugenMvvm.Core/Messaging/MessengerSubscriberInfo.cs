using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Enums;

namespace MugenMvvm.Messaging
{
    [Serializable]//todo review serializable
    [DataContract(Namespace = BuildConstant.DataContractNamespace)]
    [Preserve(Conditional = true, AllMembers = true)]
    [StructLayout(LayoutKind.Auto)]
    public readonly struct MessengerSubscriberInfo
    {
        #region Fields

        [DataMember(Name = "E")]
        public readonly ThreadExecutionMode ExecutionMode;

        [DataMember(Name = "S")]
        public readonly object Subscriber;

        #endregion

        #region Constructors

        public MessengerSubscriberInfo(object subscriber, ThreadExecutionMode executionMode)
        {
            Should.NotBeNull(subscriber, nameof(subscriber));
            Should.NotBeNull(executionMode, nameof(executionMode));
            Subscriber = subscriber;
            ExecutionMode = executionMode;
        }

        #endregion
    }
}