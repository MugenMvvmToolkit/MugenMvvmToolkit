using System;
using MugenMvvm.Binding.Infrastructure.Members;
using MugenMvvm.Binding.Infrastructure.Observers;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Metadata;

// ReSharper disable once CheckNamespace
namespace MugenMvvm.Binding
{
    public static partial class BindingMugenExtensions
    {
        #region Methods

        public static TValue GetValueOrDefault<TSource, TValue>(this BindingMemberDescriptor<TSource, TValue> descriptor, TSource source,
            object?[]? args = null, IReadOnlyMetadataContext? metadata = null, IBindingMemberProvider? provider = null)
            where TSource : class
        {
            return descriptor.GetValueOrDefault(source, default!, args, metadata, provider);
        }

        public static TValue GetValueOrDefault<TSource, TValue>(this BindingMemberDescriptor<TSource, TValue> descriptor, TSource source, TValue defaultValue,
            object?[]? args = null, IReadOnlyMetadataContext? metadata = null, IBindingMemberProvider? provider = null)
            where TSource : class
        {
            var member = descriptor.GetMember(source, metadata, provider);
            if (member == null)
                return defaultValue!;
            if (member is IAttachedBindingMemberInfo<TSource, TValue> attached)
                return attached.GetValue(source, args, metadata);
            return (TValue)member.GetValue(source, args, metadata)!;
        }

        public static IDisposable? TryObserve<TSource, TValue>(this BindingMemberDescriptor<TSource, TValue> descriptor, TSource source, IBindingEventListener listener,
            IReadOnlyMetadataContext? metadata = null, IBindingMemberProvider? provider = null)
            where TSource : class
        {
            return descriptor.GetMember(source, metadata, provider)?.TryObserve(source, listener, metadata);
        }

        public static IBindingMemberInfo? GetMember<TSource, TValue>(this BindingMemberDescriptor<TSource, TValue> descriptor, TSource source,
            IReadOnlyMetadataContext? metadata = null, IBindingMemberProvider? provider = null) where TSource : class
        {
            Should.NotBeNull(source, nameof(source));
            return provider.ServiceIfNull().GetMember(source.GetType(), descriptor, metadata ?? Default.Metadata);
        }

        public static WeakBindingEventListener ToWeak(this IBindingEventListener listener)
        {
            return new WeakBindingEventListener(listener);
        }

        #endregion
    }
}