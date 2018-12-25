using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Models;

namespace MugenMvvm
{
    public static class ViewModelMetadataKeys
    {
        #region Properties

        public static IMetadataContextKey<Guid> Id { get; set; }

        public static IMetadataContextKey<ViewModelLifecycleState> LifecycleState { get; set; }

        public static IMetadataContextKey<bool> BroadcastAllMessages { get; set; }

        public static IMetadataContextKey<BusyMessageHandlerType> BusyMessageHandlerType { get; set; }

        #endregion
    }
}