#region Copyright

// ****************************************************************************
// <copyright file="BooleanToVisibilityConverter.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
    public sealed class BooleanToVisibilityConverter : IValueConverter
    {
        #region Constructors

        public BooleanToVisibilityConverter(Visibility trueValue, Visibility falseValue, Visibility nullValue)
        {
            TrueValue = trueValue;
            FalseValue = falseValue;
            NullValue = nullValue;
        }

        #endregion

        #region Properties

        public Visibility TrueValue { get; private set; }

        public Visibility FalseValue { get; private set; }

        public Visibility NullValue { get; private set; }

        #endregion

        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return NullValue;

            if ((bool) value)
                return TrueValue;
            return FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return null;
            var visibility = (Visibility) value;
            if (visibility == TrueValue)
                return true;
            if (visibility == FalseValue)
                return false;
            return null;
        }

        #endregion
    }
}
