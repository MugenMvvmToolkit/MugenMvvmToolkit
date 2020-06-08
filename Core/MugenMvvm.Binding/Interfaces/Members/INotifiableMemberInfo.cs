using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Members
{
    public interface INotifiableMemberInfo : IObservableMemberInfo
    {
        void Raise(object? target, object? message, IReadOnlyMetadataContext? metadata = null);//todo message generic?
    }
}