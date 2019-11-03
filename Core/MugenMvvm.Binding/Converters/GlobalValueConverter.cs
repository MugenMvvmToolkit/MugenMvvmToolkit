using System;
using System.Globalization;
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

        #region Properties

        public Func<IFormatProvider>? FormatProvider { get; set; }

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

        public object? Convert(object? value, Type targetType, object? member = null, IReadOnlyMetadataContext? metadata = null)
        {
            var converters = _converters;
            for (var i = 0; i < converters.Length; i++)
            {
                if (converters[i].TryConvert(ref value, targetType, member, metadata))
                    return value;
            }

            if (value == null)
                return targetType.GetDefaultValue();
            if (targetType.IsInstanceOfType(value))
                return value;
            if (value is IConvertible)
                return System.Convert.ChangeType(value, targetType.GetNonNullableType(), FormatProvider?.Invoke() ?? CultureInfo.CurrentCulture);
            if (targetType == typeof(string))
                return value.ToString();
            return value;
        }

        #endregion
    }
}