using System;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Extensions.Components;
using MugenMvvm.Binding.Interfaces.Converters;
using MugenMvvm.Binding.Interfaces.Converters.Components;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Converters
{
    public sealed class GlobalValueConverter : ComponentOwnerBase<IGlobalValueConverter>, IGlobalValueConverter
    {
        #region Fields

        private readonly ComponentTracker _componentTracker;
        private IGlobalValueConverterComponent[]? _components;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public GlobalValueConverter(IComponentCollectionProvider? componentCollectionProvider = null) : base(componentCollectionProvider)
        {
            _componentTracker = new ComponentTracker();
            _componentTracker.AddListener<IGlobalValueConverterComponent, GlobalValueConverter>((components, state, _) => state._components = components, this);
        }

        #endregion

        #region Implementation of interfaces

        public object? Convert(object? value, Type targetType, object? member = null, IReadOnlyMetadataContext? metadata = null)
        {
            if (_components == null)
                _componentTracker.Attach(this, metadata);
            _components!.TryConvert(ref value, targetType, member, metadata);
            return value;
        }

        #endregion
    }
}