using System;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Views;

namespace MugenMvvm.Android.Views
{
    public class ResourceViewMapping : ViewMapping, IResourceViewMapping
    {
        public ResourceViewMapping(int resourceId, Type viewModelType, Type viewType, IReadOnlyMetadataContext? metadata = null)
            : base(viewModelType.Name + resourceId, viewModelType, viewType, metadata)
        {
            ResourceId = resourceId;
        }

        public int ResourceId { get; }
    }
}