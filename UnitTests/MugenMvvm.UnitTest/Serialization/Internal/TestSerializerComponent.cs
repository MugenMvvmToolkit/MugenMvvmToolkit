using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.Serialization.Components;

namespace MugenMvvm.UnitTest.Serialization.Internal
{
    public class TestSerializerComponent : ISerializerComponent, IHasPriority
    {
        #region Properties

        public Func<ISerializer, Stream, object, Type, ISerializationContext, bool>? TrySerialize { get; set; }

        public Func<ISerializer, Stream, ISerializationContext, object?>? TryDeserialize { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        bool ISerializerComponent.TrySerialize<TRequest>(ISerializer serializer, Stream stream, [DisallowNull] in TRequest request, ISerializationContext serializationContext)
        {
            return TrySerialize?.Invoke(serializer, stream, request, typeof(TRequest), serializationContext) ?? false;
        }

        bool ISerializerComponent.TryDeserialize(ISerializer serializer, Stream stream, ISerializationContext serializationContext, out object? value)
        {
            value = TryDeserialize?.Invoke(serializer, stream, serializationContext);
            return value != null;
        }

        #endregion
    }
}