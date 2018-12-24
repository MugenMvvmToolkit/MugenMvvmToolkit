using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Models;

namespace MugenMvvm
{
    public static class ViewModelMetadataKeys
    {
        #region Properties

        public static IMetadataContextKey<ViewModelLifecycleState> LifecycleState { get; set; }

        public static IMetadataContextKey<Guid> Id { get; set; }

        #endregion
    }
}