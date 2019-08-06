using System;
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

        public static TValue GetValueOrDefault<TSource, TValue>(this BindableMember<TSource, TValue> bindableMember, TSource source,
            object?[]? args = null, IReadOnlyMetadataContext? metadata = null, IBindingMemberProvider? provider = null)
            where TSource : class
        {
            return bindableMember.GetValueOrDefault(source, default!, args, metadata, provider);
        }

        public static TValue GetValueOrDefault<TSource, TValue>(this BindableMember<TSource, TValue> bindableMember, TSource source, TValue defaultValue,
            object?[]? args = null, IReadOnlyMetadataContext? metadata = null, IBindingMemberProvider? provider = null)
            where TSource : class
        {
            var member = bindableMember.GetMember(source, metadata, provider);
            if (member == null)
                return defaultValue!;
            if (member is IAttachedBindingMemberInfo<TSource, TValue> attached)
                return attached.GetValue(source, args, metadata);
            return (TValue)member.GetValue(source, args, metadata)!;
        }

        public static IDisposable? TryObserve<TSource, TValue>(this BindableMember<TSource, TValue> bindableMember, TSource source, IBindingEventListener listener,
            IReadOnlyMetadataContext? metadata = null, IBindingMemberProvider? provider = null)
            where TSource : class
        {
            return bindableMember.GetMember(source, metadata, provider)?.TryObserve(source, listener, metadata);
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