#region Copyright

// ****************************************************************************
// <copyright file="InverseBooleanConverter.cs">
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
#if WINDOWS_UWP
using Windows.UI.Xaml.Data;
#else
using System.Globalization;
using System.Windows.Data;
#endif

#if WPF
namespace MugenMvvmToolkit.WPF.Binding.Converters
#elif WINDOWS_UWP
namespace MugenMvvmToolkit.UWP.Binding.Converters
#endif
{
    public sealed class InverseBooleanConverter : IValueConverter
    {
        #region Fields

        public static readonly InverseBooleanConverter Instance;

        #endregion

        #region Constructors

        static InverseBooleanConverter()
        {
            Instance = new InverseBooleanConverter();
        }

        #endregion

        #region Implementation of IValueConverter

#if WINDOWS_UWP
        public object Convert(object value, Type targetType, object parameter, string language)
#else
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
#endif

        {
            var b = (bool?)value;
            if (b == null)
                return null;
            return Empty.BooleanToObject(!b.Value);
        }

#if WINDOWS_UWP
        public object ConvertBack(object value, Type targetType, object parameter, string culture)
#else
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
#endif

        {
            return Convert(value, targetType, parameter, culture);
        }

        #endregion
    }
}
