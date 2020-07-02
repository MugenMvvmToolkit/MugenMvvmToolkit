using System.Collections.Generic;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Extensions.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Internal;
using MugenMvvm.Android.Native.Interfaces.Views;

namespace MugenMvvm.Android.Views
{
    public sealed class AndroidViewModelViewMappingDecorator : ComponentDecoratorBase<IViewManager, IViewModelViewMappingProviderComponent>, IViewModelViewMappingProviderComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = ViewComponentPriority.MappingProvider + 1;

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IViewModelViewMapping, IReadOnlyList<IViewModelViewMapping>> TryGetMappings<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            var mappings = Components.TryGetMappings(request, metadata);
            MugenExtensions.TryGetViewModelView(request, out IResourceView? view);
            if (view == null)
                return mappings;
            ItemOrList<IViewModelViewMapping, List<IViewModelViewMapping>> result = default;
            for (var i = 0; i < mappings.Count(); i++)
            {
                var mapping = mappings.Get(i);
                if (mapping is AndroidViewModelViewMapping map && map.ResourceId == view.ViewId)
                    result.Add(mapping);
            }

            return result.Cast<IReadOnlyList<IViewModelViewMapping>>();
        }

        #endregion
    }
}