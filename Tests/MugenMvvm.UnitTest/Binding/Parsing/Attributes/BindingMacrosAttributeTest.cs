using System.Linq.Expressions;
using MugenMvvm.Binding.Attributes;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Parsing;
using MugenMvvm.Binding.Parsing.Expressions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Parsing.Attributes
{
    [BindingMacros(Name, true)]
    public class BindingMacrosAttributeTest : UnitTestBase
    {
        #region Fields

        private const string Name = "Test";

        #endregion

        #region Methods

        [Fact]
        public void TryConvertShouldReturnResourceExpression()
        {
            var attribute = (BindingMacrosAttribute)BindingSyntaxExtensionAttributeBase.TryGet(typeof(BindingMacrosAttributeTest))!;
            var ctx = new ExpressionConverterContext<Expression>();
            attribute.TryConvert(ctx, null, out var result).ShouldBeTrue();
            result.ShouldEqual(new UnaryExpressionNode(UnaryTokenType.StaticExpression, new MemberExpressionNode(null, Name)));
        }

        #endregion
    }
}