using System;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.Serialization.Components;
using Should;

namespace MugenMvvm.UnitTests.Serialization.Internal
{
    public class TestSerializationManagerComponent : ISerializationManagerComponent, IHasPriority
    {
        #region Fields

        private readonly ISerializer? _serializer;

        #endregion

        #region Constructors

        public TestSerializationManagerComponent(ISerializer? serializer = null)
        {
            _serializer = serializer;
        }

        #endregion

        #region Properties

        public Func<ISerializationFormatBase, IReadOnlyMetadataContext?, bool>? IsSupported { get; set; }

        public Func<object, ISerializationContext, object?>? TrySerialize { get; set; }

        public Func<object, ISerializationContext, object?>? TryDeserialize { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        bool ISerializationManagerComponent.IsSupported<TRequest, TResult>(ISerializer serializer, ISerializationFormatBase<TRequest, TResult> format, IReadOnlyMetadataContext? metadata)
        {
            _serializer?.ShouldEqual(serializer);
            return IsSupported?.Invoke(format, metadata) ?? false;
        }

        bool ISerializationManagerComponent.TrySerialize<TRequest, TResult>(ISerializer serializer, ISerializationFormat<TRequest, TResult> format, TRequest request, ISerializationContext serializationContext,
            [NotNullWhen(true)] [AllowNull] ref TResult result)
        {
            _serializer?.ShouldEqual(serializer);
            result = (TResult) TrySerialize?.Invoke(request!, serializationContext)!;
            return result != null;
        }

        bool ISerializationManagerComponent.TryDeserialize<TRequest, TResult>(ISerializer serializer, IDeserializationFormat<TRequest, TResult> format, TRequest request, ISerializationContext serializationContext,
            [NotNullWhen(true)] [AllowNull] ref TResult result)
        {
            _serializer?.ShouldEqual(serializer);
            result = (TResult) TryDeserialize?.Invoke(request!, serializationContext)!;
            return result != null;
        }

        #endregion
    }
}