using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Components;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.Serialization.Components;

namespace MugenMvvm.Serialization
{
    public sealed class Serializer : ComponentOwnerBase<ISerializer>, ISerializer
    {
        #region Constructors

        public Serializer(IComponentCollectionManager? componentCollectionManager = null)
            : base(componentCollectionManager)
        {
        }

        #endregion

        #region Implementation of interfaces

        public bool IsSupported<TRequest, TResult>(ISerializationFormatBase<TRequest, TResult> format, IReadOnlyMetadataContext? metadata = null)
            => GetComponents<ISerializationManagerComponent>(metadata).IsSupported(this, format, metadata);

        public bool TrySerialize<TRequest, TResult>(ISerializationFormat<TRequest, TResult> format, TRequest request, [NotNullWhen(true)] [AllowNull] ref TResult result, IReadOnlyMetadataContext? metadata = null)
        {
            using var ctx = GetContext(format, request, metadata);
            return GetComponents<ISerializationManagerComponent>(metadata).TrySerialize(this, format, request, ctx, ref result);
        }

        public bool TryDeserialize<TRequest, TResult>(IDeserializationFormat<TRequest, TResult> format, TRequest request, [NotNullWhen(true)] [AllowNull] ref TResult result, IReadOnlyMetadataContext? metadata = null)
        {
            using var ctx = GetContext(format, request, metadata);
            return GetComponents<ISerializationManagerComponent>(metadata).TryDeserialize(this, format, request, ctx, ref result);
        }

        #endregion

        #region Methods

        private ISerializationContext GetContext<TRequest, TResult>(ISerializationFormatBase<TRequest, TResult> format, TRequest request, IReadOnlyMetadataContext? metadata) =>
            GetComponents<ISerializationContextProviderComponent>(metadata).TryGetSerializationContext(this, format, request, metadata) ?? new SerializationContext<TRequest, TResult>(format, request, metadata);

        #endregion
    }
}