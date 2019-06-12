using System;
using MugenMvvm.Binding.Interfaces.Observers;

namespace MugenMvvm.Binding.Interfaces.Members
{
    public interface IAttachedBindingMemberInfo<in TTarget, TType> : IBindingMemberInfo
        where TTarget : class ?
    {
        TType GetValue(TTarget target, object?[]? args);

        object? SetValue(TTarget target, TType value);

        object? SetValues(TTarget target, object?[]? args);

        IDisposable? TryObserve(TTarget target, IBindingEventListener listener);
    }
}