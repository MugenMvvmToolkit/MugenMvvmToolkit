using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Requests;

namespace MugenMvvm.Views.Components
{
    public sealed class UndefinedMappingViewInitializer : ComponentDecoratorBase<IViewManager, IViewManagerComponent>, IViewManagerComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = ViewComponentPriority.ViewModelViewProviderDecorator + 1;

        #endregion

        #region Implementation of interfaces

        public Task<IView>? TryInitializeAsync(IViewManager viewManager, IViewMapping mapping, object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            if (mapping == ViewMapping.Undefined && request is ViewModelViewRequest r && r.ViewModel != null && r.View != null)
            {
                var mappings = viewManager.GetMappings(request, metadata);
                if (mappings.List == null)
                {
                    if (r.View is Type viewType)
                        r.View = null;
                    else
                        viewType = r.View.GetType();
                    var mappingId = mappings.Item?.Id ?? $"a{r.ViewModel.GetType().FullName}{viewType.FullName}";
                    foreach (var view in viewManager.GetViews(r.ViewModel, metadata).Iterator())
                    {
                        if (view.Mapping.Id == mappingId && (r.View == null || Equals(r.View, view.Target)))
                            return Task.FromResult(view);
                    }

                    mapping = mappings.Item ?? new ViewMapping(mappingId, viewType, r.ViewModel.GetType(), metadata);
                }
            }

            return Components.TryInitializeAsync(viewManager, mapping, request, cancellationToken, metadata);
        }

        public Task? TryCleanupAsync(IViewManager viewManager, IView view, object? state, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata) =>
            Components.TryCleanupAsync(viewManager, view, state, cancellationToken, metadata);

        #endregion
    }
}