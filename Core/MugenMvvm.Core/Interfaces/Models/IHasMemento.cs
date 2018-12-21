using MugenMvvm.Interfaces.Serialization;

namespace MugenMvvm.Interfaces.Models
{
    public interface IHasMemento
    {
        IMemento? GetMemento();
    }
}