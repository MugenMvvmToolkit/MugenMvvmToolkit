using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Members
{
    public interface IBindingPropertyInfo : IObservableBindingMemberInfo
    {
        bool CanRead { get; }

        bool CanWrite { get; }

        object? GetValue(object? source, IReadOnlyMetadataContext? metadata = null);

        void SetValue(object? source, object? value, IReadOnlyMetadataContext? metadata = null);
    }
}