namespace MugenMvvm.Binding.Interfaces.Members
{
    public interface INotifiableAttachedBindingMemberInfo : IBindingMemberInfo
    {
        bool Raise(object? target, object? message);
    }
}