using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Messaging
{
    public interface IMessengerContext
    {
        IMetadataContext Metadata { get; }

        bool MarkAsHandled(object handler);
    }
}