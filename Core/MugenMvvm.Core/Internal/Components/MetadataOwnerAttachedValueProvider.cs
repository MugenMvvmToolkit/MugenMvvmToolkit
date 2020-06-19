using MugenMvvm.Collections;
using MugenMvvm.Collections.Internal;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Metadata;

namespace MugenMvvm.Internal.Components
{
    public sealed class MetadataOwnerAttachedValueProvider : AttachedValueProviderBase, IHasPriority
    {
        #region Fields

        private static readonly IMetadataContextKey<LightDictionary<string, object?>, LightDictionary<string, object?>> Key = MetadataContextKey.FromMember(Key, typeof(MetadataOwnerAttachedValueProvider), nameof(Key));

        #endregion

        #region Properties

        public int Priority { get; set; } = InternalComponentPriority.MetadataOwnerAttachedValueProvider;

        #endregion

        #region Methods

        public override bool IsSupported(object item, IReadOnlyMetadataContext? metadata)
        {
            return item is IMetadataOwner<IMetadataContext>;
        }

        protected override LightDictionary<string, object?>? GetAttachedDictionary(object item, bool optional)
        {
            if (!optional)
                return ((IMetadataOwner<IMetadataContext>)item).Metadata.GetOrAdd(Key, this, (context, provider) => new StringOrdinalLightDictionary<object?>(3));
            var owner = (IMetadataOwner<IReadOnlyMetadataContext>)item;
            if (owner.HasMetadata)
                return owner.Metadata.Get(Key);
            return null;
        }

        protected override bool ClearInternal(object item)
        {
            var owner = (IMetadataOwner<IMetadataContext>)item;
            if (owner.HasMetadata)
                return owner.Metadata.Clear(Key, out _);
            return false;
        }

        #endregion
    }
}