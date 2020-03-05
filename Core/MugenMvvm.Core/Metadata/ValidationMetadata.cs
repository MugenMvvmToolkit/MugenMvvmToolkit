using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Metadata
{
    public static class ValidationMetadata
    {
        #region Fields

        private static IMetadataContextKey<ICollection<string>, ICollection<string>>? _ignoredMembers;

        #endregion

        #region Properties

        [AllowNull]
        public static IMetadataContextKey<ICollection<string>, ICollection<string>> IgnoreMembers
        {
            get => _ignoredMembers ??= GetBuilder(_ignoredMembers, nameof(IgnoreMembers)).NotNull().Build();
            set => _ignoredMembers = value;
        }

        #endregion

        #region Methods

        private static MetadataContextKey.Builder<TGet, TSet> GetBuilder<TGet, TSet>(IMetadataContextKey<TGet, TSet>? _, string name)
        {
            return MetadataContextKey.Create<TGet, TSet>(typeof(ValidationMetadata), name);
        }

        #endregion
    }
}