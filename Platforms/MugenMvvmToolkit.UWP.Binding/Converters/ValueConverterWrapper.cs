#region Copyright

// ****************************************************************************
// <copyright file="ValueConverterWrapper.cs">
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
using Windows.UI.Xaml.Data;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.UWP.Binding.Converters
{
    public sealed class ValueConverterWrapper : IBindingValueConverter, IValueConverter
    {
        #region Fields

        private readonly IValueConverter _valueConverter;

        #endregion

        #region Constructors

        public ValueConverterWrapper([NotNull] IValueConverter valueConverter)
        {
            Should.NotBeNull(valueConverter, nameof(valueConverter));
            _valueConverter = valueConverter;
        }

        #endregion

        #region Implementation of IValueConverterCore

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture, IDataContext context)
        {
            return _valueConverter.Convert(value, targetType, parameter, culture.Name);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture, IDataContext context)
        {
            return _valueConverter.ConvertBack(value, targetType, parameter, culture.Name);
        }

        #endregion

        #region Implementation of IValueConverter

        object IValueConverter.Convert(object value, Type targetType, object parameter, string language)
        {
            return _valueConverter.Convert(value, targetType, parameter, language);
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return _valueConverter.ConvertBack(value, targetType, parameter, language);
        }

        #endregion
    }
}
