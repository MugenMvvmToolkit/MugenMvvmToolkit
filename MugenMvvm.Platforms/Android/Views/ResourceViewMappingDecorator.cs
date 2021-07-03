﻿using MugenMvvm.Android.Interfaces;
using MugenMvvm.Android.Native.Interfaces.Views;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;

namespace MugenMvvm.Android.Views
{
    public sealed class ResourceViewMappingDecorator : ComponentDecoratorBase<IViewManager, IViewMappingProviderComponent>, IViewMappingProviderComponent
    {
        public ResourceViewMappingDecorator(int priority = ViewComponentPriority.ResourceMappingDecorator) : base(priority)
        {
        }

        public ItemOrIReadOnlyList<IViewMapping> TryGetMappings(IViewManager viewManager, object request, IReadOnlyMetadataContext? metadata)
        {
            var mappings = Components.TryGetMappings(viewManager, request, metadata);
            if (mappings.IsEmpty)
                return mappings;

            MugenExtensions.TryGetViewModelView(request, out IResourceView? view);
            if (view == null)
                return mappings;

            var viewId = view.ViewId;
            if (viewId == 0)
                return mappings;

            var result = new ItemOrListEditor<IViewMapping>(2);
            foreach (var mapping in mappings)
            {
                if (!mapping.ViewType.IsInterface && mapping.ViewType.IsInstanceOfType(view))
                    result.Add(mapping);
                else if (mapping is IResourceViewMapping map && map.ResourceId == viewId)
                    result.Add(mapping);
            }

            return result.ToItemOrList();
        }
    }
}