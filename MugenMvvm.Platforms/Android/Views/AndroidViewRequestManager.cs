using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Java.Lang;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Android.Native;
using MugenMvvm.Android.Requests;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Internal;
using MugenMvvm.Requests;
using MugenMvvm.Views;

namespace MugenMvvm.Android.Views
{
    public sealed class AndroidViewRequestManager : ComponentDecoratorBase<IViewManager, IViewManagerComponent>, IViewManagerComponent, IViewProviderComponent,
        IComponentCollectionDecorator<IViewProviderComponent>, IHasPriority
    {
        #region Fields

        private IViewProviderComponent[] _viewProviders;

        #endregion

        #region Constructors

        public AndroidViewRequestManager()
        {
            _viewProviders = Default.Array<IViewProviderComponent>();
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ViewComponentPriority.ViewModelViewProviderDecorator + 1;

        #endregion

        #region Implementation of interfaces

        void IComponentCollectionDecorator<IViewProviderComponent>.Decorate(IComponentCollection collection, IList<IViewProviderComponent> components, IReadOnlyMetadataContext? metadata)
        {
            _viewProviders = this.Decorate(components);
        }

        public Task<IView>? TryInitializeAsync(IViewManager viewManager, IViewMapping mapping, object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            if (mapping != ViewMapping.Undefined || !(request is AndroidViewRequest viewRequest) || viewRequest.ViewModel == null || !(viewRequest.Container is Object container))
                return Components.TryInitializeAsync(viewManager, mapping, request, cancellationToken, metadata);

            IAndroidViewMapping? viewMapping = null;
            foreach (var t in viewManager.GetMappings(viewRequest.ViewModel, metadata).Iterator())
            {
                if (t is IAndroidViewMapping m && (viewRequest.ResourceId == 0 || m.ResourceId == viewRequest.ResourceId))
                {
                    viewMapping = m;
                    break;
                }
            }

            if (viewMapping == null && viewRequest.ResourceId == 0)
                return Components.TryInitializeAsync(viewManager, mapping, request, cancellationToken, metadata);

            var view = MugenAndroidNativeService.GetView(container, viewMapping?.ResourceId ?? viewRequest.ResourceId);
            viewMapping ??= new AndroidViewMapping(viewRequest.ResourceId, view.GetType(), viewRequest.ViewModel.GetType(), metadata);

            return Components.TryInitializeAsync(viewManager, viewMapping, ViewModelViewRequest.GetRequestOrRaw(request, viewRequest.ViewModel, view), cancellationToken, metadata);
        }

        public Task? TryCleanupAsync(IViewManager viewManager, IView view, object? state, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            return Components.TryCleanupAsync(viewManager, view, state, cancellationToken, metadata);
        }

        public ItemOrList<IView, IReadOnlyList<IView>> TryGetViews(IViewManager viewManager, object request, IReadOnlyMetadataContext? metadata)
        {
            if (!(request is AndroidViewRequest viewRequest) || viewRequest.ViewModel == null)
                return _viewProviders.TryGetViews(viewManager, request, metadata);

            var result = _viewProviders.TryGetViews(viewManager, viewRequest.ViewModel, metadata);
            if (viewRequest.ResourceId == 0)
                return result;

            foreach (var view in result.Iterator())
            {
                if (view.Mapping is IAndroidViewMapping mapping && mapping.ResourceId == viewRequest.ResourceId)
                    return ItemOrList.FromItem(view);
            }

            return default;
        }

        #endregion
    }
}