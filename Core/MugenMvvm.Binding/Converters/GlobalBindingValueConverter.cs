using System;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Interfaces.Converters;
using MugenMvvm.Binding.Interfaces.Converters.Components;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Converters
{
    public sealed class GlobalBindingValueConverter : ComponentOwnerBase<IGlobalBindingValueConverter>, IGlobalBindingValueConverter,
        IComponentOwnerAddedCallback<IComponent<IGlobalBindingValueConverter>>, IComponentOwnerRemovedCallback<IComponent<IGlobalBindingValueConverter>>
    {
        #region Fields

        private IGlobalBindingValueConverterComponent[] _converters;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public GlobalBindingValueConverter(IComponentCollectionProvider? componentCollectionProvider = null) : base(componentCollectionProvider)
        {
            _converters = Default.EmptyArray<IGlobalBindingValueConverterComponent>();
        }

        #endregion

        #region Implementation of interfaces

        void IComponentOwnerAddedCallback<IComponent<IGlobalBindingValueConverter>>.OnComponentAdded(IComponentCollection<IComponent<IGlobalBindingValueConverter>> collection,
            IComponent<IGlobalBindingValueConverter> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnAdded(ref _converters, this, collection, component, metadata);
        }

        void IComponentOwnerRemovedCallback<IComponent<IGlobalBindingValueConverter>>.OnComponentRemoved(IComponentCollection<IComponent<IGlobalBindingValueConverter>> collection,
            IComponent<IGlobalBindingValueConverter> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnRemoved(ref _converters, collection, component, metadata);
        }

        public object? Convert(object? value, Type targetType, IBindingMemberInfo? member = null, IReadOnlyMetadataContext? metadata = null)
        {
            if (_converters.Length == 0)
            {
                if (value == null)
                    return targetType.GetDefaultValue();
                if (targetType.IsInstanceOfTypeUnified(value))
                    return value;
                return System.Convert.ChangeType(value, targetType);
            }

            for (var i = 0; i < _converters.Length; i++)
                value = _converters[i].Convert(value, targetType, member, metadata);
            return value;
        }

        #endregion
    }
}