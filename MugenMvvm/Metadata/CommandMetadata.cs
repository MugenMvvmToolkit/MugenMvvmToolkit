using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Metadata
{
    public static class CommandMetadata
    {
        public static readonly object RawCommandRequest = new();

        private static IMetadataContextKey<bool>? _forceExecute;

        [AllowNull]
        public static IMetadataContextKey<bool> ForceExecute
        {
            get => _forceExecute ??= GetBuilder(_forceExecute, nameof(ForceExecute)).Build();
            set => _forceExecute = value;
        }

        private static MetadataContextKey.Builder<T> GetBuilder<T>(IMetadataContextKey<T>? _, string name) => MetadataContextKey.Create<T>(typeof(CommandMetadata), name);
    }
}