using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Members
{
    public interface IBindingPropertyInfo<TTarget, TValue> : IBindingPropertyInfo
    {
        TValue GetValue(TTarget target, IReadOnlyMetadataContext? metadata = null);

        void SetValue(TTarget target, TValue value, IReadOnlyMetadataContext? metadata = null);
    }
}