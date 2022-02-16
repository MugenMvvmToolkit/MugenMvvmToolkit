using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Metadata
{
    public static class ApplicationMetadata
    {
        private static IMetadataContextKey<bool>? _isInBackground;

        public static int WaitLockerUpdateTimeout { get; set; } = 80;
        
        [AllowNull]
        public static IMetadataContextKey<bool> IsInBackground//todod mugen.observable
        {
            get => _isInBackground ??= GetBuilder(_isInBackground, nameof(IsInBackground)).Build();
            set => _isInBackground = value;
        }

        private static MetadataContextKey.Builder<T> GetBuilder<T>(IMetadataContextKey<T>? _, string name) => MetadataContextKey.Create<T>(typeof(ApplicationMetadata), name);
    }
}