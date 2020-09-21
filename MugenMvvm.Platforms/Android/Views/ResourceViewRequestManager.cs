﻿using System.Threading;
using System.Threading.Tasks;
using Java.Lang;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Android.Native.Views;
using MugenMvvm.Android.Requests;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;

namespace MugenMvvm.Android.Views
{
    public sealed class ResourceViewRequestManager : ComponentDecoratorBase<IViewManager, IViewManagerComponent>, IViewManagerComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = ViewComponentPriority.ViewModelViewProviderDecorator + 1;

        #endregion

        #region Implementation of interfaces

        public ValueTask<IView?> TryInitializeAsync(IViewManager viewManager, IViewMapping mapping, object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            if (mapping.IsUndefined() && request is ResourceViewRequest viewRequest && viewRequest.ViewModel != null && viewRequest.Container is Object container)
            {
                IResourceViewMapping? viewMapping = null;
                foreach (var t in viewManager.GetMappings(viewRequest.ViewModel, metadata).Iterator())
                {
                    if (t is IResourceViewMapping m && (viewRequest.ResourceId == 0 || m.ResourceId == viewRequest.ResourceId))
                    {
                        viewMapping = m;
                        break;
                    }
                }

                if (viewMapping != null || viewRequest.ResourceId != 0)
                {
                    var resourceId = viewMapping?.ResourceId ?? viewRequest.ResourceId;
                    foreach (var v in viewManager.GetViews(viewRequest.ViewModel, metadata).Iterator())
                    {
                        if (v.Mapping is IResourceViewMapping m && m.ResourceId == resourceId)
                            return new ValueTask<IView?>(v);
                    }

                    var view = ViewExtensions.GetView(container, resourceId, true);
                    viewMapping ??= new ResourceViewMapping(resourceId, view.GetType(), viewRequest.ViewModel.GetType(), metadata);
                    viewRequest.View = view;
                    mapping = viewMapping;
                }
            }

            return Components.TryInitializeAsync(viewManager, mapping, request, cancellationToken, metadata);
        }

        public Task<bool>? TryCleanupAsync(IViewManager viewManager, IView view, object? state, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata) =>
            Components.TryCleanupAsync(viewManager, view, state, cancellationToken, metadata);

        #endregion
    }
}