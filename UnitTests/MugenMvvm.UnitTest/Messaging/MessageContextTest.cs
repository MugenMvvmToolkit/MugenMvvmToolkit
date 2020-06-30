using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Messaging;
using MugenMvvm.UnitTest.Metadata;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Messaging
{
    public class MessageContextTest : MetadataOwnerTestBase
    {
        #region Methods

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            var sender = new object();
            var message = new object();
            var messageContext = new MessageContext(sender, message);
            messageContext.Sender.ShouldEqual(sender);
            messageContext.Message.ShouldEqual(message);
        }

        protected override IMetadataOwner<IMetadataContext> GetMetadataOwner(IReadOnlyMetadataContext? metadata, IMetadataContextManager? metadataContextManager)
        {
            return new MessageContext(this, this, metadata, metadataContextManager);
        }

        #endregion
    }
}