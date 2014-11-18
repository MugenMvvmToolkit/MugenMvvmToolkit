using System;
using System.Globalization;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Test.TestModels
{
    public class ValueConverterCoreMock : IBindingValueConverter
    {
        #region Properties

        public Func<object, Type, object, CultureInfo, object> Convert { get; set; }

        public Func<object, Type, object, CultureInfo, object> ConvertBack { get; set; }

        #endregion

        #region Implementation of IBindingValueConverter

        /// <summary>
        ///     Converts a value.
        /// </summary>
        /// <returns>
        ///     A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <param name="context">The current context to use in the converter.</param>
        object IBindingValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture, IDataContext context)
        {
            return Convert(value, targetType, parameter, culture);
        }

        /// <summary>
        ///     Converts a value.
        /// </summary>
        /// <returns>
        ///     A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <param name="context">The current context to use in the converter.</param>
        object IBindingValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture, IDataContext context)
        {
            return ConvertBack(value, targetType, parameter, culture);
        }

        #endregion
    }
}