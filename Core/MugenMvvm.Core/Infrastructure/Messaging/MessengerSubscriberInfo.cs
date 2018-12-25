﻿using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Models;

namespace MugenMvvm.Infrastructure.Messaging
{
    [Serializable]
    [DataContract(Namespace = BuildConstants.DataContractNamespace)]
    [Preserve(Conditional = true, AllMembers = true)]
    public struct MessengerSubscriberInfo
    {
        #region Fields

        [DataMember(Name = "E")]
        public readonly ThreadExecutionMode ExecutionMode;

        [DataMember(Name = "S")]
        public readonly IMessengerSubscriber Subscriber;

        #endregion

        #region Constructors

        public MessengerSubscriberInfo(IMessengerSubscriber subscriber, ThreadExecutionMode executionMode)
        {
            Subscriber = subscriber;
            ExecutionMode = executionMode;
        }

        #endregion
    }
}