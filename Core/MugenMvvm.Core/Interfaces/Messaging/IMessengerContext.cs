using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Messaging
{
    public interface IMessengerContext : IMetadataOwner<IMetadataContext>
    {
        IMessenger Messenger { get; }

        bool MarkAsHandled(object handler);
    }
}