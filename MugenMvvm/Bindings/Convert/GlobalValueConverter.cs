using System;
using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Extensions.Components;
using MugenMvvm.Bindings.Interfaces.Convert;
using MugenMvvm.Bindings.Interfaces.Convert.Components;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Convert
{
    public sealed class GlobalValueConverter : ComponentOwnerBase<IGlobalValueConverter>, IGlobalValueConverter
    {
        #region Fields

        private readonly ComponentTracker _componentTracker;
        private IGlobalValueConverterComponent[]? _components;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public GlobalValueConverter(IComponentCollectionManager? componentCollectionManager = null) : base(componentCollectionManager)
        {
            _componentTracker = new ComponentTracker();
            _componentTracker.AddListener<IGlobalValueConverterComponent, GlobalValueConverter>((components, state, _) => state._components = components, this);
        }

        #endregion

        #region Implementation of interfaces

        public bool TryConvert(ref object? value, Type targetType, object? member, IReadOnlyMetadataContext? metadata)
        {
            if (_components == null)
                _componentTracker.Attach(this, metadata);
            return _components!.TryConvert(this, ref value, targetType, member, metadata);
        }

        #endregion
    }
}