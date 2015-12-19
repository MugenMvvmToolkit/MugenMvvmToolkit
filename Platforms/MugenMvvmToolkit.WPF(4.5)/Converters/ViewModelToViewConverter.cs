#region Copyright

// ****************************************************************************
// <copyright file="ViewModelToViewConverter.cs">
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
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
#if !WINDOWSCOMMON
using System.Globalization;
#endif
#if WINDOWSCOMMON
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace MugenMvvmToolkit.WinRT.Binding.Converters
#elif ANDROID
using Android.App;
using Android.Graphics;
using Android.Widget;
using Android.Content;
using MugenMvvmToolkit.Interfaces.Models;
using IValueConverter = MugenMvvmToolkit.Binding.Interfaces.IBindingValueConverter;

namespace MugenMvvmToolkit.Android.Binding.Converters
#elif WINFORMS
using MugenMvvmToolkit.Interfaces.Models;
using System.Drawing;
using System.Windows.Forms;
using IValueConverter = MugenMvvmToolkit.Binding.Interfaces.IBindingValueConverter;

namespace MugenMvvmToolkit.WinForms.Binding.Converters
#elif TOUCH
using System.Drawing;
using MugenMvvmToolkit.Interfaces.Models;
using UIKit;
using IValueConverter = MugenMvvmToolkit.Binding.Interfaces.IBindingValueConverter;

namespace MugenMvvmToolkit.iOS.Binding.Converters
#elif XAMARIN_FORMS
using MugenMvvmToolkit.Interfaces.Models;
using Xamarin.Forms;
using IValueConverter = MugenMvvmToolkit.Binding.Interfaces.IBindingValueConverter;

namespace MugenMvvmToolkit.Xamarin.Forms.Binding.Converters
#elif WPF
using System.Windows;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Controls;

// ReSharper disable once CheckNamespace
namespace MugenMvvmToolkit.WPF.Binding.Converters
#elif SILVERLIGHT
using System.Windows;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Controls;

namespace MugenMvvmToolkit.Silverlight.Binding.Converters
#elif WINDOWS_PHONE
using System.Windows;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Controls;

namespace MugenMvvmToolkit.WinPhone.Binding.Converters
#endif
{
    public class ViewModelToViewConverter : IValueConverter
    {
        #region Fields

        public static readonly ViewModelToViewConverter Instance;

        #endregion

        #region Constructors

        static ViewModelToViewConverter()
        {
            Instance = new ViewModelToViewConverter();
        }

        public ViewModelToViewConverter()
        {
            ThrowOnError = ThrowOnErrorDefault;
        }

        #endregion

        #region Properties

        public static bool ThrowOnErrorDefault { get; set; }

        public bool ThrowOnError { get; set; }

        public bool? AlwaysCreateNewView { get; set; }

        public string ViewName { get; set; }

        #endregion

        #region Implementation of IValueConverter

#if WINDOWSCOMMON
        public object Convert(object value, Type targetType = null, object parameter = null, string language = null)
#elif ANDROID || WINFORMS || TOUCH || XAMARIN_FORMS
        public object Convert(object value, Type targetType = null, object parameter = null, CultureInfo culture = null, IDataContext context = null)
#else
        public object Convert(object value, Type targetType = null, object parameter = null, CultureInfo culture = null)
#endif

        {
            try
            {
                var ctx = (parameter as DataContext).ToNonReadOnly();
                if (!string.IsNullOrEmpty(ViewName))
                    ctx.AddOrUpdate(NavigationConstants.ViewName, ViewName);
#if ANDROID
                return PlatformExtensions.GetOrCreateView((IViewModel)value, AlwaysCreateNewView, ctx);
#else
                return ViewManager.GetOrCreateView((IViewModel)value, AlwaysCreateNewView, ctx);
#endif
            }
            catch (Exception exception)
            {
                if (ThrowOnError)
                    throw;
                Tracer.Error(exception.Flatten(true));
#if ANDROID
                var txt = new TextView(parameter as Context ?? Application.Context) { Text = exception.Flatten(true) };
                txt.SetTextColor(Color.Red);
                return txt;
#elif TOUCH
                return new UITextView(new RectangleF(10, 10, 300, 30)) { TextColor = UIColor.Red, Editable = false, DataDetectorTypes = UIDataDetectorType.None, Text = exception.Flatten(true) };
#elif XAMARIN_FORMS
                return new Label{TextColor = Color.Red, Text = exception.Flatten(true) };
#else
                return new TextBox
                {
                    Text = exception.Flatten(true),
#if WINFORMS
                    ReadOnly = true,
                    WordWrap = true,
                    ForeColor = Color.Red
#else
                    IsReadOnly = true,
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = new SolidColorBrush(Colors.Red)
#endif
                };
#endif
            }
        }

#if WINDOWSCOMMON
        public object ConvertBack(object value, Type targetType = null, object parameter = null, string language = null)
#elif ANDROID || WINFORMS || TOUCH || XAMARIN_FORMS
        public object ConvertBack(object value, Type targetType = null, object parameter = null, CultureInfo culture = null, IDataContext context = null)
#else
        public object ConvertBack(object value, Type targetType = null, object parameter = null, CultureInfo culture = null)
#endif

        {
            if (value == null)
                return null;
            return ViewManager.GetDataContext(value);
        }

        #endregion
    }
}
