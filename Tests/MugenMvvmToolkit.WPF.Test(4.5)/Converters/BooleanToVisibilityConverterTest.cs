using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Binding.Converters;
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
            var converter = new BooleanToVisibilityConverter(Visibility.Collapsed, Visibility.Visible, Visibility.Visible);
            converter.Convert(true, typeof (Visibility), null, null).ShouldEqual(converter.TrueValue);
            converter.Convert(false, typeof (Visibility), null, null).ShouldEqual(converter.FalseValue);
        }

        [TestMethod]
        public void ConvertBackTest()
        {
            var converter = new BooleanToVisibilityConverter(Visibility.Collapsed, Visibility.Visible, Visibility.Visible);
            converter.ConvertBack(converter.TrueValue, typeof (bool), null, null).ShouldEqual(true);
            converter.ConvertBack(converter.FalseValue, typeof (bool), null, null).ShouldEqual(false);
        }
    }
}