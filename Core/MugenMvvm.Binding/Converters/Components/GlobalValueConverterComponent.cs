using System;
using System.Globalization;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Converters.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Converters.Components
{
    public sealed class GlobalValueConverterComponent : IGlobalValueConverterComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = ConverterComponentPriority.Converter;

        public Func<IFormatProvider>? FormatProvider { get; set; }

        #endregion

        #region Implementation of interfaces

        public bool TryConvert(ref object? value, Type targetType, object? member, IReadOnlyMetadataContext? metadata)
        {
            if (value == null)
                value = targetType.GetDefaultValue();
            else if (targetType.IsInstanceOfType(value))
                return true;
            else if (targetType == typeof(string))
                value = value.ToString();
            else if (value is IConvertible)
                value = Convert.ChangeType(value, targetType.GetNonNullableType(), FormatProvider?.Invoke() ?? CultureInfo.CurrentCulture);
            else if (targetType.IsEnum)
                value = Enum.Parse(targetType, value.ToString());

            return true;
        }

        #endregion
    }
}