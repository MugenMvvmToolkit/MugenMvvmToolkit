using System;
using System.Globalization;
using MonoTouch.UIKit;
using MugenMvvmToolkit.Binding.Converters;

namespace MugenMvvmToolkit.Converters
{
    public class BooleanToCheckmarkAccessoryConverter : ValueConverterBase<bool?, UITableViewCellAccessory>
    {
        #region Overrides of ValueConverterBase<bool,UITableViewCellAccessory>

        protected override UITableViewCellAccessory Convert(bool? value, Type targetType, object parameter,
            CultureInfo culture)
        {
            return value.GetValueOrDefault() ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;
        }

        protected override bool? ConvertBack(UITableViewCellAccessory value, Type targetType, object parameter,
            CultureInfo culture)
        {
            return value == UITableViewCellAccessory.Checkmark;
        }

        #endregion
    }
}