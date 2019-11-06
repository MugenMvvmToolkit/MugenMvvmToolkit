using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Members
{
    public interface IMemberAccessorInfo<in TTarget, TValue> : IMemberAccessorInfo
    {
        TValue GetValue(TTarget target, IReadOnlyMetadataContext? metadata = null);

        void SetValue(TTarget target, TValue value, IReadOnlyMetadataContext? metadata = null);
    }
}