using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Members
{
    public interface INotifiableAttachedBindingMemberInfo<in TTarget, TType> :
        IAttachedBindingMemberInfo<TTarget, TType>, INotifiableAttachedBindingMemberInfo
        where TTarget : class ?
    {
        bool Raise(TTarget target, object? message, IReadOnlyMetadataContext? metadata = null);
    }
}