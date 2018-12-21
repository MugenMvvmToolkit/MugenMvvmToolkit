using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Serialization;

namespace MugenMvvm.Interfaces
{
    public interface IContextKey : IHasMemento
    {
        string Key { get; }

        void Validate(object? item);

        bool CanSerialize(object? item, ISerializationContext context);
    }

    public interface IContextKey<in T> : IContextKey
    {
    }
}