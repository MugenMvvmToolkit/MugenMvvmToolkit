#region Copyright

// ****************************************************************************
// <copyright file="NullToVisibilityConverter.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
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

#if WPF
namespace MugenMvvmToolkit.WPF.Binding.Converters
#elif SILVERLIGHT
namespace MugenMvvmToolkit.Silverlight.Binding.Converters
#elif WINDOWS_PHONE
namespace MugenMvvmToolkit.WinPhone.Binding.Converters
#endif
{
    /// <summary>
    ///     This converts a Boolean to a Visibility.  It supports mapping the conversions.
    /// </summary>
    public sealed class NullToVisibilityConverter : IValueConverter
    {
        #region Fields

        private readonly object _notNullValue;
        private readonly object _nullValue;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="NullToVisibilityConverter" /> class.
        /// </summary>
        public NullToVisibilityConverter(Visibility nullValue, Visibility notNullValue)
        {
            _notNullValue = notNullValue;
            _nullValue = nullValue;
        }

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
                return _nullValue;
            return _notNullValue;
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
            Should.MethodBeSupported(false, "ConvertBack()");
            return null;
        }

        #endregion
    }
}