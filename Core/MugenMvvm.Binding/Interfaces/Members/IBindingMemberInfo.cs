using System;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Observers;

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

        object? GetValue(object? source, object?[]? args);

        object? SetValue(object? source, object? value);

        object? SetValues(object? source, object?[] args);

        IDisposable? TryObserve(object? source, IBindingEventListener listener);
    }
}