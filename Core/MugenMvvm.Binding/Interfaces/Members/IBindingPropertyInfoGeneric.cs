using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Members
{
    //todo fix all in for generic slow perf
    public interface IBindingPropertyInfo<in TSource, TValue> : IBindingPropertyInfo
    {
        TValue GetValue(TSource source, IReadOnlyMetadataContext? metadata = null);

        void SetValue(TSource source, TValue value, IReadOnlyMetadataContext? metadata = null);
    }
}