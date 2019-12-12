using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Members
{
    public interface IMemberAccessorInfo : IObservableMemberInfo
    {
        bool CanRead { get; }

        bool CanWrite { get; }

        object? GetValue(object? target, IReadOnlyMetadataContext? metadata = null);

        void SetValue(object? target, object? value, IReadOnlyMetadataContext? metadata = null);
    }

    public interface IMemberAccessorInfo<in TTarget, TValue> : IMemberAccessorInfo
    {
        TValue GetValue(TTarget target, IReadOnlyMetadataContext? metadata = null);

        void SetValue(TTarget target, TValue value, IReadOnlyMetadataContext? metadata = null);
    }
}