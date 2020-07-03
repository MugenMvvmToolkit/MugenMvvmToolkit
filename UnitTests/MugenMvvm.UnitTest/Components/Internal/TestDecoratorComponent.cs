using System;
using System.Collections.Generic;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Components.Internal
{
    public class TestComponentDecorator<T, TComponent> : ComponentDecoratorBase<T, TComponent>, IHasPriority
        where T : class, IComponentOwner<T>
        where TComponent : class
    {
        #region Properties

        public int Priority { get; set; }

        public new TComponent[] Components => base.Components;

        public Action<IComponentCollection, IList<TComponent>, IReadOnlyMetadataContext?>? Decorate { get; set; }

        #endregion

        #region Methods

        protected override void DecorateInternal(IComponentCollection collection, IList<TComponent> components, IReadOnlyMetadataContext? metadata)
        {
            if (Decorate == null)
                base.DecorateInternal(collection, components, metadata);
            else
                Decorate.Invoke(collection, components, metadata);
        }

        #endregion
    }
}