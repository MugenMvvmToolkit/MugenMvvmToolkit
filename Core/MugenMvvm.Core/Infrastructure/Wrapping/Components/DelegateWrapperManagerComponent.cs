using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Interfaces.Wrapping.Components;

namespace MugenMvvm.Infrastructure.Wrapping.Components
{
    public sealed class DelegateWrapperManagerComponent : IWrapperManagerComponent
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

        public bool CanWrap(IWrapperManager wrapperManager, Type type, Type wrapperType, IReadOnlyMetadataContext? metadata)
        {
            return Condition(wrapperManager, type, wrapperType, metadata);
        }

        public object? TryWrap(IWrapperManager wrapperManager, object item, Type wrapperType, IReadOnlyMetadataContext? metadata)
        {
            return WrapperFactory(wrapperManager, item, wrapperType, metadata);
        }

        int IComponent.GetPriority(object source)
        {
            return Priority;
        }

        #endregion
    }
}