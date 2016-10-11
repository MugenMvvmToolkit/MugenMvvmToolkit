using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Binding.Converters;
using MugenMvvmToolkit.Silverlight.Binding.Converters;
using MugenMvvmToolkit.UWP.Binding.Converters;
using MugenMvvmToolkit.WPF.Binding.Converters;
using Should;
#if NETFX_CORE
using Windows.UI.Xaml;
#else
using System.Windows;
#endif

namespace MugenMvvmToolkit.Test.Converters
{
    [TestClass]
    public class BooleanToVisibilityConverterTest
    {
        [TestMethod]
        public void ConvertShouldThrowExceptionInvalidValue()
        {
            var converter = new BooleanToVisibilityConverter(Visibility.Collapsed, Visibility.Visible, Visibility.Visible);
            Action action = () => converter.Convert(new object(), typeof (object), null, null);
            action.ShouldThrow();
        }

        [TestMethod]
        public void ConvertBackShouldThrowExceptionInvalidValue()
        {
            var converter = new BooleanToVisibilityConverter(Visibility.Collapsed, Visibility.Visible, Visibility.Visible);
            Action action = () => converter.ConvertBack(new object(), typeof (object), null, null);
            action.ShouldThrow();
        }

        [TestMethod]
        public void ConvertTest()
        {
            const Visibility trueValue = Visibility.Collapsed;
            const Visibility falseValue = Visibility.Visible;
            var converter = new BooleanToVisibilityConverter(trueValue, falseValue, Visibility.Visible);
            converter.Convert(true, typeof(Visibility), null, null).ShouldEqual(trueValue);
            converter.Convert(false, typeof(Visibility), null, null).ShouldEqual(falseValue);
        }

        [TestMethod]
        public void ConvertBackTest()
        {
            const Visibility trueValue = Visibility.Collapsed;
            const Visibility falseValue = Visibility.Visible;
            var converter = new BooleanToVisibilityConverter(Visibility.Collapsed, Visibility.Visible, Visibility.Visible);
            converter.ConvertBack(trueValue, typeof(bool), null, null).ShouldEqual(true);
            converter.ConvertBack(falseValue, typeof(bool), null, null).ShouldEqual(false);
        }
    }
}
