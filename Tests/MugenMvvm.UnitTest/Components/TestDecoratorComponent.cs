using System;
using System.Collections.Generic;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Components
{
    public class TestDecoratorComponent<T, TComponent> : DecoratorComponentBase<T, TComponent>, IHasPriority
        where T : class, IComponentOwner<T>
        where TComponent : class
    {
        #region Properties

        public int Priority { get; set; }

        public new TComponent[] Components => base.Components;

        public Action<IList<TComponent>, IReadOnlyMetadataContext?> Decorate { get; set; }

        #endregion

        #region Methods

        protected override void DecorateInternal(IList<TComponent> components, IReadOnlyMetadataContext? metadata)
        {
            if (Decorate == null)
                base.DecorateInternal(components, metadata);
            else
                Decorate.Invoke(components, metadata);
        }

        #endregion
    }
}