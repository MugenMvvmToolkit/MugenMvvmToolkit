using System;
using MugenMvvm.Attributes;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.BusyIndicator;
using MugenMvvm.Infrastructure.Messaging;
using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Interfaces.BusyIndicator;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Infrastructure;

namespace MugenMvvm.Infrastructure.ViewModels
{
    public class ServiceResolverViewModelDispatcherComponent : IServiceResolverViewModelDispatcherComponent
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public ServiceResolverViewModelDispatcherComponent()
        {
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        public void OnLifecycleChanged(IViewModelDispatcher viewModelDispatcher, IViewModelBase viewModel, ViewModelLifecycleState lifecycleState,
            IReadOnlyMetadataContext metadata)
        {
        }

        public object? TryGetService(IViewModelDispatcher viewModelDispatcher, IViewModelBase viewModel, Type service, IReadOnlyMetadataContext metadata)
        {
            if (service == typeof(IObservableMetadataContext))
                return new MetadataContext();
            if (service == typeof(IMessenger))
                return new Messenger(Service<IThreadDispatcher>.Instance);
            if (service == typeof(IBusyIndicatorProvider))
                return new BusyIndicatorProvider();
            return null;
        }

        #endregion
    }
}