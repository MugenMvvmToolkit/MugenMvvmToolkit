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
using System.Globalization;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
#if NETFX_CORE || WINDOWSCOMMON
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
#elif ANDROID
using Android.App;
using Android.Graphics;
using Android.Widget;
using Android.Content;
using IValueConverter = MugenMvvmToolkit.Binding.Interfaces.IBindingValueConverter;
#elif WINFORMS
using System.Drawing;
using System.Windows.Forms;
using IValueConverter = MugenMvvmToolkit.Binding.Interfaces.IBindingValueConverter;
#elif TOUCH
using System.Drawing;
using MonoTouch.UIKit;
using IValueConverter = MugenMvvmToolkit.Binding.Interfaces.IBindingValueConverter;
#elif XAMARIN_FORMS
using Xamarin.Forms;
using IValueConverter = MugenMvvmToolkit.Binding.Interfaces.IBindingValueConverter;
#else
using System.Windows;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Controls;
#endif

// ReSharper disable once CheckNamespace
namespace MugenMvvmToolkit.Binding.Converters
{
    /// <summary>
    ///     Represents the converter that allows to convert a view model to view.
    /// </summary>
    public class ViewModelToViewConverter : IValueConverter
    {
        #region Fields

        /// <summary>
        /// Gets an instance of <see cref="ViewModelToViewConverter"/>.
        /// </summary>
        public static readonly ViewModelToViewConverter Instance;

        #endregion

        #region Constructors

        static ViewModelToViewConverter()
        {
            Instance = new ViewModelToViewConverter();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelToViewConverter"/> class.
        /// </summary>
        public ViewModelToViewConverter()
        {
            ThrowOnError = ThrowOnErrorDefault;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     true to throw an exception if the view cannot be found; false to return default view. 
        ///     Specifying false also suppresses some other exception conditions, but not all of them.
        /// </summary>
        public static bool ThrowOnErrorDefault { get; set; }

        /// <summary>
        ///     true to throw an exception if the view cannot be found; false to return default view. 
        ///     Specifying false also suppresses some other exception conditions, but not all of them.
        /// </summary>
        public bool ThrowOnError { get; set; }

        /// <summary>
        /// Gets or sets the default value that indicates that view converter should always create new view.
        /// </summary>
        public bool? AlwaysCreateNewView { get; set; }

        /// <summary>
        /// Gets or sets the name of view.
        /// </summary>
        public string ViewName { get; set; }

        #endregion

        #region Implementation of IValueConverter

#if NETFX_CORE || WINDOWSCOMMON
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
                var txt = new TextView(parameter as Context ?? Application.Context) { Text = exception.Flatten(false) };
                txt.SetTextColor(Color.Red);
                return txt;
#elif TOUCH
                return new UITextView(new RectangleF(10, 10, 300, 30)) { TextColor = UIColor.Red, Editable = false, DataDetectorTypes = UIDataDetectorType.None, Text = exception.Flatten(false) };
#elif XAMARIN_FORMS
                return new Label{TextColor = Color.Red, Text = exception.Flatten(false) };
#else
                return new TextBox
                {
                    Text = exception.Flatten(false),
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

#if NETFX_CORE || WINDOWSCOMMON
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