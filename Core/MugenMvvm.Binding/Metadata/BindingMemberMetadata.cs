using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;

namespace MugenMvvm.Binding.Metadata
{
    public static class BindingMemberMetadata
    {
        #region Fields

        private static IMetadataContextKey<bool>? _ignoreAttachedMembers;

        #endregion

        #region Properties

        public static IMetadataContextKey<bool> IgnoreAttachedMembers
        {
            get => _ignoreAttachedMembers ??= GetBuilder<bool>(nameof(IgnoreAttachedMembers)).Build();
            set => _ignoreAttachedMembers = value;
        }

        #endregion

        #region Methods

        private static MetadataContextKey.Builder<T> GetBuilder<T>(string name)
        {
            return MetadataContextKey.Create<T>(typeof(BindingMemberMetadata), name);
        }

        #endregion
    }
}