using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Interfaces.Wrapping.Components;

namespace MugenMvvm.Wrapping.Components
{
    public sealed class DelegateWrapperManagerComponent : IWrapperManagerComponent, IHasPriority
    {
        #region Fields

        public Func<IWrapperManager, Type, Type, IReadOnlyMetadataContext?, bool> Condition;
        public Func<IWrapperManager, object, Type, IReadOnlyMetadataContext?, object?> WrapperFactory;

        #endregion

        #region Constructors

        public DelegateWrapperManagerComponent(Func<IWrapperManager, Type, Type, IReadOnlyMetadataContext?, bool> condition,
            Func<IWrapperManager, object, Type, IReadOnlyMetadataContext?, object?> wrapperFactory)
        {
            Should.NotBeNull(condition, nameof(condition));
            Should.NotBeNull(wrapperFactory, nameof(wrapperFactory));
            Condition = condition;
            WrapperFactory = wrapperFactory;
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        public bool CanWrap(IWrapperManager wrapperManager, Type targetType, Type wrapperType, IReadOnlyMetadataContext? metadata)
        {
            return Condition(wrapperManager, targetType, wrapperType, metadata);
        }

        public object? TryWrap(IWrapperManager wrapperManager, object target, Type wrapperType, IReadOnlyMetadataContext? metadata)
        {
            return WrapperFactory(wrapperManager, target, wrapperType, metadata);
        }
        
        #endregion
    }
}