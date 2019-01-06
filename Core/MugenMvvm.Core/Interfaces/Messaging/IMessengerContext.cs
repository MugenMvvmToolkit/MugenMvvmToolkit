using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Messaging
{
    public interface IMessengerContext : IHasMetadata<IMetadataContext>
    {
        bool MarkAsHandled(object handler);
    }
}