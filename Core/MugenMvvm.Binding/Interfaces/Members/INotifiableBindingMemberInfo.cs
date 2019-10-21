using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Members
{
    public interface INotifiableBindingMemberInfo : IBindingMemberInfo
    {
        bool Raise(object? target, object? message, IReadOnlyMetadataContext? metadata = null);
    }
}