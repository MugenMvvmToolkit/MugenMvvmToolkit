using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using MugenMvvm.Binding.Attributes;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Binding.Members.Descriptors;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Extensions
{
    public static partial class MugenBindingExtensions
    {
        #region Methods

        [IgnoreBindingMember]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BindableMembersTargetDescriptor<T> BindableMembers<T>(this T target) where T : class => new BindableMembersTargetDescriptor<T>(target);

        public static IMethodMemberInfo? TryGetMember<TTarget, TValue>(this BindableMethodDescriptor<TTarget, TValue> bindableMember, Type? type = null, MemberFlags flags = MemberFlags.All,
            IReadOnlyMetadataContext? metadata = null, IMemberManager? memberManager = null)
            where TTarget : class =>
            memberManager
                .DefaultIfNull()
                .TryGetMembers(type ?? typeof(TTarget), MemberType.Method, flags.SetInstanceOrStaticFlags(bindableMember.IsStatic), bindableMember.Request!, metadata)
                .SingleOrDefault<IMethodMemberInfo>();

        public static IAccessorMemberInfo? TryGetMember<TTarget, TValue>(this BindablePropertyDescriptor<TTarget, TValue> bindableMember,
            Type? type = null, MemberFlags flags = MemberFlags.All, IReadOnlyMetadataContext? metadata = null, IMemberManager? memberManager = null) where TTarget : class =>
            memberManager
                .DefaultIfNull()
                .TryGetMembers(type ?? typeof(TTarget), MemberType.Accessor, flags.SetInstanceOrStaticFlags(bindableMember.IsStatic), bindableMember.Name, metadata)
                .SingleOrDefault<IAccessorMemberInfo>();

        public static IObservableMemberInfo? TryGetMember<TTarget>(this BindableEventDescriptor<TTarget> bindableMember,
            Type? type = null, MemberFlags flags = MemberFlags.All, IReadOnlyMetadataContext? metadata = null, IMemberManager? memberManager = null) where TTarget : class =>
            memberManager
                .DefaultIfNull()
                .TryGetMembers(type ?? typeof(TTarget), MemberType.Event, flags.SetInstanceOrStaticFlags(bindableMember.IsStatic), bindableMember.Name, metadata)
                .SingleOrDefault<IObservableMemberInfo>();

        public static IMemberInfo? TryGetMember(this IMemberManager memberManager, Type type, MemberType memberTypes, MemberFlags flags, object request, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(memberManager, nameof(memberManager));
            return memberManager.TryGetMembers(type, memberTypes, flags, request, metadata).SingleOrDefault<IMemberInfo>();
        }

        public static TValue GetValue<TTarget, TValue>(this BindablePropertyDescriptor<TTarget, TValue> bindableMember, TTarget target,
            MemberFlags flags = MemberFlags.All, IReadOnlyMetadataContext? metadata = null, IMemberManager? memberManager = null) where TTarget : class
        {
            Should.NotBeNull(target, nameof(target));
            flags = flags.SetInstanceOrStaticFlags(bindableMember.IsStatic);
            return (TValue) memberManager.DefaultIfNull().GetValue(flags.GetTargetType(ref target!), target, bindableMember, flags, metadata)!;
        }

        public static void SetValue<TTarget, TValue>(this BindablePropertyDescriptor<TTarget, TValue> bindableMember, TTarget target,
            [MaybeNull] TValue value, MemberFlags flags = MemberFlags.All, IReadOnlyMetadataContext? metadata = null, IMemberManager? memberManager = null)
            where TTarget : class
        {
            Should.NotBeNull(target, nameof(target));
            var member = bindableMember.TryGetMember(GetTargetType(bindableMember.IsStatic, ref target!), flags, metadata, memberManager);
            if (member == null)
                BindingExceptionManager.ThrowInvalidBindingMember(target, bindableMember);
            member.SetValue(target, BoxingExtensions.Box(value), metadata);
        }

        public static TReturn Invoke<TTarget, TReturn>(this BindableMethodDescriptor<TTarget, TReturn> methodMember, TTarget target,
            object?[]? args = null, MemberFlags flags = MemberFlags.All, IReadOnlyMetadataContext? metadata = null, IMemberManager? memberManager = null) where TTarget : class
        {
            Should.NotBeNull(target, nameof(target));
            var method = methodMember.TryGetMember(GetTargetType(methodMember.IsStatic, ref target!), flags, metadata, memberManager);
            if (method == null)
                BindingExceptionManager.ThrowInvalidBindingMember(target, methodMember.ToString());
            return (TReturn) method.Invoke(target, args ?? Default.Array<object?>(), metadata)!;
        }

        public static TReturn Invoke<TTarget, TArg1, TReturn>(this BindableMethodDescriptor<TTarget, TArg1, TReturn> methodMember, TTarget target,
            TArg1 arg1, MemberFlags flags = MemberFlags.All, IReadOnlyMetadataContext? metadata = null, IMemberManager? memberManager = null) where TTarget : class =>
            methodMember.RawMethod.Invoke(target, new[] {BoxingExtensions.Box(arg1)}, flags, metadata, memberManager);

        public static TReturn Invoke<TTarget, TArg1, TArg2, TReturn>(this BindableMethodDescriptor<TTarget, TArg1, TArg2, TReturn> methodMember, TTarget target,
            TArg1 arg1, TArg2 arg2, MemberFlags flags = MemberFlags.All, IReadOnlyMetadataContext? metadata = null, IMemberManager? memberManager = null) where TTarget : class =>
            methodMember.RawMethod.Invoke(target, new[] {BoxingExtensions.Box(arg1), BoxingExtensions.Box(arg2)}, flags, metadata, memberManager);

        public static TReturn Invoke<TTarget, TArg1, TArg2, TArg3, TReturn>(this BindableMethodDescriptor<TTarget, TArg1, TArg2, TArg3, TReturn> methodMember, TTarget target,
            TArg1 arg1, TArg2 arg2, TArg3 arg3, MemberFlags flags = MemberFlags.All, IReadOnlyMetadataContext? metadata = null, IMemberManager? memberManager = null) where TTarget : class =>
            methodMember.RawMethod.Invoke(target, new[] {BoxingExtensions.Box(arg1), BoxingExtensions.Box(arg2), BoxingExtensions.Box(arg3)}, flags, metadata, memberManager);

        public static TReturn Invoke<TTarget, TArg1, TArg2, TArg3, TArg4, TReturn>(this BindableMethodDescriptor<TTarget, TArg1, TArg2, TArg3, TArg4, TReturn> methodMember, TTarget target,
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, MemberFlags flags = MemberFlags.All, IReadOnlyMetadataContext? metadata = null, IMemberManager? memberManager = null) where TTarget : class =>
            methodMember.RawMethod.Invoke(target, new[] {BoxingExtensions.Box(arg1), BoxingExtensions.Box(arg2), BoxingExtensions.Box(arg3), BoxingExtensions.Box(arg4)}, flags, metadata, memberManager);

        public static TReturn Invoke<TTarget, TArg1, TArg2, TArg3, TArg4, TArg5, TReturn>(this BindableMethodDescriptor<TTarget, TArg1, TArg2, TArg3, TArg4, TArg5, TReturn> methodMember, TTarget target,
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, MemberFlags flags = MemberFlags.All, IReadOnlyMetadataContext? metadata = null, IMemberManager? memberManager = null) where TTarget : class =>
            methodMember.RawMethod.Invoke(target, new[] {BoxingExtensions.Box(arg1), BoxingExtensions.Box(arg2), BoxingExtensions.Box(arg3), BoxingExtensions.Box(arg4), BoxingExtensions.Box(arg5)}, flags, metadata,
                memberManager);

        public static ActionToken Subscribe<TTarget>(this BindableEventDescriptor<TTarget> eventMember, TTarget target,
            IEventListener listener, MemberFlags flags = MemberFlags.All, IReadOnlyMetadataContext? metadata = null, IMemberManager? memberManager = null) where TTarget : class
        {
            Should.NotBeNull(target, nameof(target));
            var member = eventMember.TryGetMember(GetTargetType(eventMember.IsStatic, ref target!), flags, metadata, memberManager);
            if (member == null)
                BindingExceptionManager.ThrowInvalidBindingMember(target, eventMember);
            return member.TryObserve(target, listener, metadata);
        }

        public static ActionToken TryObserve<TTarget, TValue>(this BindablePropertyDescriptor<TTarget, TValue> bindableMember, TTarget target,
            IEventListener listener, MemberFlags flags = MemberFlags.All, IReadOnlyMetadataContext? metadata = null, IMemberManager? memberManager = null) where TTarget : class
        {
            Should.NotBeNull(target, nameof(target));
            var member = bindableMember.TryGetMember(GetTargetType(bindableMember.IsStatic, ref target!), flags, metadata, memberManager);
            if (member == null)
                BindingExceptionManager.ThrowInvalidBindingMember(target, bindableMember);
            return member.TryObserve(target, listener, metadata);
        }

        public static ActionToken TryObserve<TTarget, TReturn>(this BindableMethodDescriptor<TTarget, TReturn> methodMember, TTarget target,
            IEventListener listener, MemberFlags flags = MemberFlags.All, IReadOnlyMetadataContext? metadata = null, IMemberManager? memberManager = null) where TTarget : class
        {
            Should.NotBeNull(target, nameof(target));
            var member = methodMember.TryGetMember(GetTargetType(methodMember.IsStatic, ref target!), flags, metadata, memberManager);
            if (member == null)
                BindingExceptionManager.ThrowInvalidBindingMember(target, methodMember);
            return member.TryObserve(target, listener, metadata);
        }

        public static void TryRaise<TTarget, TValue>(this BindablePropertyDescriptor<TTarget, TValue> bindableMember, TTarget target, object? message = null,
            MemberFlags flags = MemberFlags.All, IReadOnlyMetadataContext? metadata = null, IMemberManager? memberManager = null) where TTarget : class
        {
            Should.NotBeNull(target, nameof(target));
            (bindableMember.TryGetMember(GetTargetType(bindableMember.IsStatic, ref target!), flags, metadata, memberManager) as INotifiableMemberInfo)?.Raise(target, message, metadata);
        }

        public static void TryRaise<TTarget>(this BindableEventDescriptor<TTarget> eventMember, TTarget target, object? message = null,
            MemberFlags flags = MemberFlags.All, IReadOnlyMetadataContext? metadata = null, IMemberManager? memberManager = null) where TTarget : class
        {
            Should.NotBeNull(target, nameof(target));
            (eventMember.TryGetMember(GetTargetType(eventMember.IsStatic, ref target!), flags, metadata, memberManager) as INotifiableMemberInfo)?.Raise(target, message, metadata);
        }

        public static void TryRaise<TTarget, TReturn>(this BindableMethodDescriptor<TTarget, TReturn> methodMember, TTarget target, object? message = null,
            MemberFlags flags = MemberFlags.All, IReadOnlyMetadataContext? metadata = null, IMemberManager? memberManager = null) where TTarget : class
        {
            Should.NotBeNull(target, nameof(target));
            (methodMember.TryGetMember(GetTargetType(methodMember.IsStatic, ref target!), flags, metadata, memberManager) as INotifiableMemberInfo)?.Raise(target, message, metadata);
        }

        internal static ActionToken TryObserve(this IObservableMemberInfo observableMember, IMemberInfo _, object? target, IEventListener listener, IReadOnlyMetadataContext? metadata) =>
            observableMember.TryObserve(target, listener, metadata);

        internal static void Raise(this INotifiableMemberInfo notifiableMember, IMemberInfo _, object? target, object? message, IReadOnlyMetadataContext? metadata) => notifiableMember.Raise(target, message, metadata);

        [return: MaybeNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TReturn? SingleOrDefault<TReturn>(this ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> itemOrList) where TReturn : class, IMemberInfo
        {
            if (itemOrList.List != null)
                BindingExceptionManager.ThrowAmbiguousMatchFound();
            return (TReturn?) itemOrList.Item;
        }

        #endregion
    }
}