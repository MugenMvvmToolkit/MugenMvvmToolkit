using System;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Metadata
{
    public static class CommandMetadata
    {
        public static readonly object RawCommandRequest = new();

        private static IMetadataContextKey<bool>? _forceExecute;
        private static IMetadataContextKey<Func<ICompositeCommand?, IReadOnlyMetadataContext?, bool?>>? _canForceExecute;

        [AllowNull]
        public static IMetadataContextKey<Func<ICompositeCommand?, IReadOnlyMetadataContext?, bool?>> CanForceExecute
        {
            get => _canForceExecute ??= GetBuilder(_canForceExecute, nameof(CanForceExecute)).NotNull().Build();
            set => _canForceExecute = value;
        }

        [AllowNull]
        public static IMetadataContextKey<bool> ForceExecute
        {
            get => _forceExecute ??= GetBuilder(_forceExecute, nameof(ForceExecute)).Build();
            set => _forceExecute = value;
        }

        private static MetadataContextKey.Builder<T> GetBuilder<T>(IMetadataContextKey<T>? _, string name) => MetadataContextKey.Create<T>(typeof(CommandMetadata), name);
    }
}