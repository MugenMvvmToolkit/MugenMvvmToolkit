#region Copyright

// ****************************************************************************
// <copyright file="ValueConverterWrapper.cs">
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
using System.Globalization;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
#if XAMARIN_FORMS
using Xamarin.Forms;

namespace MugenMvvmToolkit.Xamarin.Forms.Binding.Converters
#elif WPF
using System.Windows.Data;

namespace MugenMvvmToolkit.WPF.Binding.Converters
#endif
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
            return _valueConverter.Convert(value, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture, IDataContext context)
        {
            return _valueConverter.ConvertBack(value, targetType, parameter, culture);
        }

        #endregion

        #region Implementation of IValueConverter

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType, parameter, culture, DataContext.Empty);
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ConvertBack(value, targetType, parameter, culture, DataContext.Empty);
        }

        #endregion
    }
}
