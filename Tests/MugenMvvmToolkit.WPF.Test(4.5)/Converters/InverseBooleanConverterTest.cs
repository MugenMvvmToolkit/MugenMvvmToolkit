using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using MugenMvvmToolkit.Binding.Converters;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Interfaces;
using Should;

namespace MugenMvvmToolkit.Test.Converters
{
    [TestClass]
    public class InverseBooleanConverterTest
    {
        [TestMethod]
        public void TestConvertNotBool()
        {
            IBindingValueConverter converter = new InverseBooleanValueConverter();
            Action action = () => converter.Convert(new object(), typeof (object), null, null);
            action.ShouldThrow();
        }

        [TestMethod]
        public void TestConvertBackNotBool()
        {
            IBindingValueConverter converter = new InverseBooleanValueConverter();
            Action action = () => converter.ConvertBack(new object(), typeof (object), null, null);
            action.ShouldThrow();
        }

        [TestMethod]
        public void TestConvert()
        {
            IBindingValueConverter converter = new InverseBooleanValueConverter();
            converter.Convert(true, typeof (bool), null, null).ShouldEqual(false);
            converter.Convert(false, typeof (bool), null, null).ShouldEqual(true);
        }

        [TestMethod]
        public void TestConvertBack()
        {
            IBindingValueConverter converter = new InverseBooleanValueConverter();
            converter.ConvertBack(false, typeof (bool), null, null).ShouldEqual(true);
            converter.ConvertBack(true, typeof (bool), null, null).ShouldEqual(false);
        }
    }
}