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

        public Action<IComponentCollection, IList<TComponent>, IReadOnlyMetadataContext?>? DecorateHandler { get; set; }

        #endregion

        #region Methods

        protected override void Decorate(IComponentCollection collection, IList<TComponent> components, IReadOnlyMetadataContext? metadata)
        {
            if (DecorateHandler == null)
                base.Decorate(collection, components, metadata);
            else
                DecorateHandler.Invoke(collection, components, metadata);
        }

        #endregion
    }
}