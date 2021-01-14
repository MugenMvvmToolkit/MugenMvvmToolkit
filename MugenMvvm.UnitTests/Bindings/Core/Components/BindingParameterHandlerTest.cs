using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Core.Components;
using MugenMvvm.Bindings.Metadata;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.UnitTests.Bindings.Compiling.Internal;
using MugenMvvm.UnitTests.Bindings.Convert.Internal;
using MugenMvvm.UnitTests.Bindings.Core.Internal;
using MugenMvvm.UnitTests.Bindings.Members.Internal;
using MugenMvvm.UnitTests.Bindings.Observation.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Core.Components
{
    public class BindingParameterHandlerTest : UnitTestBase
    {
        [Fact]
        public void DisposeShouldDisposeValues()
        {
            var observer1Disposed = false;
            var observer2Disposed = false;
            var observer3Disposed = false;

            var expression = new TestCompiledExpression();
            var converter = new TestBindingValueConverter();
            var converterParameter = new TestMemberPathObserver {Dispose = () => observer1Disposed = true};
            var fallback = new[] {new TestMemberPathObserver {Dispose = () => observer2Disposed = true}, new TestMemberPathObserver {Dispose = () => observer3Disposed = true}};
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
            var binding = new TestBinding();
            var targetNullValue = new object();
            var handler = new BindingParameterHandler(default, default, default, new BindingParameterValue(targetNullValue, null));
            handler.InterceptSourceValue(binding, new MemberPathLastMember(this, targetMember), targetNullValue, DefaultMetadata).ShouldBeNull();

            handler = new BindingParameterHandler(default, default, default, default);
            handler.InterceptSourceValue(binding, new MemberPathLastMember(this, targetMember), targetNullValue, DefaultMetadata).ShouldEqual(targetNullValue);
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
            var binding = new TestBinding();
            var converterParameter = new object();
            var converter = new TestBindingValueConverter
            {
                ConvertBack = (o, type, arg3, arg4) =>
                {
                    o.ShouldEqual(value);
                    type.ShouldEqual(targetMember.Type);
                    arg3.ShouldEqual(converterParameter);
                    arg4.ShouldEqual(DefaultMetadata);
                    return result;
                }
            };
            var handler = new BindingParameterHandler(new BindingParameterValue(converter, null), new BindingParameterValue(converterParameter, null), default, default);
            handler.InterceptSourceValue(binding, new MemberPathLastMember(this, targetMember), value, DefaultMetadata).ShouldEqual(result);
        }

        [Fact]
        public void InterceptTargetValueShouldReturnFallbackValue()
        {
            var targetMember = new TestAccessorMemberInfo
            {
                Type = typeof(int)
            };
            var binding = new TestBinding();
            var fallback = new object();
            var handler = new BindingParameterHandler(default, default, new BindingParameterValue(fallback, null), default);
            handler.InterceptTargetValue(binding, new MemberPathLastMember(this, targetMember), BindingMetadata.UnsetValue, DefaultMetadata).ShouldEqual(fallback);

            handler = new BindingParameterHandler(default, default, default, default);
            handler.InterceptTargetValue(binding, new MemberPathLastMember(this, targetMember), BindingMetadata.UnsetValue, DefaultMetadata).ShouldEqual(0);
        }

        [Fact]
        public void InterceptTargetValueShouldReturnTargetNullValue()
        {
            var targetMember = new TestAccessorMemberInfo
            {
                Type = typeof(int)
            };
            var binding = new TestBinding();
            var targetNullValue = new object();
            var handler = new BindingParameterHandler(default, default, default, new BindingParameterValue(targetNullValue, null));
            handler.InterceptTargetValue(binding, new MemberPathLastMember(this, targetMember), null, DefaultMetadata).ShouldEqual(targetNullValue);

            handler = new BindingParameterHandler(default, default, default, default);
            handler.InterceptTargetValue(binding, new MemberPathLastMember(this, targetMember), null, DefaultMetadata).ShouldEqual(null);
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
            var binding = new TestBinding();
            var converterParameter = new object();
            var converter = new TestBindingValueConverter
            {
                Convert = (o, type, arg3, arg4) =>
                {
                    o.ShouldEqual(value);
                    type.ShouldEqual(targetMember.Type);
                    arg3.ShouldEqual(converterParameter);
                    arg4.ShouldEqual(DefaultMetadata);
                    return result;
                }
            };
            var handler = new BindingParameterHandler(new BindingParameterValue(converter, null), new BindingParameterValue(converterParameter, null), default, default);
            handler.InterceptTargetValue(binding, new MemberPathLastMember(this, targetMember), value, DefaultMetadata).ShouldEqual(result);
        }
    }
}