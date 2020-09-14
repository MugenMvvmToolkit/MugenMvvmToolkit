using System;
using System.Linq.Expressions;
using MugenMvvm.Binding.Build;
using MugenMvvm.Binding.Interfaces.Build;
using MugenMvvm.Binding.Parsing;
using MugenMvvm.Binding.Parsing.Expressions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Binding.Build
{
    public class BindingBuilderTargetTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void FromShouldCreateBindingBuilderFrom()
        {
            BindingBuilderTarget<string, object> target = default;
            target.For("P").PathOrExpression.ShouldEqual("P");
            target.For(MemberExpressionNode.Empty).PathOrExpression.ShouldEqual(MemberExpressionNode.Empty);
            Expression<Func<string, int>> expression = s => s.Length;
            target.For(expression).PathOrExpression.ShouldEqual(expression);
        }

        [Fact]
        public void ActionShouldCreateBindingBuilderTo()
        {
            BindingBuilderTarget<string, object> target = default;
            Expression<Func<IBindingBuilderContext<string, object>, object?>> expression1 = context => context.Source;
            var converterRequest = (BindingExpressionRequest) target.Action(expression1);
            converterRequest.Target.ShouldEqual(MemberExpressionNode.Action);
            converterRequest.Source.ShouldEqual(expression1);
            converterRequest.Parameters.AsList().Count.ShouldEqual(0);

            Expression<Func<IBindingBuilderContext<string, IComparable>, object?>> expression2 = context => context.Source;
            converterRequest = target.Action(expression2);
            converterRequest.Target.ShouldEqual(MemberExpressionNode.Action);
            converterRequest.Source.ShouldEqual(expression2);
            converterRequest.Parameters.AsList().Count.ShouldEqual(0);

            Expression<Action<IBindingBuilderContext<string, object>>> expression3 = context => context.ToString();
            converterRequest = target.Action(expression3);
            converterRequest.Target.ShouldEqual(MemberExpressionNode.Action);
            converterRequest.Source.ShouldEqual(expression3);
            converterRequest.Parameters.AsList().Count.ShouldEqual(0);

            Expression<Action<IBindingBuilderContext<string, IComparable>>> expression4 = context => context.ToString();
            converterRequest = target.Action(expression4);
            converterRequest.Target.ShouldEqual(MemberExpressionNode.Action);
            converterRequest.Source.ShouldEqual(expression4);
            converterRequest.Parameters.AsList().Count.ShouldEqual(0);
        }

        #endregion
    }
}