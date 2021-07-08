using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.UnitTests.Bindings.Compiling.Internal;
using MugenMvvm.UnitTests.Bindings.Parsing.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Core
{
    public class BindingParameterExpressionTest : UnitTestBase
    {
        private readonly TestCompiledExpression _compiledExpression;

        public BindingParameterExpressionTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _compiledExpression = new TestCompiledExpression();
        }

        [Fact]
        public void DefaultShouldBeEmpty() => default(BindingParameterExpression).IsEmpty.ShouldBeTrue();

        [Fact]
        public void ToBindingParameterShouldReturnBindingParameterValue1()
        {
            var value = "";
            var expression = new BindingParameterExpression(value, null);
            expression.ToBindingParameter(this, this, DefaultMetadata).ShouldEqual(new BindingParameterValue(value, null));
        }

        [Fact]
        public void ToBindingParameterShouldReturnBindingParameterValue2()
        {
            var target = new object();
            var source = new object();
            var result = new object();
            var value = new TestBindingMemberExpressionNode
            {
                GetBindingSource = (t, s, m) =>
                {
                    t.ShouldEqual(target);
                    s.ShouldEqual(source);
                    m.ShouldEqual(DefaultMetadata);
                    return result;
                }
            };
            var parameterExpression = new BindingParameterExpression(value, _compiledExpression);
            parameterExpression.ToBindingParameter(target, source, DefaultMetadata).ShouldEqual(new BindingParameterValue(result, _compiledExpression));
        }

        [Fact]
        public void ToBindingParameterShouldReturnBindingParameterValue3()
        {
            var target = new object();
            var source = new object();
            var result1 = new object();
            var result2 = new object();
            var value = new IBindingMemberExpressionNode[]
            {
                new TestBindingMemberExpressionNode
                {
                    GetBindingSource = (t, s, m) =>
                    {
                        t.ShouldEqual(target);
                        s.ShouldEqual(source);
                        m.ShouldEqual(DefaultMetadata);
                        return result1;
                    }
                },
                new TestBindingMemberExpressionNode
                {
                    GetBindingSource = (t, s, m) =>
                    {
                        t.ShouldEqual(target);
                        s.ShouldEqual(source);
                        m.ShouldEqual(DefaultMetadata);
                        return result2;
                    }
                }
            };
            var parameterExpression = new BindingParameterExpression(value, _compiledExpression);
            var bindingParameterValue = parameterExpression.ToBindingParameter(target, source, DefaultMetadata);
            bindingParameterValue.Expression.ShouldEqual(_compiledExpression);
            ((object[])bindingParameterValue.Parameter!).ShouldEqual(new[] { result1, result2 });
        }
    }
}