using System;
using System.Globalization;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Convert.Components;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Convert.Components
{
    public class DefaultGlobalValueConverterTest : UnitTestBase
    {
        private readonly DefaultGlobalValueConverter _component;

        public DefaultGlobalValueConverterTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _component = new DefaultGlobalValueConverter();
        }

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            _component.Priority.ShouldEqual(ConverterComponentPriority.Converter);
            _component.Priority = int.MaxValue;
            _component.Priority.ShouldEqual(int.MaxValue);
        }

        [Fact]
        public void TryConvertShouldHandleConvertible()
        {
            object? value = int.MaxValue.ToString();

            _component.TryConvert(null!, ref value, typeof(int), null, null).ShouldEqual(true);
            value.ShouldEqual(int.MaxValue);

            const float f = 1.1f;
            _component.FormatProvider = () => new NumberFormatInfo {CurrencyDecimalSeparator = ";", NumberDecimalSeparator = ";"};
            value = f.ToString(_component.FormatProvider());
            _component.TryConvert(null!, ref value, typeof(float), null, null).ShouldEqual(true);
            value.ShouldEqual(f);
        }

        [Fact]
        public void TryConvertShouldHandleEnum()
        {
            object? value = StringComparison.CurrentCulture.ToString();
            _component.TryConvert(null!, ref value, typeof(StringComparison), null, null).ShouldEqual(true);
            value.ShouldEqual(StringComparison.CurrentCulture);
        }

        [Fact]
        public void TryConvertShouldHandleInstanceOfType()
        {
            object? value = this;
            _component.TryConvert(null!, ref value, typeof(object), null, null).ShouldEqual(true);
            value.ShouldEqual(this);
        }

        [Fact]
        public void TryConvertShouldHandleNull()
        {
            object? value = null;
            _component.TryConvert(null!, ref value, typeof(object), null, null).ShouldEqual(true);
            value.ShouldBeNull();

            value = null;
            _component.TryConvert(null!, ref value, typeof(bool), null, null).ShouldEqual(true);
            value.ShouldEqual(false);
        }

        [Fact]
        public void TryConvertShouldHandleString()
        {
            object? value = this;
            _component.TryConvert(null!, ref value, typeof(string), null, null).ShouldEqual(true);
            value.ShouldEqual(ToString());

            const float f = 1.1f;
            value = f;
            _component.TryConvert(null!, ref value, typeof(string), null, null).ShouldEqual(true);
            value.ShouldEqual(f.ToString());

            _component.FormatProvider = () => new NumberFormatInfo {CurrencyDecimalSeparator = ";", NumberDecimalSeparator = ";"};
            value = f;
            _component.TryConvert(null!, ref value, typeof(string), null, null).ShouldEqual(true);
            value.ShouldEqual(f.ToString(_component.FormatProvider()));
        }

        [Fact]
        public void TryConvertShouldIgnoreNotConvertibleValue()
        {
            var v = new object();
            var value = v;

            _component.TryConvert(null!, ref value, GetType(), null, null).ShouldBeFalse();
            value.ShouldEqual(v);
        }
    }
}