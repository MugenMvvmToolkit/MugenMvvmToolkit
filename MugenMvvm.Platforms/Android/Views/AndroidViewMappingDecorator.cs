using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Android.Interfaces;
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
    public sealed class AndroidViewMappingDecorator : ComponentDecoratorBase<IViewManager, IViewMappingProviderComponent>, IViewMappingProviderComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = ViewComponentPriority.MappingProvider + 1;

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IViewMapping, IReadOnlyList<IViewMapping>> TryGetMappings<TRequest>(IViewManager viewManager, [DisallowNull]in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            var mappings = Components.TryGetMappings(viewManager, request, metadata);
            MugenExtensions.TryGetViewModelView(request, out IResourceView? view);
            if (view == null)
                return mappings;
            ItemOrList<IViewMapping, List<IViewMapping>> result = default;
            for (var i = 0; i < mappings.Count(); i++)
            {
                var mapping = mappings.Get(i);
                if (mapping is IAndroidViewMapping map && map.ResourceId == view.ViewId)
                    result.Add(mapping);
            }

            return result.Cast<IReadOnlyList<IViewMapping>>();
        }

        #endregion
    }
}