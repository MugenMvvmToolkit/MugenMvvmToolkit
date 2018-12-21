using System;
using MugenMvvm.Interfaces;
using MugenMvvm.Models;

namespace MugenMvvm
{
    public static class ViewModelMetadataKeys
    {
        #region Properties

        public static IContextKey<ViewModelLifecycleState> LifecycleState { get; set; }

        public static IContextKey<Guid> Id { get; set; }

        #endregion
    }
}