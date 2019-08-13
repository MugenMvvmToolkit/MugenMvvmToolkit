using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Messaging.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Messaging.Components
{
    public class ViewModelMessengerSubscriberDecoratorComponent : IMessengerSubscriberDecoratorComponent, IHasPriority
    {
        #region Fields

        public static readonly ViewModelMessengerSubscriberDecoratorComponent Instance = new ViewModelMessengerSubscriberDecoratorComponent();

        #endregion

        #region Properties

        public int Priority { get; set; } = 1;

        #endregion

        #region Implementation of interfaces

        public object OnSubscribing(object subscriber, ThreadExecutionMode executionMode, IReadOnlyMetadataContext metadata)
        {
            if (subscriber is IViewModelBase vm)
            {
                var vmSubscriber = ViewModelMessengerSubscriber.TryGetSubscriber(vm, true);
                if (vmSubscriber != null)
                    return vmSubscriber;
            }

            return subscriber;
        }

        #endregion
    }
}