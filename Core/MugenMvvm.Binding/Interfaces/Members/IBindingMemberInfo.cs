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

        object? GetValue(object? target, object?[]? args);

        object? SetValue(object? target, object? value);

        object? SetValues(object? target, object?[] args);

        IDisposable? TryObserve(object? target, IBindingEventListener listener);
    }
}