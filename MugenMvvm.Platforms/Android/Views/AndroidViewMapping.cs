using System;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Views;

namespace MugenMvvm.Android.Views
{
    public class AndroidViewMapping : ViewMapping, IAndroidViewMapping
    {
        #region Constructors

        public AndroidViewMapping(int resourceId, Type viewType, Type viewModelType, IReadOnlyMetadataContext? metadata = null)
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