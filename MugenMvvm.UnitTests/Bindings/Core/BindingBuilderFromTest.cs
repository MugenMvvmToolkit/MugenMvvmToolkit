using System;
using System.Linq.Expressions;
using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Interfaces.Build;
using MugenMvvm.Bindings.Parsing;
using MugenMvvm.Bindings.Parsing.Expressions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Core
{
    public class BindingBuilderFromTest : UnitTestBase
    {
        [Fact]
        public void ToShouldCreateBindingBuilderTo()
        {
            var fromValue = "f";
            var toValue = "t";
            var target = new BindingBuilderFrom<string, object>(fromValue);

            var converterRequest = (BindingExpressionRequest)target.To(toValue);
            converterRequest.Target.ShouldEqual(fromValue);
            converterRequest.Source.ShouldEqual(toValue);
            converterRequest.Parameters.Count.ShouldEqual(0);

            converterRequest = target.To<IComparable>(toValue);
            converterRequest.Target.ShouldEqual(fromValue);
            converterRequest.Source.ShouldEqual(toValue);
            converterRequest.Parameters.Count.ShouldEqual(0);

            converterRequest = target.To(MemberExpressionNode.Empty);
            converterRequest.Target.ShouldEqual(fromValue);
            converterRequest.Source.ShouldEqual(MemberExpressionNode.Empty);
            converterRequest.Parameters.Count.ShouldEqual(0);

            converterRequest = target.To<IComparable>(MemberExpressionNode.Empty);
            converterRequest.Target.ShouldEqual(fromValue);
            converterRequest.Source.ShouldEqual(MemberExpressionNode.Empty);
            converterRequest.Parameters.Count.ShouldEqual(0);

            Expression<Func<IBindingBuilderContext<string, object>, object?>> expression1 = context => context.Source;
            converterRequest = target.To<object, object?>(expression1);
            converterRequest.Target.ShouldEqual(fromValue);
            converterRequest.Source.ShouldEqual(expression1);
            converterRequest.Parameters.Count.ShouldEqual(0);

            Expression<Func<IBindingBuilderContext<string, IComparable>, object?>> expression2 = context => context.Source;
            converterRequest = target.To(expression2);
            converterRequest.Target.ShouldEqual(fromValue);
            converterRequest.Source.ShouldEqual(expression2);
            converterRequest.Parameters.Count.ShouldEqual(0);

            Expression<Action<IBindingBuilderContext<string, object>>> expression3 = context => context.ToString();
            converterRequest = target.To(expression3);
            converterRequest.Target.ShouldEqual(fromValue);
            converterRequest.Source.ShouldEqual(expression3);
            converterRequest.Parameters.Count.ShouldEqual(0);

            Expression<Action<IBindingBuilderContext<string, IComparable>>> expression4 = context => context.ToString();
            converterRequest = target.To(expression4);
            converterRequest.Target.ShouldEqual(fromValue);
            converterRequest.Source.ShouldEqual(expression4);
            converterRequest.Parameters.Count.ShouldEqual(0);
        }
    }
}