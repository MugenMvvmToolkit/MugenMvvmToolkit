using System;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Wrapping.Components;

namespace MugenMvvm.Wrapping.Components
{
    public sealed class DelegateWrapperManagerComponent<TState> : IWrapperManagerComponent, IHasPriority
    {
        #region Fields

        private readonly Func<Type, Type, TState, IReadOnlyMetadataContext?, bool> _condition;
        private readonly TState _state;
        private readonly Func<object, Type, TState, IReadOnlyMetadataContext?, object?> _wrapperFactory;

        #endregion

        #region Constructors

        public DelegateWrapperManagerComponent(Func<Type, Type, TState, IReadOnlyMetadataContext?, bool> condition,
            Func<object, Type, TState, IReadOnlyMetadataContext?, object?> wrapperFactory, TState state)
        {
            Should.NotBeNull(condition, nameof(condition));
            Should.NotBeNull(wrapperFactory, nameof(wrapperFactory));
            _condition = condition;
            _wrapperFactory = wrapperFactory;
            _state = state;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = WrappingComponentPriority.WrapperManger;

        #endregion

        #region Implementation of interfaces

        public bool CanWrap(Type targetType, Type wrapperType, IReadOnlyMetadataContext? metadata)
        {
            return _condition(targetType, wrapperType, _state, metadata);
        }

        public object? TryWrap(object target, Type wrapperType, IReadOnlyMetadataContext? metadata)
        {
            if (CanWrap(target.GetType(), wrapperType, metadata))
                return _wrapperFactory(target, wrapperType, _state, metadata);
            return null;
        }

        #endregion
    }
}