using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTests.Components.Internal
{
    public class TestComponentDecorator<T, TComponent> : ComponentDecoratorBase<T, TComponent>, IHasPriority
        where T : class, IComponentOwner<T>
        where TComponent : class
    {
        #region Constructors

        public TestComponentDecorator() : base(0)
        {
        }

        #endregion

        #region Properties

        public new int Priority
        {
            get => base.Priority;
            set => base.Priority = value;
        }

        public new ItemOrArray<TComponent> Components => base.Components;

        public DecorateDelegate? DecorateHandler { get; set; }

        #endregion

        #region Methods

        protected override void Decorate(IComponentCollection collection, ref ItemOrListEditor<TComponent> components, IReadOnlyMetadataContext? metadata)
        {
            if (DecorateHandler == null)
                base.Decorate(collection, ref components, metadata);
            else
                DecorateHandler.Invoke(collection, ref components, metadata);
        }

        #endregion

        #region Nested types

        public delegate void DecorateDelegate(IComponentCollection collection, ref ItemOrListEditor<TComponent> components, IReadOnlyMetadataContext? metadata);

        #endregion
    }
}