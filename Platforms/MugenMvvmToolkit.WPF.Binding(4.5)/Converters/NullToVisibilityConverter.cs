#region Copyright

// ****************************************************************************
// <copyright file="NullToVisibilityConverter.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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

namespace MugenMvvmToolkit.WPF.Binding.Converters
{
    public sealed class NullToVisibilityConverter : IValueConverter
    {
        #region Fields

        private readonly object _notNullValue;
        private readonly object _nullValue;

        #endregion

        #region Constructors

        public NullToVisibilityConverter(Visibility nullValue, Visibility notNullValue)
        {
            _notNullValue = notNullValue;
            _nullValue = nullValue;
        }

        #endregion

        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return _nullValue;
            return _notNullValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Should.MethodBeSupported(false, "ConvertBack()");
            return null;
        }

        #endregion
    }
}
