using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Interfaces.Members
{
    public interface IAccessorMemberInfo : IObservableMemberInfo
    {
        bool CanRead { get; }

        bool CanWrite { get; }

        object? GetValue(object? target, IReadOnlyMetadataContext? metadata = null);

        void SetValue(object? target, object? value, IReadOnlyMetadataContext? metadata = null);
    }
}