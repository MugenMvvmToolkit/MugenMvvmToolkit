using System;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Interfaces.Wrapping.Components;

namespace MugenMvvm.Wrapping.Components
{
    public sealed class DelegateWrapperManager<TConditionRequest, TWrapRequest, TState> : IWrapperManagerComponent, IHasPriority
    {
        #region Fields

        private readonly Func<Type, TConditionRequest, TState, IReadOnlyMetadataContext?, bool> _condition;

        [AllowNull]
        private readonly TState _state;

        private readonly Func<Type, TWrapRequest, TState, IReadOnlyMetadataContext?, object?> _wrapperFactory;

        #endregion

        #region Constructors

        public DelegateWrapperManager(Func<Type, TConditionRequest, TState, IReadOnlyMetadataContext?, bool> condition,
            Func<Type, TWrapRequest, TState, IReadOnlyMetadataContext?, object?> wrapperFactory, [AllowNull] TState state)
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

        public bool CanWrap(IWrapperManager wrapperManager, Type wrapperType, object request, IReadOnlyMetadataContext? metadata)
        {
            if (request is TConditionRequest conditionRequest)
                return _condition.Invoke(wrapperType, conditionRequest, _state, metadata);
            return false;
        }

        public object? TryWrap(IWrapperManager wrapperManager, Type wrapperType, object request, IReadOnlyMetadataContext? metadata)
        {
            if (request is TWrapRequest wrapRequest)
                return _wrapperFactory.Invoke(wrapperType, wrapRequest, _state, metadata);
            return null;
        }

        #endregion
    }
}