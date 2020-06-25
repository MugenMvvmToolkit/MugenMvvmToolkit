using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Members
{
    public interface INotifiableMemberInfo : IObservableMemberInfo
    {
        void Raise<T>(object? target, in T message, IReadOnlyMetadataContext? metadata = null);
    }
}