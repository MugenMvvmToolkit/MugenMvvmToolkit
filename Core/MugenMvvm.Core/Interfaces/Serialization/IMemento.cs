using System;
using MugenMvvm.Serialization;

namespace MugenMvvm.Interfaces.Serialization
{
    public interface IMemento
    {
        Type TargetType { get; }

        void Preserve(ISerializationContext serializationContext);

        MementoResult Restore(ISerializationContext serializationContext);
    }
}