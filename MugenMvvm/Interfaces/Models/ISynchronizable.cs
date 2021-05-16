using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Models
{
    public interface ISynchronizable
    {
        ActionToken Lock();
    }
}