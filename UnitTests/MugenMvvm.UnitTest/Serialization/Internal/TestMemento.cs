using System;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Serialization;

namespace MugenMvvm.UnitTest.Serialization.Internal
{
    public class TestMemento : IMemento
    {
        #region Properties

        public Type TargetType { get; set; } = null!;

        public Action<ISerializationContext>? Preserve { get; set; }

        public Func<ISerializationContext, MementoResult>? Restore { get; set; }

        #endregion

        #region Implementation of interfaces

        void IMemento.Preserve(ISerializationContext serializationContext) => Preserve?.Invoke(serializationContext);

        MementoResult IMemento.Restore(ISerializationContext serializationContext) => Restore?.Invoke(serializationContext) ?? default;

        #endregion
    }
}