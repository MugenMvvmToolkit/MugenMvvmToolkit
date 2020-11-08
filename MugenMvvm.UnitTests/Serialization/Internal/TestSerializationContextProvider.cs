using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.Serialization.Components;
using Should;

namespace MugenMvvm.UnitTests.Serialization.Internal
{
    public class TestSerializationContextProvider : ISerializationContextProviderComponent, IHasPriority
    {
        #region Fields

        private readonly ISerializer? _serializer;

        #endregion

        #region Constructors

        public TestSerializationContextProvider(ISerializer? serializer = null)
        {
            _serializer = serializer;
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        public Func<ISerializationFormatBase, object, IReadOnlyMetadataContext?, ISerializationContext?>? TryGetSerializationContext { get; set; }

        #endregion

        #region Implementation of interfaces

        ISerializationContext? ISerializationContextProviderComponent.TryGetSerializationContext<TRequest, TResult>(ISerializer serializer, ISerializationFormatBase<TRequest, TResult> format, TRequest request,
            IReadOnlyMetadataContext? metadata)
        {
            _serializer?.ShouldEqual(serializer);
            return TryGetSerializationContext?.Invoke(format, request!, metadata);
        }

        #endregion
    }
}