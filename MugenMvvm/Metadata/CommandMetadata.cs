using System.Diagnostics.CodeAnalysis;
using System.Threading;
using MugenMvvm.Commands.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Metadata
{
    public static class CommandMetadata
    {
        public static readonly object RawCommandRequest = new();

        private static IMetadataContextKey<bool>? _forceExecute;
        private static IMetadataContextKey<CancellationTokenSource>? _synchronizerToken;
        private static IMetadataContextKey<CancellationTokenSource>? _executorToken;

        [AllowNull]
        public static IMetadataContextKey<bool> ForceExecute
        {
            get => _forceExecute ??= GetBuilder(_forceExecute, nameof(ForceExecute)).Build();
            set => _forceExecute = value;
        }

        [AllowNull]
        public static IMetadataContextKey<CancellationTokenSource> SynchronizerToken
        {
            get => _synchronizerToken ??= GetBuilder(_synchronizerToken, nameof(SynchronizerToken)).Build();
            set => _synchronizerToken = value;
        }

        [AllowNull]
        public static IMetadataContextKey<CancellationTokenSource> ExecutorToken
        {
            get => _executorToken ??= GetBuilder(_executorToken, nameof(ExecutorToken)).Build();
            set => _executorToken = value;
        }

        private static MetadataContextKey.Builder<T> GetBuilder<T>(IMetadataContextKey<T>? _, string name) => MetadataContextKey.Create<T>(typeof(CommandMetadata), name);
    }
}