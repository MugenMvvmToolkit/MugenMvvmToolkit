using System;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Members
{
    public interface IAttachedBindingMemberInfo<in TTarget, TType> : IBindingMemberInfo
        where TTarget : class ?
    {
        TType GetValue(TTarget target, object?[]? args, IReadOnlyMetadataContext? metadata = null);

        object? SetValue(TTarget target, TType value, IReadOnlyMetadataContext? metadata = null);

        object? SetValues(TTarget target, object?[]? args, IReadOnlyMetadataContext? metadata = null);

        IDisposable? TryObserve(TTarget target, IBindingEventListener listener, IReadOnlyMetadataContext? metadata = null);
    }
}