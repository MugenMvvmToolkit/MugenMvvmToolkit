using System;
using MugenMvvm.Binding.Interfaces.Converters;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Metadata;

// ReSharper disable once CheckNamespace
namespace MugenMvvm.Binding
{
    public static partial class BindingMugenExtensions
    {
        #region Methods

        public static bool TryConvert(this IGlobalBindingValueConverter? converter, object? value, Type targetType, IBindingMemberInfo? member, IReadOnlyMetadataContext? metadata,
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

        public static TValue GetValueOrDefault<TSource, TValue>(this BindableMember<TSource, TValue> bindableMember, TSource source,
            IReadOnlyMetadataContext? metadata = null, IBindingMemberProvider? provider = null)
            where TSource : class
        {
            return bindableMember.GetValueOrDefault(source, default!, metadata, provider);
        }

        public static TValue GetValueOrDefault<TSource, TValue>(this BindableMember<TSource, TValue> bindableMember, TSource source, TValue defaultValue,
             IReadOnlyMetadataContext? metadata = null, IBindingMemberProvider? provider = null)
            where TSource : class
        {
            if (!(bindableMember.GetMember(source, metadata, provider) is IBindingPropertyInfo member))
                return defaultValue!;
            if (member is IBindingPropertyInfo<TSource, TValue> attached)
                return attached.GetValue(source, metadata);
            return (TValue)member.GetValue(source, metadata)!;
        }

        public static IDisposable? TryObserve<TSource, TValue>(this BindableMember<TSource, TValue> bindableMember, TSource source, IBindingEventListener listener,
            IReadOnlyMetadataContext? metadata = null, IBindingMemberProvider? provider = null)
            where TSource : class
        {
            return (bindableMember.GetMember(source, metadata, provider) as IObservableBindingMemberInfo)?.TryObserve(source, listener, metadata);
        }

        public static IBindingMemberInfo? GetMember<TSource, TValue>(this BindableMember<TSource, TValue> bindableMember, TSource source,
            IReadOnlyMetadataContext? metadata = null, IBindingMemberProvider? provider = null) where TSource : class
        {
            Should.NotBeNull(source, nameof(source));
            return provider.ServiceIfNull().GetMember(source.GetType(), bindableMember, metadata ?? Default.Metadata);
        }

        public static WeakBindingEventListener ToWeak(this IBindingEventListener listener)
        {
            return new WeakBindingEventListener(listener);
        }

        #endregion
    }
}