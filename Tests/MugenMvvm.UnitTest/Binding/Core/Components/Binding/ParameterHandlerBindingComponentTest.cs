using MugenMvvm.Binding.Core;
using MugenMvvm.Binding.Core.Components.Binding;
using MugenMvvm.Binding.Metadata;
using MugenMvvm.Binding.Observers;
using MugenMvvm.UnitTest.Binding.Compiling.Internal;
using MugenMvvm.UnitTest.Binding.Converters.Internal;
using MugenMvvm.UnitTest.Binding.Core.Internal;
using MugenMvvm.UnitTest.Binding.Members.Internal;
using MugenMvvm.UnitTest.Binding.Observers.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Core.Components.Binding
{
    public class ParameterHandlerBindingComponentTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void InterceptTargetValueShouldReturnFallbackValue()
        {
            var targetMember = new TestMemberAccessorInfo
            {
                Type = typeof(int)
            };
            var binding = new TestBinding();
            var fallback = new object();
            var handler = new ParameterHandlerBindingComponent(default, default, new BindingParameterValue(fallback, null), default);
            handler.InterceptTargetValue(binding, new MemberPathLastMember(this, targetMember), BindingMetadata.UnsetValue, DefaultMetadata).ShouldEqual(fallback);

            handler = new ParameterHandlerBindingComponent(default, default, default, default);
            handler.InterceptTargetValue(binding, new MemberPathLastMember(this, targetMember), BindingMetadata.UnsetValue, DefaultMetadata).ShouldEqual(0);
        }

        [Fact]
        public void InterceptTargetValueShouldReturnTargetNullValue()
        {
            var targetMember = new TestMemberAccessorInfo
            {
                Type = typeof(int)
            };
            var binding = new TestBinding();
            var targetNullValue = new object();
            var handler = new ParameterHandlerBindingComponent(default, default, default, new BindingParameterValue(targetNullValue, null));
            handler.InterceptTargetValue(binding, new MemberPathLastMember(this, targetMember), null, DefaultMetadata).ShouldEqual(targetNullValue);

            handler = new ParameterHandlerBindingComponent(default, default, default, default);
            handler.InterceptTargetValue(binding, new MemberPathLastMember(this, targetMember), null, DefaultMetadata).ShouldEqual(null);
        }

        [Fact]
        public void InterceptTargetValueShouldUseConverter()
        {
            var targetMember = new TestMemberAccessorInfo
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
            var handler = new ParameterHandlerBindingComponent(new BindingParameterValue(converter, null), new BindingParameterValue(converterParameter, null), default, default);
            handler.InterceptTargetValue(binding, new MemberPathLastMember(this, targetMember), value, DefaultMetadata).ShouldEqual(result);
        }

        [Fact]
        public void InterceptSourceValueShouldUseConverter()
        {
            var targetMember = new TestMemberAccessorInfo
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
            var handler = new ParameterHandlerBindingComponent(new BindingParameterValue(converter, null), new BindingParameterValue(converterParameter, null), default, default);
            handler.InterceptSourceValue(binding, new MemberPathLastMember(this, targetMember), value, DefaultMetadata).ShouldEqual(result);
        }

        [Fact]
        public void InterceptSourceValueShouldReturnTargetNullValue()
        {
            var targetMember = new TestMemberAccessorInfo
            {
                Type = typeof(int)
            };
            var binding = new TestBinding();
            var targetNullValue = new object();
            var handler = new ParameterHandlerBindingComponent(default, default, default, new BindingParameterValue(targetNullValue, null));
            handler.InterceptSourceValue(binding, new MemberPathLastMember(this, targetMember), targetNullValue, DefaultMetadata).ShouldBeNull();

            handler = new ParameterHandlerBindingComponent(default, default, default, default);
            handler.InterceptSourceValue(binding, new MemberPathLastMember(this, targetMember), targetNullValue, DefaultMetadata).ShouldEqual(targetNullValue);
        }

        [Fact]
        public void DisposeShouldDisposeValues()
        {
            var expressionDisposed = false;
            var observer1Disposed = false;
            var observer2Disposed = false;
            var observer3Disposed = false;

            var expression = new TestCompiledExpression {Dispose = () => expressionDisposed = true};
            var converter = new TestBindingValueConverter();
            var converterParameter = new TestMemberPathObserver {Dispose = () => observer1Disposed = true};
            var fallback = new[] {new TestMemberPathObserver {Dispose = () => observer2Disposed = true}, new TestMemberPathObserver {Dispose = () => observer3Disposed = true}};
            var targetNullValue = new object();
            var handler = new ParameterHandlerBindingComponent(new BindingParameterValue(converter, null), new BindingParameterValue(converterParameter, null), new BindingParameterValue(fallback, expression),
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
            expressionDisposed.ShouldBeTrue();
            observer1Disposed.ShouldBeTrue();
            observer2Disposed.ShouldBeTrue();
            observer3Disposed.ShouldBeTrue();
        }

        #endregion
    }
}