using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Messaging;
using MugenMvvm.UnitTests.Metadata;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Messaging
{
    public class MessageContextTest : MetadataOwnerTestBase
    {
        protected override IMetadataOwner<IMetadataContext> GetMetadataOwner(IReadOnlyMetadataContext? metadata) => new MessageContext(this, this, metadata);

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            var sender = new object();
            var message = new object();
            var messageContext = new MessageContext(sender, message);
            messageContext.Sender.ShouldEqual(sender);
            messageContext.Message.ShouldEqual(message);
        }
    }
}