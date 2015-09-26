#region Copyright

// ****************************************************************************
// <copyright file="BooleanToVisibilityConverter.cs">
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
    public sealed class BooleanToVisibilityConverter : IValueConverter
    {
        #region Fields

        private readonly object _trueValue;
        private readonly object _falseValue;
        private readonly object _nullValue;

        #endregion

        #region Constructors

        public BooleanToVisibilityConverter(Visibility trueValue, Visibility falseValue, Visibility nullValue)
        {
            _trueValue = trueValue;
            _falseValue = falseValue;
            _nullValue = nullValue;
        }

        #endregion

        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return _nullValue;

            if (((bool)value))
                return _trueValue;
            return _falseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            Should.BeOfType<Visibility>(value, "value");
            if (_trueValue.Equals(value))
                return Empty.TrueObject;
            if (_falseValue.Equals(value))
                return Empty.FalseObject;
            return null;
        }

        #endregion
    }
}
