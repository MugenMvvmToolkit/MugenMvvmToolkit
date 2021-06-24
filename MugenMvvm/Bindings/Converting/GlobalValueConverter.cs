using System;
using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Converting.Components;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Extensions.Components;
using MugenMvvm.Bindings.Interfaces.Converting;
using MugenMvvm.Bindings.Interfaces.Converting.Components;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Converting
{
    public sealed class GlobalValueConverter : ComponentOwnerBase<IGlobalValueConverter>, IGlobalValueConverter, IHasComponentAddedHandler, IHasComponentRemovedHandler,
        IHasComponentChangedHandler
    {
        private readonly ComponentTracker _componentTracker;
        private DefaultGlobalValueConverter? _component;
        private ItemOrArray<IGlobalValueConverterComponent> _components;

        [Preserve(Conditional = true)]
        public GlobalValueConverter(IComponentCollectionManager? componentCollectionManager = null) : base(componentCollectionManager)
        {
            _componentTracker = new ComponentTracker();
            _componentTracker.AddListener<IGlobalValueConverterComponent, GlobalValueConverter>((components, state, _) =>
            {
                state._components = components;
                if (components.Count == 1 && components[0] is DefaultGlobalValueConverter c)
                    state._component = c;
                else
                    state._component = null;
            }, this);
        }

        public bool TryConvert(ref object? value, Type targetType, object? member, IReadOnlyMetadataContext? metadata)
        {
            var component = _component;
            if (component == null)
                return _components.TryConvert(this, ref value, targetType, member, metadata);
            return BindingMugenExtensions.TryConvert(ref value, targetType, component.FormatProvider);
        }

        void IHasComponentAddedHandler.OnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) =>
            _componentTracker.OnComponentChanged(collection, component, metadata);

        void IHasComponentChangedHandler.OnComponentChanged(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) =>
            _componentTracker.OnComponentChanged(collection, component, metadata);

        void IHasComponentRemovedHandler.OnComponentRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) =>
            _componentTracker.OnComponentChanged(collection, component, metadata);
    }
}