using System;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Interfaces.Wrapping.Components;

namespace MugenMvvm.Wrapping.Components
{
    public sealed class DelegateWrapperManager<TConditionRequest, TWrapRequest> : IWrapperManagerComponent, IHasPriority
    {
        private readonly Func<Type, TConditionRequest, IReadOnlyMetadataContext?, bool> _condition;
        private readonly Func<Type, TWrapRequest, IReadOnlyMetadataContext?, object?> _wrapperFactory;

        public DelegateWrapperManager(Func<Type, TConditionRequest, IReadOnlyMetadataContext?, bool> condition,
            Func<Type, TWrapRequest, IReadOnlyMetadataContext?, object?> wrapperFactory)
        {
            Should.NotBeNull(condition, nameof(condition));
            Should.NotBeNull(wrapperFactory, nameof(wrapperFactory));
            _condition = condition;
            _wrapperFactory = wrapperFactory;
        }

        public int Priority { get; init; } = WrappingComponentPriority.WrapperManger;

        public bool CanWrap(IWrapperManager wrapperManager, Type wrapperType, object request, IReadOnlyMetadataContext? metadata)
        {
            if (request is TConditionRequest conditionRequest)
                return _condition.Invoke(wrapperType, conditionRequest, metadata);
            return false;
        }

        public object? TryWrap(IWrapperManager wrapperManager, Type wrapperType, object request, IReadOnlyMetadataContext? metadata)
        {
            if (request is TWrapRequest wrapRequest)
                return _wrapperFactory.Invoke(wrapperType, wrapRequest, metadata);
            return null;
        }
    }
}