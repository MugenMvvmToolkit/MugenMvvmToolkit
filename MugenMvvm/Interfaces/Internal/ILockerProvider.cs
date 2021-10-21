using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Internal
{
    public interface ILockerProvider
    {
        ILocker GetLocker(object target, IReadOnlyMetadataContext? metadata);
    }
}