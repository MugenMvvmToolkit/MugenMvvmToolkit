using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Delegates
{
    public delegate ActionToken TryObserveDelegate<in TMember, in TTarget>(TMember member, TTarget target, IEventListener listener, IReadOnlyMetadataContext? metadata)
        where TMember : class, IMemberInfo;

    public delegate void MemberAttachedDelegate<in TMember, in TTarget>(TMember member, TTarget target, IReadOnlyMetadataContext? metadata)
        where TMember : class, IMemberInfo;

    public delegate void RaiseDelegate<in TMember, in TTarget>(TMember member, TTarget target, object? message, IReadOnlyMetadataContext? metadata)
        where TMember : class, IMemberInfo;

    public delegate TValue GetValueDelegate<in TMember, in TTarget, out TValue>(TMember member, TTarget target, IReadOnlyMetadataContext? metadata)
        where TMember : class, IMemberInfo;

    public delegate void SetValueDelegate<in TMember, in TTarget, in TValue>(TMember member, TTarget target, TValue value, IReadOnlyMetadataContext? metadata)
        where TMember : class, IMemberInfo;

    public delegate void ValueChangedDelegate<in TMember, in TTarget, in TValue>(TMember member, TTarget target, TValue oldValue, TValue newValue, IReadOnlyMetadataContext? metadata);

    public delegate TValue InvokeMethodDelegate<in TMember, in TTarget, out TValue>(TMember member, TTarget target, object?[] args, IReadOnlyMetadataContext? metadata);

    public delegate IAccessorMemberInfo? TryGetAccessorDelegate<in TMember>(TMember member, ArgumentFlags argumentFlags, object?[]? args, IReadOnlyMetadataContext? metadata);
}