namespace MugenMvvm.Interfaces.Messaging
{
    public interface IMessengerContext
    {
        IContext Metadata { get; }

        bool MarkAsHandled(object handler);
    }
}