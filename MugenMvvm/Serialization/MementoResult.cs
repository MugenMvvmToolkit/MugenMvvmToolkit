using System.Runtime.InteropServices;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Serialization
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct MementoResult
    {
        public readonly bool IsRestored;
        public readonly object? Target;
        private readonly IReadOnlyMetadataContext? _metadata;

        public MementoResult(object target, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(target, nameof(target));
            Target = target;
            IsRestored = true;
            _metadata = metadata;
        }

        public MementoResult(bool isRestored, IReadOnlyMetadataContext? metadata = null)
        {
            Target = null;
            IsRestored = isRestored;
            _metadata = metadata;
        }

        public IReadOnlyMetadataContext Metadata => _metadata.DefaultIfNull();
    }
}