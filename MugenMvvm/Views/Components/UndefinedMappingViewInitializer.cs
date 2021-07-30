﻿using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Requests;

namespace MugenMvvm.Views.Components
{
    public sealed class UndefinedMappingViewInitializer : ComponentDecoratorBase<IViewManager, IViewManagerComponent>, IViewManagerComponent
    {
        public UndefinedMappingViewInitializer(int priority = ViewComponentPriority.UndefinedMappingDecorator) : base(priority)
        {
        }

        public ValueTask<IView?> TryInitializeAsync(IViewManager viewManager, IViewMapping mapping, object request, CancellationToken cancellationToken,
            IReadOnlyMetadataContext? metadata)
        {
            if (mapping.IsUndefined())
            {
                var viewModel = MugenExtensions.TryGetViewModelView(request, out object? v);
                if (viewModel != null)
                {
                    var mappings = viewManager.GetMappings(request, metadata);
                    if (mappings.List == null && (v != null || mappings.Item != null))
                    {
                        if (v is Type viewType)
                            v = null;
                        else
                            viewType = v?.GetType()!;
                        var mappingId = mappings.Item?.Id ?? $"a{viewModel.GetType().Name}{viewType.Name}";
                        foreach (var view in viewManager.GetViews(viewModel, metadata))
                        {
                            if (view.Mapping.Id == mappingId && (v == null || Equals(v, view.Target)))
                                return new ValueTask<IView?>(view);
                        }

                        mapping = mappings.Item ?? new ViewMapping(mappingId, viewModel.GetType(), viewType, metadata);
                    }

                    request = ViewModelViewRequest.GetRequestOrRaw(request, viewModel, v);
                }
            }

            return Components.TryInitializeAsync(viewManager, mapping, request, cancellationToken, metadata);
        }

        public Task<bool> TryCleanupAsync(IViewManager viewManager, IView view, object? state, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata) =>
            Components.TryCleanupAsync(viewManager, view, state, cancellationToken, metadata);
    }
}