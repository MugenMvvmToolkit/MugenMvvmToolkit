using System.Threading;
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
using MugenMvvm.Views;

namespace MugenMvvm.Android.Views
{
    public sealed class AndroidViewRequestManager : ComponentDecoratorBase<IViewManager, IViewManagerComponent>, IViewManagerComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = ViewComponentPriority.ViewModelViewProviderDecorator + 1;

        #endregion

        #region Implementation of interfaces

        public Task<IView>? TryInitializeAsync(IViewManager viewManager, IViewMapping mapping, object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            if (mapping == ViewMapping.Undefined && request is AndroidViewRequest viewRequest && viewRequest.ViewModel != null && viewRequest.Container is Object container)
            {
                IAndroidViewMapping? viewMapping = null;
                foreach (var t in viewManager.GetMappings(viewRequest.ViewModel, metadata).Iterator())
                {
                    if (t is IAndroidViewMapping m && (viewRequest.ResourceId == 0 || m.ResourceId == viewRequest.ResourceId))
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
                        if (v.Mapping is IAndroidViewMapping m && m.ResourceId == resourceId)
                            return Task.FromResult(v);
                    }

                    var view = ViewExtensions.GetView(container, resourceId, true);
                    mapping ??= new AndroidViewMapping(resourceId, view.GetType(), viewRequest.ViewModel.GetType(), metadata);
                    viewRequest.View = view;
                }
            }

            return Components.TryInitializeAsync(viewManager, mapping, request, cancellationToken, metadata);
        }

        public Task? TryCleanupAsync(IViewManager viewManager, IView view, object? state, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata) =>
            Components.TryCleanupAsync(viewManager, view, state, cancellationToken, metadata);

        #endregion
    }
}