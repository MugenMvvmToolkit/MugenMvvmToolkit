using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Parsing;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

// ReSharper disable once CheckNamespace
namespace MugenMvvm.Bindings.Delegates
{
    public delegate ItemOrArray<BindingExpressionRequest> BindingBuilderDelegate<TTarget, TSource>(BindingBuilderTarget<TTarget, TSource> target)
        where TTarget : class
        where TSource : class;

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

    public delegate void ValueChangedDelegate<in TMember, in TTarget, in TValue>(TMember member, TTarget target, TValue oldValue, TValue newValue,
        IReadOnlyMetadataContext? metadata);

    public delegate TValue InvokeMethodDelegate<in TMember, in TTarget, out TValue>(TMember member, TTarget target, ItemOrArray<object?> args, IReadOnlyMetadataContext? metadata);

    public delegate IAccessorMemberInfo? TryGetAccessorDelegate<in TMember>(TMember member, EnumFlags<ArgumentFlags> argumentFlags, ItemOrIReadOnlyList<object?> args,
        IReadOnlyMetadataContext? metadata);
}