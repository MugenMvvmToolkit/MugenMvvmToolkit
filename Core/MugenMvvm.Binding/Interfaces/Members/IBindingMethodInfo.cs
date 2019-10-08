using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Members
{
    public interface IBindingMethodInfo : IObservableBindingMemberInfo
    {
        object? Invoke(object? source, object?[]? args, IReadOnlyMetadataContext? metadata = null);
    }
}