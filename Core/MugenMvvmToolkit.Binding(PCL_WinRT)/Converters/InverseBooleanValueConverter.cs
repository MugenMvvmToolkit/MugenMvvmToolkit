#region Copyright
// ****************************************************************************
// <copyright file="InverseBooleanValueConverter.cs">
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
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Converters
{
    /// <summary>
    ///     This converter reverses a Boolean value (True == False, False == True).
    /// </summary>
    public sealed class InverseBooleanValueConverter : ValueConverterBase<bool?, bool?>
    {
        #region Fields

        /// <summary>
        ///     Gets an instance of <see cref="InverseBooleanValueConverter" />.
        /// </summary>
        public static readonly InverseBooleanValueConverter Instance;

        #endregion

        #region Constructors

        static InverseBooleanValueConverter()
        {
            Instance = new InverseBooleanValueConverter();
        }

        #endregion

        #region Overrides of ValueConverterBase

        protected override bool? Convert(bool? value, Type targetType, object parameter, CultureInfo culture, IDataContext context)
        {
            if (value.HasValue)
                return !value.Value;
            return null;
        }

        protected override bool? ConvertBack(bool? value, Type targetType, object parameter, CultureInfo culture, IDataContext context)
        {
            return Convert(value, targetType, parameter, culture, context);
        }

        #endregion
    }
}