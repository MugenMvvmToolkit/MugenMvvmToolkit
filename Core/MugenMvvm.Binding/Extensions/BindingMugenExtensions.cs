using System;
using System.Runtime.CompilerServices;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Converters;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Members.Descriptors;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;

// ReSharper disable once CheckNamespace
namespace MugenMvvm.Binding
{
    public static partial class BindingMugenExtensions
    {
        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFlagEx(this BindingMemberType value, BindingMemberType flag)
        {
            return (value & flag) == flag;
        }

        public static bool TryConvert(this IGlobalValueConverter? converter, object? value, Type targetType, IBindingMemberInfo? member, IReadOnlyMetadataContext? metadata,
            out object? result)
        {
            try
            {
                result = converter.ServiceIfNull().Convert(value, targetType, member, metadata);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        public static TValue GetBindableMemberValue<TTarget, TValue>(this TTarget target,
            BindablePropertyDescriptor<TTarget, TValue> bindableMember, TValue defaultValue = default, MemberFlags flags = MemberFlags.All,
            IReadOnlyMetadataContext? metadata = null, IMemberProvider? provider = null) where TTarget : class
        {
            var propertyInfo = provider
                .ServiceIfNull()
                .GetMember(target.GetType(), bindableMember.Name, BindingMemberType.Property | BindingMemberType.Field, flags, metadata) as IBindingMemberAccessorInfo;
            if (propertyInfo == null)
                return defaultValue;
            if (propertyInfo is IBindingMemberAccessorInfo<TTarget, TValue> p)
                return p.GetValue(target, metadata);
            return (TValue)propertyInfo.GetValue(target, metadata)!;
        }

        public static void SetBindableMemberValue<TTarget, TValue>(this TTarget target,
            BindablePropertyDescriptor<TTarget, TValue> bindableMember, TValue value, bool throwOnError = true, MemberFlags flags = MemberFlags.All,
            IReadOnlyMetadataContext? metadata = null, IMemberProvider? provider = null) where TTarget : class
        {
            var propertyInfo = provider
                .ServiceIfNull()
                .GetMember(target.GetType(), bindableMember.Name, BindingMemberType.Property | BindingMemberType.Field, flags, metadata) as IBindingMemberAccessorInfo;
            if (propertyInfo == null)
            {
                if (throwOnError)
                    BindingExceptionManager.ThrowInvalidBindingMember(target.GetType(), bindableMember.Name);
                return;
            }

            if (propertyInfo is IBindingMemberAccessorInfo<TTarget, TValue> p)
                p.SetValue(target, value, metadata);
            else
                propertyInfo.SetValue(target, value, metadata);
        }

        public static Unsubscriber TryObserveBindableMember<TTarget, TValue>(this TTarget target,
            BindablePropertyDescriptor<TTarget, TValue> bindableMember, IEventListener listener, MemberFlags flags = MemberFlags.All,
            IReadOnlyMetadataContext? metadata = null, IMemberProvider? provider = null) where TTarget : class
        {
            var propertyInfo = provider
                .ServiceIfNull()
                .GetMember(target.GetType(), bindableMember.Name, BindingMemberType.Property | BindingMemberType.Field, flags, metadata) as IObservableBindingMemberInfo;
            if (propertyInfo == null)
                return default;
            return propertyInfo.TryObserve(target, listener, metadata);
        }

        public static Unsubscriber TrySubscribeBindableEvent<TTarget>(this TTarget target,
            BindableEventDescriptor<TTarget> eventMember, IEventListener listener, MemberFlags flags = MemberFlags.All,
            IReadOnlyMetadataContext? metadata = null, IMemberProvider? provider = null) where TTarget : class
        {
            var eventInfo = provider
                .ServiceIfNull()
                .GetMember(target.GetType(), eventMember.Name, BindingMemberType.Event, flags, metadata) as IBindingEventInfo;
            if (eventInfo == null)
                return default;
            return eventInfo.TrySubscribe(target, listener, metadata);
        }

        public static object? TryInvokeBindableMethod<TTarget>(this TTarget target,
            BindableMethodDescriptor<TTarget> methodMember, object?[]? args = null, MemberFlags flags = MemberFlags.All,
            IReadOnlyMetadataContext? metadata = null, IMemberProvider? provider = null) where TTarget : class
        {
            var methodInfo = provider
                .ServiceIfNull()
                .GetMember(target.GetType(), methodMember.Name, BindingMemberType.Method, flags, metadata) as IBindingMethodInfo;
            return methodInfo?.Invoke(target, args ?? Default.EmptyArray<object>());
        }

        public static WeakEventListener ToWeak(this IEventListener listener)
        {
            return new WeakEventListener(listener);
        }

        #endregion
    }
}