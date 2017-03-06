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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace MugenMvvmToolkit.UWP.Binding.Converters
{
    public sealed class NullToVisibilityConverter : IValueConverter
    {
        #region Constructors

        public NullToVisibilityConverter(Visibility nullValue, Visibility notNullValue)
        {
            NotNullValue = notNullValue;
            NullValue = nullValue;
        }

        #endregion

        #region Properties

        public Visibility NotNullValue { get; set; }

        public Visibility NullValue { get; set; }

        #endregion

        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return NullValue;
            return NotNullValue;
        }


        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            Should.MethodBeSupported(false, "ConvertBack()");
            return null;
        }

        #endregion
    }
}
