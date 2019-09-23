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
    public sealed class GlobalValueConverter : ComponentOwnerBase<IGlobalValueConverter>, IGlobalValueConverter,
        IComponentOwnerAddedCallback<IComponent<IGlobalValueConverter>>, IComponentOwnerRemovedCallback<IComponent<IGlobalValueConverter>>
    {
        #region Fields

        private IGlobalValueConverterComponent[] _converters;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public GlobalValueConverter(IComponentCollectionProvider? componentCollectionProvider = null) : base(componentCollectionProvider)
        {
            _converters = Default.EmptyArray<IGlobalValueConverterComponent>();
        }

        #endregion

        #region Implementation of interfaces

        void IComponentOwnerAddedCallback<IComponent<IGlobalValueConverter>>.OnComponentAdded(IComponentCollection<IComponent<IGlobalValueConverter>> collection,
            IComponent<IGlobalValueConverter> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnAdded(ref _converters, this, collection, component, metadata);
        }

        void IComponentOwnerRemovedCallback<IComponent<IGlobalValueConverter>>.OnComponentRemoved(IComponentCollection<IComponent<IGlobalValueConverter>> collection,
            IComponent<IGlobalValueConverter> component, IReadOnlyMetadataContext? metadata)
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