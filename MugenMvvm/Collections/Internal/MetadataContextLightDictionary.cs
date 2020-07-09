using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Collections.Internal
{
    internal sealed class MetadataContextLightDictionary : LightDictionary<IMetadataContextKey, object?>
    {
        #region Constructors

        public MetadataContextLightDictionary(int capacity) : base(capacity)
        {
        }

        #endregion

        #region Methods

        protected override bool Equals(IMetadataContextKey x, IMetadataContextKey y)
        {
            return ReferenceEquals(x, y) || x.Equals(y);
        }

        protected override int GetHashCode(IMetadataContextKey key)
        {
            return key.GetHashCode();
        }

        #endregion
    }
}