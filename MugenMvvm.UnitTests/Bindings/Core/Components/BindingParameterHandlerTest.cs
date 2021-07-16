using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Core.Components;
using MugenMvvm.Bindings.Metadata;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Tests.Bindings.Converting;
using MugenMvvm.Tests.Bindings.Members;
using MugenMvvm.Tests.Bindings.Observation;
using MugenMvvm.UnitTests.Bindings.Compiling.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Core.Components
{
    public class BindingParameterHandlerTest : UnitTestBase
    {
        public BindingParameterHandlerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
        }

        [Fact]
        public void DisposeShouldDisposeValues()
        {
            var observer1Disposed = false;
            var observer2Disposed = false;
            var observer3Disposed = false;

            var expression = new TestCompiledExpression();
            var converter = new TestBindingValueConverter();
            var converterParameter = new TestMemberPathObserver { Dispose = () => observer1Disposed = true };
            var fallback = new[]
                { new TestMemberPathObserver { Dispose = () => observer2Disposed = true }, new TestMemberPathObserver { Dispose = () => observer3Disposed = true } };
            var targetNullValue = new object();
            var handler = new BindingParameterHandler(new BindingParameterValue(converter, null), new BindingParameterValue(converterParameter, null),
                new BindingParameterValue(fallback, expression),
                new BindingParameterValue(targetNullValue, null));

            handler.Converter.Parameter.ShouldEqual(converter);
            handler.Converter.Expression.ShouldBeNull();

            handler.ConverterParameter.Parameter.ShouldEqual(converterParameter);
            handler.ConverterParameter.Expression.ShouldBeNull();

            handler.Fallback.Parameter.ShouldEqual(fallback);
            handler.Fallback.Expression.ShouldEqual(expression);

            handler.TargetNullValue.Parameter.ShouldEqual(targetNullValue);
            handler.TargetNullValue.Expression.ShouldBeNull();

            handler.Dispose();

            handler.Converter.IsEmpty.ShouldBeTrue();
            handler.ConverterParameter.IsEmpty.ShouldBeTrue();
            handler.Fallback.IsEmpty.ShouldBeTrue();
            handler.TargetNullValue.IsEmpty.ShouldBeTrue();
            observer1Disposed.ShouldBeTrue();
            observer2Disposed.ShouldBeTrue();
            observer3Disposed.ShouldBeTrue();
        }

        [Fact]
        public void InterceptSourceValueShouldReturnTargetNullValue()
        {
            var targetMember = new TestAccessorMemberInfo
            {
                Type = typeof(int)
            };
            var targetNullValue = new object();
            var handler = new BindingParameterHandler(default, default, default, new BindingParameterValue(targetNullValue, null));
            handler.InterceptSourceValue(Binding, new MemberPathLastMember(this, targetMember), targetNullValue, Metadata).ShouldBeNull();

            handler = new BindingParameterHandler(default, default, default, default);
            handler.InterceptSourceValue(Binding, new MemberPathLastMember(this, targetMember), targetNullValue, Metadata).ShouldEqual(targetNullValue);
        }

        [Fact]
        public void InterceptSourceValueShouldUseConverter()
        {
            var targetMember = new TestAccessorMemberInfo
            {
                Type = typeof(string)
            };
            var value = new object();
            var result = new object();
            var converterParameter = new object();
            var converter = new TestBindingValueConverter
            {
                ConvertBack = (o, type, arg3, arg4) =>
                {
                    o.ShouldEqual(value);
                    type.ShouldEqual(targetMember.Type);
                    arg3.ShouldEqual(converterParameter);
                    arg4.ShouldEqual(Metadata);
                    return result;
                }
            };
            var handler = new BindingParameterHandler(new BindingParameterValue(converter, null), new BindingParameterValue(converterParameter, null), default, default);
            handler.InterceptSourceValue(Binding, new MemberPathLastMember(this, targetMember), value, Metadata).ShouldEqual(result);
        }

        [Fact]
        public void InterceptTargetValueShouldReturnFallbackValue()
        {
            var targetMember = new TestAccessorMemberInfo
            {
                Type = typeof(int)
            };
            var fallback = new object();
            var handler = new BindingParameterHandler(default, default, new BindingParameterValue(fallback, null), default);
            handler.InterceptTargetValue(Binding, new MemberPathLastMember(this, targetMember), BindingMetadata.UnsetValue, Metadata).ShouldEqual(fallback);

            handler = new BindingParameterHandler(default, default, default, default);
            handler.InterceptTargetValue(Binding, new MemberPathLastMember(this, targetMember), BindingMetadata.UnsetValue, Metadata).ShouldEqual(0);
        }

        [Fact]
        public void InterceptTargetValueShouldReturnTargetNullValue()
        {
            var targetMember = new TestAccessorMemberInfo
            {
                Type = typeof(int)
            };
            var targetNullValue = new object();
            var handler = new BindingParameterHandler(default, default, default, new BindingParameterValue(targetNullValue, null));
            handler.InterceptTargetValue(Binding, new MemberPathLastMember(this, targetMember), null, Metadata).ShouldEqual(targetNullValue);

            handler = new BindingParameterHandler(default, default, default, default);
            handler.InterceptTargetValue(Binding, new MemberPathLastMember(this, targetMember), null, Metadata).ShouldEqual(null);
        }

        [Fact]
        public void InterceptTargetValueShouldUseConverter()
        {
            var targetMember = new TestAccessorMemberInfo
            {
                Type = typeof(string)
            };
            var value = new object();
            var result = new object();
            var converterParameter = new object();
            var converter = new TestBindingValueConverter
            {
                Convert = (o, type, arg3, arg4) =>
                {
                    o.ShouldEqual(value);
                    type.ShouldEqual(targetMember.Type);
                    arg3.ShouldEqual(converterParameter);
                    arg4.ShouldEqual(Metadata);
                    return result;
                }
            };
            var handler = new BindingParameterHandler(new BindingParameterValue(converter, null), new BindingParameterValue(converterParameter, null), default, default);
            handler.InterceptTargetValue(Binding, new MemberPathLastMember(this, targetMember), value, Metadata).ShouldEqual(result);
        }
    }
}