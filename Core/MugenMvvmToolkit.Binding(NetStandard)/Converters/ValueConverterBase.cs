#region Copyright

// ****************************************************************************
// <copyright file="ValueConverterBase.cs">
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
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Converters
{
    public abstract class ValueConverterBase<TFrom, TTo> : IBindingValueConverter
    {
        #region Implementation of IValueConverterCore

        object IBindingValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture, IDataContext context)
        {
            return Convert((TFrom)value, targetType, parameter, culture, context);
        }

        object IBindingValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture, IDataContext context)
        {
            return ConvertBack((TTo)value, targetType, parameter, culture, context);
        }

        #endregion

        #region Methods

        protected abstract TTo Convert(TFrom value, Type targetType, object parameter, CultureInfo culture, IDataContext context);

        protected abstract TFrom ConvertBack(TTo value, Type targetType, object parameter, CultureInfo culture, IDataContext context);

        #endregion
    }
}
