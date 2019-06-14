using System;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Members
{
    public interface IBindingMemberInfo
    {
        string Name { get; }

        Type Type { get; }

        object? Member { get; }

        BindingMemberType MemberType { get; }

        bool CanRead { get; }

        bool CanWrite { get; }

        bool CanObserve { get; }

        object? GetValue(object? target, object?[]? args, IReadOnlyMetadataContext? metadata);

        object? SetValue(object? target, object? value, IReadOnlyMetadataContext? metadata);

        object? SetValues(object? target, object?[] args, IReadOnlyMetadataContext? metadata);

        IDisposable? TryObserve(object? target, IBindingEventListener listener, IReadOnlyMetadataContext? metadata);
    }
}