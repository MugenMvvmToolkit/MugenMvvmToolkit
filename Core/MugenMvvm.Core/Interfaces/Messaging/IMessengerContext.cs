using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Messaging
{
    public interface IMessengerContext : IMetadataOwner<IMetadataContext>
    {
        IMessenger Messenger { get; }

        bool MarkAsHandled(object handler);
    }
}