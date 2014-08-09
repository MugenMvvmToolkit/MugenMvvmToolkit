#region Copyright
// ****************************************************************************
// <copyright file="BooleanToVisibilityConverter.cs">
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
using System.Windows;
using System.Windows.Data;

namespace MugenMvvmToolkit.Binding.Converters
{
    /// <summary>
    ///     This converts a Boolean to a Visibility.  It supports mapping the conversions.
    /// </summary>
    public sealed class BooleanToVisibilityConverter : IValueConverter
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="BooleanToVisibilityConverter" /> class.
        /// </summary>
        public BooleanToVisibilityConverter(Visibility trueValue, Visibility falseValue, Visibility nullValue)
        {
            TrueValue = trueValue;
            FalseValue = falseValue;
            NullValue = nullValue;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Mapping for True to Visibility.
        /// </summary>
        public Visibility TrueValue { get; private set; }

        /// <summary>
        ///     Mapping for False to Visibility.
        /// </summary>
        public Visibility FalseValue { get; private set; }

        /// <summary>
        ///     Mapping for null to Visibility.
        /// </summary>
        public Visibility NullValue { get; private set; }

        #endregion

        #region Implementation of IValueConverter

        /// <summary>
        ///     Converts a value.
        /// </summary>
        /// <returns>
        ///     A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">
        ///     The value produced by the binding source.
        /// </param>
        /// <param name="targetType">
        ///     The type of the binding target property.
        /// </param>
        /// <param name="parameter">
        ///     The converter parameter to use.
        /// </param>
        /// <param name="culture">
        ///     The culture to use in the converter.
        /// </param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return NullValue;

            if (((bool)value))
                return TrueValue;
            return FalseValue;
        }

        /// <summary>
        ///     Converts a value.
        /// </summary>
        /// <returns>
        ///     A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">
        ///     The value that is produced by the binding target.
        /// </param>
        /// <param name="targetType">
        ///     The type to convert to.
        /// </param>
        /// <param name="parameter">
        ///     The converter parameter to use.
        /// </param>
        /// <param name="culture">
        ///     The culture to use in the converter.
        /// </param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            var visibility = (Visibility)value;
            if (visibility == TrueValue)
                return true;
            if (visibility == FalseValue)
                return false;
            return null;
        }

        #endregion
    }
}