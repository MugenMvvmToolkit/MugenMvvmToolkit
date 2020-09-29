using System.Collections.Generic;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Android.Native.Interfaces.Views;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Android.Views
{
    public sealed class ResourceViewMappingDecorator : ComponentDecoratorBase<IViewManager, IViewMappingProviderComponent>, IViewMappingProviderComponent
    {
        #region Constructors

        public ResourceViewMappingDecorator(int priority = ViewComponentPriority.MappingProvider + 1) : base(priority)
        {
        }

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IViewMapping, IReadOnlyList<IViewMapping>> TryGetMappings(IViewManager viewManager, object request, IReadOnlyMetadataContext? metadata)
        {
            var mappings = Components.TryGetMappings(viewManager, request, metadata);
            MugenExtensions.TryGetViewModelView(request, out IResourceView? view);
            if (view == null)
                return mappings;

            var viewId = view.ViewId;
            if (viewId == 0)
                return mappings;

            var result = ItemOrListEditor.Get<IViewMapping>();
            foreach (var mapping in mappings.Iterator())
            {
                if (!mapping.ViewType.IsInterface && mapping.ViewType.IsInstanceOfType(view))
                    result.Add(mapping);
                else if (mapping is IResourceViewMapping map && map.ResourceId == viewId)
                    result.Add(mapping);
            }

            return result.ToItemOrList<IReadOnlyList<IViewMapping>>();
        }

        #endregion
    }
}