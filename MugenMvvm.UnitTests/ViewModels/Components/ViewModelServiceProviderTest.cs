using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Busy.Components;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Messaging.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTests.ViewModels.Internal;
using MugenMvvm.ViewModels.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.ViewModels.Components
{
    public class ViewModelServiceProviderTest : UnitTestBase
    {
        [Fact]
        public void TryGetServiceShouldReturnBusyManager()
        {
            var component = new ViewModelServiceProvider();
            var service = (IBusyManager) component.TryGetService(null!, new TestViewModel(), typeof(IBusyManager), DefaultMetadata)!;
            service.GetComponent<IBusyManagerComponent>().ShouldNotBeNull();
        }

        [Fact]
        public void TryGetServiceShouldReturnMessenger()
        {
            var component = new ViewModelServiceProvider();
            var service = (IMessenger) component.TryGetService(null!, new TestViewModel(), typeof(IMessenger), DefaultMetadata)!;
            service.GetComponent<IMessagePublisherComponent>().ShouldNotBeNull();
            service.GetComponent<IMessengerSubscriberComponent>().ShouldNotBeNull();
        }

        [Fact]
        public void TryGetServiceShouldReturnMetadataContext()
        {
            var component = new ViewModelServiceProvider();
            component.TryGetService(null!, new TestViewModel(), typeof(IMetadataContext), DefaultMetadata).ShouldBeType<MetadataContext>();
        }

        [Fact]
        public void TryGetServiceShouldReturnNullUnknownComponent()
        {
            var component = new ViewModelServiceProvider();
            component.TryGetService(null!, new TestViewModel(), typeof(object), DefaultMetadata).ShouldBeNull();
        }
    }
}