using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
using MugenMvvm.Extensions.Internal;
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

        public Task<IView>? TryInitializeAsync<TRequest>(IViewManager viewManager, IViewMapping mapping, [DisallowNull] in TRequest request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            if (typeof(AndroidViewRequest) != typeof(TRequest) || mapping != ViewMapping.Undefined)
                return Components.TryInitializeAsync(viewManager, mapping, request, cancellationToken, metadata);

            var viewRequest = MugenExtensions.CastGeneric<TRequest, AndroidViewRequest>(request);
            var views = viewManager.GetViews(request, metadata);
            if (views.Item != null)
                return Task.FromResult(views.Item);

            IAndroidViewMapping? map = null;
            var mappings = viewManager.GetMappings(viewRequest.ViewModel, metadata);
            var count = mappings.Count();
            for (var i = 0; i < count; i++)
            {
                if (mappings.Get(i) is IAndroidViewMapping m && (viewRequest.ResourceId == 0 || m.ResourceId == viewRequest.ResourceId))
                {
                    map = m;
                    break;
                }
            }

            if (!(viewRequest.Container is Object container) || map == null && viewRequest.ResourceId == 0)
                return Components.TryInitializeAsync(viewManager, mapping, request, cancellationToken, metadata);

            var view = MugenAndroidNativeService.GetView(container, map?.ResourceId ?? viewRequest.ResourceId);
            map ??= new AndroidViewMapping(viewRequest.ResourceId, view.GetType(), viewRequest.ViewModel.GetType(), metadata);

            return Components.TryInitializeAsync(viewManager, map, new ViewModelViewRequest(viewRequest.ViewModel, view), cancellationToken, metadata);
        }

        public Task? TryCleanupAsync<TRequest>(IViewManager viewManager, IView view, [DisallowNull] in TRequest request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            return Components.TryCleanupAsync(viewManager, view, request, cancellationToken, metadata);
        }

        public ItemOrList<IView, IReadOnlyList<IView>> TryGetViews<TRequest>(IViewManager viewManager, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            if (typeof(AndroidViewRequest) != typeof(TRequest))
                return _viewProviders.TryGetViews(viewManager, request, metadata);

            var viewRequest = MugenExtensions.CastGeneric<TRequest, AndroidViewRequest>(request);
            var result = _viewProviders.TryGetViews(viewManager, viewRequest.ViewModel, metadata);
            if (viewRequest.ResourceId == 0)
                return result;

            var count = result.Count();
            for (var i = 0; i < count; i++)
            {
                var view = result.Get(i);
                if (view.Mapping is IAndroidViewMapping mapping && mapping.ResourceId == viewRequest.ResourceId)
                    return new ItemOrList<IView, IReadOnlyList<IView>>(view);
            }

            return default;
        }

        #endregion
    }
}