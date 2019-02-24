using System;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Messaging;
using MugenMvvm.Interfaces.BusyIndicator;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Infrastructure;
using MugenMvvm.Metadata;

namespace MugenMvvm.Infrastructure.ViewModels
{
    public class SubscriberViewModelDispatcherListener : IViewModelDispatcherListener
    {
        #region Properties

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        public IViewModelBase? TryGetViewModel(IViewModelDispatcher viewModelDispatcher, Guid id, IReadOnlyMetadataContext metadata)
        {
            return null;
        }

        public bool OnSubscribe(IViewModelDispatcher viewModelDispatcher, IViewModelBase viewModel, object observer, ThreadExecutionMode executionMode,
            IReadOnlyMetadataContext metadata)
        {
            var messenger = viewModel.TryGetService<IMessenger>();
            if (messenger == null)
                return false;

            var result = false;

            if (observer is IViewModelBase targetVm)
            {
                var messengerSubscriber = ViewModelMessengerSubscriber.TryGetSubscriber(targetVm, true);
                if (messengerSubscriber != null)
                {
                    SubscribeBusyTokens(viewModel, targetVm);
                    messenger.Subscribe(messengerSubscriber, executionMode);
                    result = true;
                }
            }

            if (observer is IMessengerSubscriber subscriber)
            {
                messenger.Subscribe(subscriber, executionMode);
                result = true;
            }
            else if (observer is IMessengerHandler handler)
            {
                messenger.Subscribe(handler, executionMode);
                result = true;
            }

            return result;
        }

        public bool OnUnsubscribe(IViewModelDispatcher viewModelDispatcher, IViewModelBase viewModel, object observer, IReadOnlyMetadataContext metadata)
        {
            var messenger = viewModel.TryGetServiceOptional<IMessenger>();
            if (messenger == null)
                return false;

            var result = false;
            if (observer is IViewModelBase targetVm)
            {
                var messengerSubscriber = ViewModelMessengerSubscriber.TryGetSubscriber(targetVm, false);
                if (messengerSubscriber != null && messenger.Unsubscribe(messengerSubscriber))
                    result = true;
            }

            if (observer is IMessengerSubscriber subscriber)
            {
                if (messenger.Unsubscribe(subscriber))
                    result = true;
            }
            else if (observer is IMessengerHandler handler)
            {
                if (messenger.Unsubscribe(new MessengerHandlerSubscriber(handler)))
                    result = true;
            }

            return result;
        }

        public void OnLifecycleChanged(IViewModelDispatcher viewModelDispatcher, IViewModelBase viewModel, ViewModelLifecycleState lifecycleState,
            IReadOnlyMetadataContext metadata)
        {
        }

        public int GetPriority(object source)
        {
            return Priority;
        }

        #endregion

        #region Methods

        private void SubscribeBusyTokens(IViewModelBase viewModel, IViewModelBase targetVm)
        {
            if (!targetVm.Metadata.Get(ViewModelMetadata.BusyMessageHandlerType).HasFlagEx(BusyMessageHandlerType.Handle))
                return;

            var srcBusy = viewModel.TryGetServiceOptional<IBusyIndicatorProvider>();
            if (srcBusy == null)
                return;

            var tokens = srcBusy.GetTokens();
            if (tokens.Count == 0)
                return;

            var targetBusy = targetVm.TryGetService<IBusyIndicatorProvider>();
            if (targetBusy == null)
                return;

            for (var index = 0; index < tokens.Count; index++)
                targetBusy.Begin(tokens[index]);
        }

        #endregion
    }
}