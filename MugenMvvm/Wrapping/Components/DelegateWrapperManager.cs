using System;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Interfaces.Wrapping.Components;
using MugenMvvm.Internal;

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
            Func<Type, TWrapRequest, TState, IReadOnlyMetadataContext?, object?> wrapperFactory, [AllowNull]in TState state)
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

        public bool CanWrap<TRequest>(IWrapperManager wrapperManager, Type wrapperType, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            if (typeof(TRequest) == typeof(TConditionRequest) || !TypeChecker.IsValueType<TRequest>() && request is TConditionRequest)
                return _condition.Invoke(wrapperType, MugenExtensions.CastGeneric<TRequest, TConditionRequest>(request), _state, metadata);
            return false;
        }

        public object? TryWrap<TRequest>(IWrapperManager wrapperManager, Type wrapperType, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            if (typeof(TRequest) == typeof(TWrapRequest) || !TypeChecker.IsValueType<TRequest>() && request is TWrapRequest)
                return _wrapperFactory.Invoke(wrapperType, MugenExtensions.CastGeneric<TRequest, TWrapRequest>(request), _state, metadata);
            return null;
        }

        #endregion
    }
}