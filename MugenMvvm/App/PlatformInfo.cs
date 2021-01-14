using MugenMvvm.Enums;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;

namespace MugenMvvm.App
{
    public class PlatformInfo : MetadataOwnerBase, IPlatformInfo
    {
        public static readonly IPlatformInfo UnitTest = new PlatformInfo(PlatformType.UnitTest);

        protected internal PlatformInfo(PlatformType type, IReadOnlyMetadataContext? metadata = null) : base(metadata)
        {
            Should.NotBeNull(type, nameof(type));
            Type = type;
        }

        public virtual PlatformIdiom Idiom => PlatformIdiom.Unknown;

        public virtual string ApplicationVersion => "0.0";

        public virtual string DeviceVersion => "0.0";

        public PlatformType Type { get; }
    }
}