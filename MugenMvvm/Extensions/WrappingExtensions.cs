using System;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Interfaces.Wrapping.Components;
using MugenMvvm.Wrapping.Components;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static T? TryGetUnderlyingItem<T>(object target) where T : class
        {
            while (true)
            {
                if (target is T r)
                    return r;

                if (!(target is IWrapper<object> t))
                    return null;

                target = t.Target;
            }
        }

        public static object GetUnderlyingItem(object target)
        {
            while (true)
            {
                if (!(target is IWrapper<object> t))
                    return target;
                target = t.Target;
            }
        }

        public static object Wrap(this IWrapperManager wrapperManager, Type wrapperType, object request, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(wrapperManager, nameof(wrapperManager));
            var wrapper = wrapperManager.TryWrap(wrapperType, request, metadata);
            if (wrapper == null)
                ExceptionManager.ThrowWrapperTypeNotSupported(wrapperType);
            return wrapper;
        }

        public static IWrapperManagerComponent AddWrapper<TConditionRequest, TWrapRequest, TState>(this IWrapperManager wrapperManager, TState state,
            Func<Type, TConditionRequest, TState, IReadOnlyMetadataContext?, bool> condition,
            Func<Type, TWrapRequest, TState, IReadOnlyMetadataContext?, object?> wrapperFactory, int priority = WrappingComponentPriority.WrapperManger, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(wrapperManager, nameof(wrapperManager));
            var wrapper = new DelegateWrapperManager<TConditionRequest, TWrapRequest, TState>(condition, wrapperFactory, state) { Priority = priority };
            wrapperManager.Components.Add(wrapper, metadata);
            return wrapper;
        }

        #endregion
    }
}