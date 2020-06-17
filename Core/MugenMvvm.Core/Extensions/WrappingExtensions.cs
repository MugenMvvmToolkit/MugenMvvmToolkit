using System;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Interfaces.Wrapping.Components;
using MugenMvvm.Wrapping.Components;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static object Wrap<TRequest>(this IWrapperManager wrapperManager, Type wrapperType, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(wrapperManager, nameof(wrapperManager));
            var wrapper = wrapperManager.TryWrap(wrapperType, request, metadata);
            if (wrapper == null)
                ExceptionManager.ThrowWrapperTypeNotSupported(wrapperType);
            return wrapper;
        }

        public static IWrapperManagerComponent AddWrapper<TConditionRequest, TWrapRequest, TState>(this IWrapperManager wrapperManager, in TState state,
            Func<Type, TConditionRequest, TState, IReadOnlyMetadataContext?, bool> condition,
            Func<Type, TWrapRequest, TState, IReadOnlyMetadataContext?, object?> wrapperFactory, int priority = WrappingComponentPriority.WrapperManger, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(wrapperManager, nameof(wrapperManager));
            var wrapper = new DelegateWrapperManager<TConditionRequest, TWrapRequest, TState>(condition, wrapperFactory, state) {Priority = priority};
            wrapperManager.Components.Add(wrapper, metadata);
            return wrapper;
        }

        #endregion
    }
}