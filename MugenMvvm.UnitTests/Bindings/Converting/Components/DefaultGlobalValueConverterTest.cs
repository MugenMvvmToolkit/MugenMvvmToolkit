using System;
using System.Globalization;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Converting;
using MugenMvvm.Bindings.Converting.Components;
using MugenMvvm.Bindings.Interfaces.Converting;
using MugenMvvm.Extensions;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Converting.Components
{
    public class DefaultGlobalValueConverterTest : UnitTestBase
    {
        private readonly DefaultGlobalValueConverter _component;

        public DefaultGlobalValueConverterTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _component = new DefaultGlobalValueConverter();
            GlobalValueConverter.AddComponent(_component);
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

            GlobalValueConverter.TryConvert(ref value, typeof(int), null, null).ShouldEqual(true);
            value.ShouldEqual(int.MaxValue);

            const float f = 1.1f;
            _component.FormatProvider = () => new NumberFormatInfo { CurrencyDecimalSeparator = ";", NumberDecimalSeparator = ";" };
            value = f.ToString(_component.FormatProvider());
            GlobalValueConverter.TryConvert(ref value, typeof(float), null, null).ShouldEqual(true);
            value.ShouldEqual(f);
        }

        [Fact]
        public void TryConvertShouldHandleEnum()
        {
            object? value = StringComparison.CurrentCulture.ToString();
            GlobalValueConverter.TryConvert(ref value, typeof(StringComparison), null, null).ShouldEqual(true);
            value.ShouldEqual(StringComparison.CurrentCulture);
        }

        [Fact]
        public void TryConvertShouldHandleInstanceOfType()
        {
            object? value = this;
            GlobalValueConverter.TryConvert(ref value, typeof(object), null, null).ShouldEqual(true);
            value.ShouldEqual(this);
        }

        [Fact]
        public void TryConvertShouldHandleNull()
        {
            object? value = null;
            GlobalValueConverter.TryConvert(ref value, typeof(object), null, null).ShouldEqual(true);
            value.ShouldBeNull();

            value = null;
            GlobalValueConverter.TryConvert(ref value, typeof(bool), null, null).ShouldEqual(true);
            value.ShouldEqual(false);
        }

        [Fact]
        public void TryConvertShouldHandleString()
        {
            object? value = this;
            GlobalValueConverter.TryConvert(ref value, typeof(string), null, null).ShouldEqual(true);
            value.ShouldEqual(ToString());

            const float f = 1.1f;
            value = f;
            GlobalValueConverter.TryConvert(ref value, typeof(string), null, null).ShouldEqual(true);
            value.ShouldEqual(f.ToString());

            _component.FormatProvider = () => new NumberFormatInfo { CurrencyDecimalSeparator = ";", NumberDecimalSeparator = ";" };
            value = f;
            GlobalValueConverter.TryConvert(ref value, typeof(string), null, null).ShouldEqual(true);
            value.ShouldEqual(f.ToString(_component.FormatProvider()));
        }

        [Fact]
        public void TryConvertShouldIgnoreNotConvertibleValue()
        {
            var v = new object();
            var value = v;

            GlobalValueConverter.TryConvert(ref value, GetType(), null, null).ShouldBeFalse();
            value.ShouldEqual(v);
        }

        protected override IGlobalValueConverter GetGlobalValueConverter() => new GlobalValueConverter(ComponentCollectionManager);
    }
}