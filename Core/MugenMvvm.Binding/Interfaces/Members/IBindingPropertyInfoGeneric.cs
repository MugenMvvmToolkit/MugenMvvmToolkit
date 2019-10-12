using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Members
{
    public interface IBindingPropertyInfo<in TSource, TValue> : IBindingPropertyInfo
    {
        TValue GetValue(TSource source, IReadOnlyMetadataContext? metadata = null);

        void SetValue(TSource source, TValue value, IReadOnlyMetadataContext? metadata = null);
    }
}