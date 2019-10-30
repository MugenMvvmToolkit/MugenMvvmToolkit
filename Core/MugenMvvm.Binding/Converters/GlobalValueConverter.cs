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
        IComponentOwnerAddedCallback<IComponent<IGlobalValueConverter>>, IComponentOwnerRemovedCallback<IComponent<IGlobalValueConverter>>//todo review IConvertible
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
            MugenExtensions.ComponentTrackerOnAdded(ref _converters, collection, component);
        }

        void IComponentOwnerRemovedCallback<IComponent<IGlobalValueConverter>>.OnComponentRemoved(IComponentCollection<IComponent<IGlobalValueConverter>> collection,
            IComponent<IGlobalValueConverter> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnRemoved(ref _converters, component);
        }

        public object? Convert(object? value, Type targetType, IBindingMemberInfo? member = null, IReadOnlyMetadataContext? metadata = null)
        {
            var converters = _converters;
            if (converters.Length == 0)
            {
                if (value == null)
                    return targetType.GetDefaultValue();
                if (targetType.IsInstanceOfType(value))
                    return value;
                return System.Convert.ChangeType(value, targetType);
            }

            for (var i = 0; i < converters.Length; i++)
                value = converters[i].Convert(value, targetType, member, metadata);
            return value;
        }

        #endregion
    }
}