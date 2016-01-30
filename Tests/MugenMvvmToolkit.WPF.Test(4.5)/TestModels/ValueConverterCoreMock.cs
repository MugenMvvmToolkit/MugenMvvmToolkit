using System;
using System.Globalization;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Test.TestModels
{
    public class ValueConverterCoreMock : IBindingValueConverter
    {
        #region Properties

        public Func<object, Type, object, CultureInfo, object> Convert { get; set; }

        public Func<object, Type, object, CultureInfo, object> ConvertBack { get; set; }

        #endregion

        #region Implementation of IBindingValueConverter

        object IBindingValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture, IDataContext context)
        {
            return Convert(value, targetType, parameter, culture);
        }

        object IBindingValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture, IDataContext context)
        {
            return ConvertBack(value, targetType, parameter, culture);
        }

        #endregion
    }
}
