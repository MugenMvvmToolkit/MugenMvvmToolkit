using System;
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

        public static IWrapperManagerComponent AddWrapper<TState>(this IWrapperManager wrapperManager, TState state, Func<Type, Type, TState, IReadOnlyMetadataContext?, bool> condition,
            Func<object, Type, TState, IReadOnlyMetadataContext?, object?> wrapperFactory, int priority = WrappingComponentPriority.WrapperManger, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(wrapperManager, nameof(wrapperManager));
            var factory = new DelegateWrapperManagerComponent<TState>(condition, wrapperFactory, state) {Priority = priority};
            wrapperManager.Components.Add(factory, metadata);
            return factory;
        }

        #endregion
    }
}