#region Copyright

// ****************************************************************************
// <copyright file="InverseBooleanConverter.cs">
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
#if NETFX_CORE || WINDOWSCOMMON
using Windows.UI.Xaml.Data;
#else
using System.Globalization;
using System.Windows.Data;
#endif

#if WPF
namespace MugenMvvmToolkit.WPF.Binding.Converters
#elif SILVERLIGHT
namespace MugenMvvmToolkit.Silverlight.Binding.Converters
#elif NETFX_CORE || WINDOWSCOMMON
namespace MugenMvvmToolkit.WinRT.Binding.Converters
#elif WINDOWS_PHONE
namespace MugenMvvmToolkit.WinPhone.Binding.Converters
#endif
{
    /// <summary>
    ///     This converter reverses a Boolean value (True == False, False == True).
    /// </summary>
    public sealed class InverseBooleanConverter : IValueConverter
    {
        #region Fields

        /// <summary>
        ///     Gets an instance of <see cref="InverseBooleanConverter" />.
        /// </summary>
        public static readonly InverseBooleanConverter Instance;

        #endregion

        #region Constructors

        static InverseBooleanConverter()
        {
            Instance = new InverseBooleanConverter();
        }

        #endregion

        #region Implementation of IValueConverter

#if NETFX_CORE || WINDOWSCOMMON
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

#if NETFX_CORE || WINDOWSCOMMON
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
