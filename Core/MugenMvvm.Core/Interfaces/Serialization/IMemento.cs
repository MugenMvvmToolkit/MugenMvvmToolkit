using System;

namespace MugenMvvm.Interfaces.Serialization
{
    public interface IMemento
    {
        Type TargetType { get; }

        void Preserve(ISerializationContext serializationContext);

        IMementoResult Restore(ISerializationContext serializationContext);
    }
}