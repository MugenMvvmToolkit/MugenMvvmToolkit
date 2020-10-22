using MugenMvvm.Binding.Core;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.UnitTests.Binding.Compiling.Internal;
using MugenMvvm.UnitTests.Binding.Parsing.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Binding.Core
{
    public class BindingParameterExpressionTest : UnitTestBase
    {
        #region Methods

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
            var exp = new TestCompiledExpression();
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
            var parameterExpression = new BindingParameterExpression(value, exp);
            parameterExpression.ToBindingParameter(target, source, DefaultMetadata).ShouldEqual(new BindingParameterValue(result, exp));
        }

        [Fact]
        public void ToBindingParameterShouldReturnBindingParameterValue3()
        {
            var target = new object();
            var source = new object();
            var result1 = new object();
            var result2 = new object();
            var exp = new TestCompiledExpression();
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
            var parameterExpression = new BindingParameterExpression(value, exp);
            var bindingParameterValue = parameterExpression.ToBindingParameter(target, source, DefaultMetadata);
            bindingParameterValue.Expression.ShouldEqual(exp);
            ((object[]) bindingParameterValue.Parameter!).ShouldEqual(new[] {result1, result2});
        }

        #endregion
    }
}