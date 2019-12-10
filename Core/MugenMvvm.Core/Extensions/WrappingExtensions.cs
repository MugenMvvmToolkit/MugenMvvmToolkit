using System;
using System.Linq;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Interfaces.Wrapping.Components;
using MugenMvvm.Wrapping.Components;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static IWrapperManagerComponent AddWrapper(this IWrapperManager wrapperManager, Func<Type, Type, IReadOnlyMetadataContext?, bool> condition,
            Func<object, Type, IReadOnlyMetadataContext?, object?> wrapperFactory, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(wrapperManager, nameof(wrapperManager));
            var factory = new DelegateWrapperManagerComponent(condition, wrapperFactory);
            wrapperManager.Components.Add(factory, metadata);
            return factory;
        }

        public static IWrapperManagerComponent AddWrapper(this IWrapperManager wrapperManager, Type wrapperType, Type implementation,
            Func<object, Type, IReadOnlyMetadataContext?, object>? wrapperFactory = null,
            IReflectionDelegateProvider? reflectionDelegateProvider = null)
        {
            Should.NotBeNull(wrapperManager, nameof(wrapperManager));
            Should.NotBeNull(wrapperType, nameof(wrapperType));
            Should.BeOfType(implementation, nameof(implementation), wrapperType);
            if (implementation.IsInterface || implementation.IsAbstract)
                ExceptionManager.ThrowWrapperTypeShouldBeNonAbstract(implementation);

            if (wrapperFactory == null)
            {
                var constructor = implementation
                    .GetConstructors(BindingFlagsEx.InstanceOnly)
                    .FirstOrDefault(info => info.GetParameters().Length == 1);
                if (constructor == null)
                    ExceptionManager.ThrowCannotFindConstructor(implementation);

                wrapperFactory = constructor.GetActivator<Func<object, object>>(reflectionDelegateProvider).Invoke;
            }

            return wrapperManager.AddWrapper((_, requestedWrapperType, __) => wrapperType == requestedWrapperType, wrapperFactory); //todo closure check
        }

        public static IWrapperManagerComponent AddWrapper<TWrapper>(this IWrapperManager wrapperManager, Type implementation,
            Func<object, Type, IReadOnlyMetadataContext?, TWrapper>? wrapperFactory = null)
            where TWrapper : class
        {
            return wrapperManager.AddWrapper(typeof(TWrapper), implementation, wrapperFactory);
        }

        public static IWrapperManagerComponent AddWrapper<TWrapper, TImplementation>(this IWrapperManager wrapperManager,
            Func<object, Type, IReadOnlyMetadataContext?, TWrapper>? wrapperFactory = null)
            where TWrapper : class
            where TImplementation : class, TWrapper
        {
            return wrapperManager.AddWrapper(typeof(TWrapper), typeof(TImplementation), wrapperFactory);
        }

        private static object Invoke(this Func<object, object> activator, object target, Type _, IReadOnlyMetadataContext? __)
        {
            return activator(target);
        }

        #endregion
    }
}