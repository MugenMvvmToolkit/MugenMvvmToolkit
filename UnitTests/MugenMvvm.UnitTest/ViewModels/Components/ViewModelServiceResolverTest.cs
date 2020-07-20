using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Busy.Components;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Messaging.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTest.Metadata.Internal;
using MugenMvvm.UnitTest.ViewModels.Internal;
using MugenMvvm.ViewModels.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.ViewModels.Components
{
    public class ViewModelServiceResolverTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryGetServiceShouldReturnMetadataContext()
        {
            var component = new ViewModelServiceResolver();
            component.TryGetService(null!, new TestViewModel(), typeof(IMetadataContext), DefaultMetadata).ShouldBeType<MetadataContext>();
        }

        [Fact]
        public void TryGetServiceShouldReturnMessenger()
        {
            var component = new ViewModelServiceResolver();
            var service = (IMessenger)component.TryGetService(null!, new TestViewModel(), typeof(IMessenger), DefaultMetadata)!;
            service.GetComponent<IMessagePublisherComponent>().ShouldNotBeNull();
            service.GetComponent<IMessengerSubscriberComponent>().ShouldNotBeNull();
        }

        [Fact]
        public void TryGetServiceShouldReturnBusyManager()
        {
            var component = new ViewModelServiceResolver();
            var service = (IBusyManager)component.TryGetService(null!, new TestViewModel(), typeof(IBusyManager), DefaultMetadata)!;
            service.GetComponent<IBusyManagerComponent>().ShouldNotBeNull();
        }

        [Fact]
        public void TryGetServiceShouldReturnNullUnknownComponent()
        {
            var component = new ViewModelServiceResolver();
            component.TryGetService(null!, new TestViewModel(), typeof(object), DefaultMetadata).ShouldBeNull();
        }

        #endregion
    }
}