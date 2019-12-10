using System;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Wrapping.Components;

namespace MugenMvvm.Wrapping.Components
{
    public sealed class DelegateWrapperManagerComponent : IWrapperManagerComponent, IHasPriority
    {
        #region Fields

        public Func<Type, Type, IReadOnlyMetadataContext?, bool> Condition;
        public Func<object, Type, IReadOnlyMetadataContext?, object?> WrapperFactory;

        #endregion

        #region Constructors

        public DelegateWrapperManagerComponent(Func<Type, Type, IReadOnlyMetadataContext?, bool> condition, Func<object, Type, IReadOnlyMetadataContext?, object?> wrapperFactory)
        {
            Should.NotBeNull(condition, nameof(condition));
            Should.NotBeNull(wrapperFactory, nameof(wrapperFactory));
            Condition = condition;
            WrapperFactory = wrapperFactory;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = WrappingComponentPriority.WrapperManger;

        #endregion

        #region Implementation of interfaces

        public bool CanWrap(Type targetType, Type wrapperType, IReadOnlyMetadataContext? metadata)
        {
            return Condition(targetType, wrapperType, metadata);
        }

        public object? TryWrap(object target, Type wrapperType, IReadOnlyMetadataContext? metadata)
        {
            return WrapperFactory(target, wrapperType, metadata);
        }

        #endregion
    }
}