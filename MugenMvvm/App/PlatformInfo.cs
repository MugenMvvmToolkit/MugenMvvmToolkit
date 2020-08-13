using MugenMvvm.Enums;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;

namespace MugenMvvm.App
{
    public class PlatformInfo : MetadataOwnerBase, IPlatformInfo
    {
        #region Constructors

        protected internal PlatformInfo(PlatformType type, IReadOnlyMetadataContext? metadata = null) : base(metadata)
        {
            Should.NotBeNull(type, nameof(type));
            Type = type;
        }

        #endregion

        #region Properties

        public PlatformType Type { get; }

        public virtual PlatformIdiom Idiom => PlatformIdiom.Unknown;

        public virtual string ApplicationVersion => "0.0";

        public virtual string DeviceVersion => "0.0";

        #endregion
    }
}