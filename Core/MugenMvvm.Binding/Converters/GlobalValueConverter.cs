using System;
using System.Globalization;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Interfaces.Converters;
using MugenMvvm.Binding.Interfaces.Converters.Components;
using MugenMvvm.Collections.Internal;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Converters
{
    public sealed class GlobalValueConverter : ComponentOwnerBase<IGlobalValueConverter>, IGlobalValueConverter
    {
        #region Fields

        private static readonly TypeLightDictionary<object?> DefaultValueCache = new TypeLightDictionary<object?>(23);

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public GlobalValueConverter(IComponentCollectionProvider? componentCollectionProvider = null) : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Properties

        public Func<IFormatProvider>? FormatProvider { get; set; }

        #endregion

        #region Implementation of interfaces

        public object? Convert(object? value, Type targetType, object? member = null, IReadOnlyMetadataContext? metadata = null)
        {
            var converters = GetComponents<IGlobalValueConverterComponent>(metadata);
            for (var i = 0; i < converters.Length; i++)
            {
                if (converters[i].TryConvert(ref value, targetType, member, metadata))
                    return value;
            }

            if (value == null)
                return GetDefaultValue(targetType);
            if (targetType.IsInstanceOfType(value))
                return value;
            if (targetType == typeof(string))
                return value.ToString();
            if (value is IConvertible)
                return System.Convert.ChangeType(value, targetType.GetNonNullableType(), FormatProvider?.Invoke() ?? CultureInfo.CurrentCulture);
            if (targetType.IsEnum)
                return Enum.Parse(targetType, value.ToString());
            return value;
        }

        #endregion

        #region Methods

        public static object? GetDefaultValue(Type type)
        {
            if (typeof(bool) == type)
                return BoxingExtensions.TrueObject;
            if (!typeof(ValueType).IsAssignableFrom(type))
                return null;
            if (!DefaultValueCache.TryGetValue(type, out var value))
            {
                value = Activator.CreateInstance(type);
                DefaultValueCache[type] = value;
            }

            return value;
        }

        #endregion
    }
}