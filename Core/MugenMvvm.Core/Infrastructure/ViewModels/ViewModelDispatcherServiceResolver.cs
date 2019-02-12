using System;
using System.Collections.Generic;
using MugenMvvm.Infrastructure.BusyIndicator;
using MugenMvvm.Infrastructure.Messaging;
using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Interfaces;
using MugenMvvm.Interfaces.BusyIndicator;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Infrastructure;

namespace MugenMvvm.Infrastructure.ViewModels
{
    public class ViewModelDispatcherServiceResolver : IViewModelDispatcherServiceResolver
    {
        #region Constructors

        public ViewModelDispatcherServiceResolver()
        {
            Services = new[] { typeof(IObservableMetadataContext), typeof(IMessenger), typeof(IBusyIndicatorProvider) };
        }

        #endregion

        #region Properties

        public IReadOnlyList<Type> Services { get; }

        #endregion

        #region Implementation of interfaces

        public object Resolve(IViewModelBase viewModel, Type service, IReadOnlyMetadataContext metadata)
        {
            if (service == typeof(IObservableMetadataContext))
                return new MetadataContext();
            if (service == typeof(IMessenger))
                return new Messenger(Service<IThreadDispatcher>.Instance, Service<ITracer>.Instance);
            if (service == typeof(IBusyIndicatorProvider))
                return new BusyIndicatorProvider();
            throw new NotSupportedException();
        }

        #endregion
    }
}