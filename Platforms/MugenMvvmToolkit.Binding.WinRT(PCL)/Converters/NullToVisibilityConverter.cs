#region Copyright
// ****************************************************************************
// <copyright file="NullToVisibilityConverter.cs">
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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace MugenMvvmToolkit.Binding.Converters
{
    /// <summary>
    ///     This converts a Boolean to a Visibility.  It supports mapping the conversions.
    /// </summary>
    public sealed class NullToVisibilityConverter : IValueConverter
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="NullToVisibilityConverter" /> class.
        /// </summary>
        public NullToVisibilityConverter(Visibility nullValue, Visibility notNullValue)
        {
            NotNullValue = notNullValue;
            NullValue = nullValue;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Mapping for False to Visibility.
        /// </summary>
        public Visibility NotNullValue { get; set; }

        /// <summary>
        ///     Mapping for null to Visibility.
        /// </summary>
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