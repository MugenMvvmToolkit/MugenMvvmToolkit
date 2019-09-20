using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Metadata
{
    public static class ValidationMetadata
    {
        #region Fields

        private static IMetadataContextKey<ICollection<string>?> _ignoredMembers;

        #endregion

        #region Properties

        public static IMetadataContextKey<ICollection<string>?> IgnoredMembers
        {
            get => _ignoredMembers ??= GetBuilder<ICollection<string>?>(nameof(IgnoredMembers)).NotNull().Build();
            set => _ignoredMembers = value;
        }

        #endregion

        #region Methods

        private static MetadataContextKey.Builder<T> GetBuilder<T>(string name)
        {
            return MetadataContextKey.Create<T>(typeof(ValidationMetadata), name);
        }

        #endregion
    }
}