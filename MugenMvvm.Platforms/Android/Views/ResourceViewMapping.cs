using System;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Views;

namespace MugenMvvm.Android.Views
{
    public class ResourceViewMapping : ViewMapping, IResourceViewMapping
    {
        #region Constructors

        public ResourceViewMapping(int resourceId, Type viewType, Type viewModelType, IReadOnlyMetadataContext? metadata = null)
            : base(viewModelType.Name + resourceId, viewType, viewModelType, metadata)
        {
            ResourceId = resourceId;
        }

        #endregion

        #region Properties

        public int ResourceId { get; }

        #endregion
    }
}