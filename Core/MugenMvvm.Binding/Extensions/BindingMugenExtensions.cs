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

        public static TValue GetBindableMemberValue<TSource, TValue>(this TSource source,
            BindablePropertyDescriptor<TSource, TValue> bindableMember, TValue defaultValue = default, MemberFlags flags = MemberFlags.All,
            IReadOnlyMetadataContext? metadata = null, IMemberProvider? provider = null) where TSource : class
        {
            var propertyInfo = provider
                .ServiceIfNull()
                .GetMember(source.GetType(), bindableMember.Name, BindingMemberType.Property | BindingMemberType.Field, flags, metadata) as IBindingPropertyInfo;
            if (propertyInfo == null)
                return defaultValue;
            if (propertyInfo is IBindingPropertyInfo<TSource, TValue> p)
                return p.GetValue(source, metadata);
            return (TValue)propertyInfo.GetValue(source, metadata)!;
        }

        public static void SetBindableMemberValue<TSource, TValue>(this TSource source,
            BindablePropertyDescriptor<TSource, TValue> bindableMember, TValue value, bool throwOnError = true, MemberFlags flags = MemberFlags.All,
            IReadOnlyMetadataContext? metadata = null, IMemberProvider? provider = null) where TSource : class
        {
            var propertyInfo = provider
                .ServiceIfNull()
                .GetMember(source.GetType(), bindableMember.Name, BindingMemberType.Property | BindingMemberType.Field, flags, metadata) as IBindingPropertyInfo;
            if (propertyInfo == null)
            {
                if (throwOnError)
                    BindingExceptionManager.ThrowInvalidBindingMember(source.GetType(), bindableMember.Name);
                return;
            }

            if (propertyInfo is IBindingPropertyInfo<TSource, TValue> p)
                p.SetValue(source, value, metadata);
            else
                propertyInfo.SetValue(source, value, metadata);
        }

        public static Unsubscriber TryObserveBindableMember<TSource, TValue>(this TSource source,
            BindablePropertyDescriptor<TSource, TValue> bindableMember, IEventListener listener, MemberFlags flags = MemberFlags.All,
            IReadOnlyMetadataContext? metadata = null, IMemberProvider? provider = null) where TSource : class
        {
            var propertyInfo = provider
                .ServiceIfNull()
                .GetMember(source.GetType(), bindableMember.Name, BindingMemberType.Property | BindingMemberType.Field, flags, metadata) as IObservableBindingMemberInfo;
            if (propertyInfo == null)
                return default;
            return propertyInfo.TryObserve(source, listener, metadata);
        }

        public static Unsubscriber TrySubscribeBindableEvent<TSource>(this TSource source,
            BindableEventDescriptor<TSource> eventMember, IEventListener listener, MemberFlags flags = MemberFlags.All,
            IReadOnlyMetadataContext? metadata = null, IMemberProvider? provider = null) where TSource : class
        {
            var eventInfo = provider
                .ServiceIfNull()
                .GetMember(source.GetType(), eventMember.Name, BindingMemberType.Event, flags, metadata) as IBindingEventInfo;
            if (eventInfo == null)
                return default;
            return eventInfo.TrySubscribe(source, listener, metadata);
        }

        public static object? TryInvokeBindableMethod<TSource>(this TSource source,
            BindableMethodDescriptor<TSource> methodMember, object?[]? args = null, MemberFlags flags = MemberFlags.All,
            IReadOnlyMetadataContext? metadata = null, IMemberProvider? provider = null) where TSource : class
        {
            var methodInfo = provider
                .ServiceIfNull()
                .GetMember(source.GetType(), methodMember.Name, BindingMemberType.Method, flags, metadata) as IBindingMethodInfo;
            return methodInfo?.Invoke(source, args ?? Default.EmptyArray<object>());
        }

        public static WeakEventListener ToWeak(this IEventListener listener)
        {
            return new WeakEventListener(listener);
        }

        #endregion
    }
}