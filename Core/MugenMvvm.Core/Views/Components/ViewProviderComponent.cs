using System;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;

namespace MugenMvvm.Views.Components
{
    public sealed class ViewProviderComponent : AttachableComponentBase<IViewManager>, IViewProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IServiceProvider? _serviceProvider;

        #endregion

        #region Constructors

        public ViewProviderComponent(IServiceProvider? serviceProvider = null)
        {
            _serviceProvider = serviceProvider;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ViewComponentPriority.ViewProvider;

        #endregion

        #region Implementation of interfaces

        public object? TryGetViewForViewModel(IViewInitializer initializer, IViewModelBase viewModel, IMetadataContext metadata)
        {
            Should.NotBeNull(initializer, nameof(initializer));
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(metadata, nameof(metadata));
            return (_serviceProvider ?? MugenService.Instance<IServiceProvider>()).GetService(initializer.ViewType);
        }

        #endregion
    }
}