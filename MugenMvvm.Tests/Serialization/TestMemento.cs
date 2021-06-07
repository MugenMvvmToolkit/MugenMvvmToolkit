using System;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Serialization;

namespace MugenMvvm.Tests.Serialization
{
    public class TestMemento : IMemento
    {
        public Action<ISerializationContext>? Preserve { get; set; }

        public Func<ISerializationContext, MementoResult>? Restore { get; set; }

        public Type TargetType { get; set; } = null!;

        void IMemento.Preserve(ISerializationContext serializationContext) => Preserve?.Invoke(serializationContext);

        MementoResult IMemento.Restore(ISerializationContext serializationContext) => Restore?.Invoke(serializationContext) ?? default;
    }
}