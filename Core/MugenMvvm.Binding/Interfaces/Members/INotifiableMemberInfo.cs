using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Members
{
    public interface INotifiableMemberInfo : IMemberInfo
    {
        void Raise(object? target, object? message, IReadOnlyMetadataContext? metadata = null);
    }
}