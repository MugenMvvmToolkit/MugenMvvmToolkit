using System;
using System.IO;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.Serialization.Components;
using Should;

namespace MugenMvvm.UnitTest.Serialization.Internal
{
    public class TestSerializerComponent : ISerializerComponent, IHasPriority
    {
        #region Fields

        private readonly ISerializer? _serializer;

        #endregion

        #region Constructors

        public TestSerializerComponent(ISerializer? serializer = null)
        {
            _serializer = serializer;
        }

        #endregion

        #region Properties

        public Func<object, ISerializationContext, bool>? TrySerialize { get; set; }

        public Func<ISerializationContext, object?>? TryDeserialize { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        bool ISerializerComponent.TrySerialize(ISerializer serializer, object request, ISerializationContext serializationContext)
        {
            _serializer?.ShouldEqual(serializer);
            return TrySerialize?.Invoke(request, serializationContext) ?? false;
        }

        bool ISerializerComponent.TryDeserialize(ISerializer serializer, ISerializationContext serializationContext, out object? value)
        {
            _serializer?.ShouldEqual(serializer);
            value = TryDeserialize?.Invoke(serializationContext);
            return value != null;
        }

        #endregion
    }
}