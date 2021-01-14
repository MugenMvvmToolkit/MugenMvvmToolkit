using System;
using System.Linq.Expressions;
using MugenMvvm.Bindings.Attributes;
using MugenMvvm.Bindings.Parsing;
using MugenMvvm.Bindings.Parsing.Components.Converters;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Attributes
{
    public static class IgnoreBindingMemberAttributeTestExt
    {
        [IgnoreBindingMember]
        public static int L(this string target, int value) => value;
    }

    public class IgnoreBindingMemberAttributeTest : UnitTestBase
    {
        [Fact]
        public void TryConvertShouldIgnoreExpression1()
        {
            var call = Expression.Call(typeof(IgnoreBindingMemberAttributeTest), nameof(StaticMethod), Default.Array<Type>());
            var attribute = (IgnoreBindingMemberAttribute) BindingSyntaxExtensionAttributeBase.TryGet(call.Method)!;
            var ctx = new ExpressionConverterContext<Expression>();
            attribute.TryConvert(ctx, call, out var result).ShouldBeTrue();
            result.ShouldEqual(ConstantExpressionNode.Get<IgnoreBindingMemberAttributeTest>());
        }

        [Fact]
        public void TryConvertShouldIgnoreExpression2()
        {
            var call = Expression.Call(Expression.Constant(this), nameof(Method), Default.Array<Type>());
            var attribute = (IgnoreBindingMemberAttribute) BindingSyntaxExtensionAttributeBase.TryGet(call.Method)!;
            var ctx = new ExpressionConverterContext<Expression>
            {
                Converters = new[] {new ConstantExpressionConverter()}
            };
            attribute.TryConvert(ctx, call, out var result).ShouldBeTrue();
            result.ShouldEqual(ConstantExpressionNode.Get(this));
        }

        [Fact]
        public void TryConvertShouldIgnoreExpression3()
        {
            var access = Expression.MakeMemberAccess(null, GetType().GetProperty(nameof(StaticProperty))!);
            var attribute = (IgnoreBindingMemberAttribute) BindingSyntaxExtensionAttributeBase.TryGet(access.Member)!;
            var ctx = new ExpressionConverterContext<Expression>();
            attribute.TryConvert(ctx, access, out var result).ShouldBeTrue();
            result.ShouldEqual(ConstantExpressionNode.Get<IgnoreBindingMemberAttributeTest>());
        }

        [Fact]
        public void TryConvertShouldIgnoreExpression4()
        {
            var access = Expression.MakeMemberAccess(Expression.Constant(this), GetType().GetProperty(nameof(Property))!);
            var attribute = (IgnoreBindingMemberAttribute) BindingSyntaxExtensionAttributeBase.TryGet(access.Member)!;
            var ctx = new ExpressionConverterContext<Expression>
            {
                Converters = new[] {new ConstantExpressionConverter()}
            };
            attribute.TryConvert(ctx, access, out var result).ShouldBeTrue();
            result.ShouldEqual(ConstantExpressionNode.Get(this));
        }

        [Fact]
        public void TryConvertShouldIgnoreExpression5()
        {
            var call = Expression.Call(typeof(IgnoreBindingMemberAttributeTestExt), nameof(IgnoreBindingMemberAttributeTestExt.L), Default.Array<Type>(), Expression.Constant(""),
                Expression.Constant(1));
            var attribute = (IgnoreBindingMemberAttribute) BindingSyntaxExtensionAttributeBase.TryGet(call.Method)!;
            var ctx = new ExpressionConverterContext<Expression>
            {
                Converters = new[] {new ConstantExpressionConverter()}
            };
            attribute.TryConvert(ctx, call, out var result).ShouldBeTrue();
            result.ShouldEqual(ConstantExpressionNode.Get(""));
        }

        [IgnoreBindingMember]
        public static string StaticProperty => "";

        [IgnoreBindingMember]
        public string Property => "";

        [IgnoreBindingMember]
        public static string StaticMethod() => "";

        [IgnoreBindingMember]
        public string Method() => "";
    }
}