using System;
using MugenMvvm.Binding.Interfaces.Events;

namespace MugenMvvm.Binding.Interfaces.Members
{
    public interface IAttachedBindingMemberInfo<in TTarget, TType> : IBindingMemberInfo
        where TTarget : class ?
    {
        TType GetValue(TTarget source, object?[]? args);

        object? SetValue(TTarget source, TType value);

        object? SetValue(TTarget source, object?[]? args);

        IDisposable? TryObserve(TTarget source, IBindingEventListener listener);
    }
}