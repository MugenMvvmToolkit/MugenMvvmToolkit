using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;

namespace MugenMvvm.Views
{
    public sealed class ViewManager : ComponentOwnerBase<IViewManager>, IViewManager
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public ViewManager(IComponentCollectionProvider? componentCollectionProvider = null)
            : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Implementation of interfaces

        public IReadOnlyList<IViewInfo> GetViews(IViewModelBase viewModel, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            var components = GetComponents<IViewInfoProviderComponent>(metadata);
            if (components.Length == 0)
                return Default.EmptyArray<IViewInfo>();
            if (components.Length == 1)
                return components[0].GetViews(viewModel, metadata) ?? Default.EmptyArray<IViewInfo>();

            var result = new List<IViewInfo>();
            for (var i = 0; i < components.Length; i++)
            {
                var views = components[i].GetViews(viewModel, metadata);
                if (views != null && views.Count != 0)
                    result.AddRange(views);
            }

            return result;
        }

        public IReadOnlyList<IViewInitializer> GetInitializersByView(object view, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(view, nameof(view));
            var components = GetComponents<IViewInitializerProviderComponent>(metadata);
            if (components.Length == 0)
                return Default.EmptyArray<IViewInitializer>();

            var result = new List<IViewInitializer>();
            for (var i = 0; i < components.Length; i++)
            {
                var initializers = components[i].GetInitializersByView(view, metadata);
                if (initializers != null && initializers.Count != 0)
                    result.AddRange(initializers);
            }

            return result;
        }

        public IReadOnlyList<IViewInitializer> GetInitializersByViewModel(IViewModelBase viewModel, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            var components = GetComponents<IViewInitializerProviderComponent>(metadata);
            if (components.Length == 0)
                return Default.EmptyArray<IViewInitializer>();

            var result = new List<IViewInitializer>();
            for (var i = 0; i < components.Length; i++)
            {
                var initializers = components[i].GetInitializersByViewModel(viewModel, metadata);
                if (initializers != null && initializers.Count != 0)
                    result.AddRange(initializers);
            }

            return result;
        }

        #endregion
    }
}