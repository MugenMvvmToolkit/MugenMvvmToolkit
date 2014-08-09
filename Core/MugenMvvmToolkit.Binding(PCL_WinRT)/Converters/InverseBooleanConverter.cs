#region Copyright
// ****************************************************************************
// <copyright file="InverseBooleanConverter.cs">
// Copyright © Vyacheslav Volkov 2012-2014
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************
#endregion
using System;
using System.Globalization;

namespace MugenMvvmToolkit.Binding.Converters
{
    /// <summary>
    ///     This converter reverses a Boolean value (True == False, False == True).
    /// </summary>
    public sealed class InverseBooleanConverterCore : ValueConverterBase<bool?, bool?>
    {
        #region Fields

        /// <summary>
        ///     Gets the <see cref="InverseBooleanConverterCore" />.
        /// </summary>
        public static readonly InverseBooleanConverterCore Instance = new InverseBooleanConverterCore();

        #endregion

        #region Overrides of ValueConverterBase

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
        protected override bool? Convert(bool? value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.HasValue)
                return !value.Value;
            return null;
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
        protected override bool? ConvertBack(bool? value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType, parameter, culture);
        }

        #endregion
    }
}