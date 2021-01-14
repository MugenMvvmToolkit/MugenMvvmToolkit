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
    public static class BindingMemberAttributeTestExt
    {
        public const string Name = "LL";

        [BindingMember(Name)]
        public static int L(this string target) => target.Length;

        [BindingMember(1)]
        public static int M(this string target, string name) => target.Length;
    }

    [BindingMember(Name)]
    public class BindingMemberAttributeTest : UnitTestBase
    {
        private const string Name = "Test";
        private const string MethodName = "Method";
        private const string PropertyName = "P";

        [Fact]
        public void TryConvertShouldReturnMemberExpression1()
        {
            var attribute = (BindingMemberAttribute) BindingSyntaxExtensionAttributeBase.TryGet(typeof(BindingMemberAttributeTest))!;
            var ctx = new ExpressionConverterContext<Expression>();
            attribute.TryConvert(ctx, null, out var result).ShouldBeTrue();
            result.ShouldEqual(MemberExpressionNode.Get(null, Name));
        }

        [Fact]
        public void TryConvertShouldReturnMemberExpression2()
        {
            var call = Expression.Call(typeof(BindingMemberAttributeTest), nameof(StaticMethod), Default.Array<Type>());
            var attribute = (BindingMemberAttribute) BindingSyntaxExtensionAttributeBase.TryGet(call.Method)!;
            var ctx = new ExpressionConverterContext<Expression>();
            attribute.TryConvert(ctx, call, out var result).ShouldBeTrue();
            result.ShouldEqual(MemberExpressionNode.Get(MemberExpressionNode.Get(null, Name), MethodName));
        }

        [Fact]
        public void TryConvertShouldReturnMemberExpression3()
        {
            const string name = "TT";
            var call = Expression.Call(typeof(BindingMemberAttributeTest), nameof(StaticMemberMethod), Default.Array<Type>(), Expression.Constant(name));
            var attribute = (BindingMemberAttribute) BindingSyntaxExtensionAttributeBase.TryGet(call.Method)!;
            var ctx = new ExpressionConverterContext<Expression>();
            attribute.TryConvert(ctx, call, out var result).ShouldBeTrue();
            result.ShouldEqual(MemberExpressionNode.Get(MemberExpressionNode.Get(null, Name), name));
        }

        [Fact]
        public void TryConvertShouldReturnMemberExpression4()
        {
            var call = Expression.Call(Expression.Constant(this), nameof(Method), Default.Array<Type>());
            var attribute = (BindingMemberAttribute) BindingSyntaxExtensionAttributeBase.TryGet(call.Method)!;
            var ctx = new ExpressionConverterContext<Expression>
            {
                Converters = new[] {new ConstantExpressionConverter()}
            };
            attribute.TryConvert(ctx, call, out var result).ShouldBeTrue();
            result.ShouldEqual(MemberExpressionNode.Get(ConstantExpressionNode.Get(this), MethodName));
        }

        [Fact]
        public void TryConvertShouldReturnMemberExpression5()
        {
            const string name = "TT";
            var call = Expression.Call(Expression.Constant(this), nameof(MemberMethod), Default.Array<Type>(), Expression.Constant(name));
            var attribute = (BindingMemberAttribute) BindingSyntaxExtensionAttributeBase.TryGet(call.Method)!;
            var ctx = new ExpressionConverterContext<Expression>
            {
                Converters = new[] {new ConstantExpressionConverter()}
            };
            attribute.TryConvert(ctx, call, out var result).ShouldBeTrue();
            result.ShouldEqual(MemberExpressionNode.Get(ConstantExpressionNode.Get(this), name));
        }

        [Fact]
        public void TryConvertShouldReturnMemberExpression6()
        {
            var access = Expression.MakeMemberAccess(null, GetType().GetProperty(nameof(StaticProperty))!);
            var attribute = (BindingMemberAttribute) BindingSyntaxExtensionAttributeBase.TryGet(access.Member)!;
            var ctx = new ExpressionConverterContext<Expression>();
            attribute.TryConvert(ctx, access, out var result).ShouldBeTrue();
            result.ShouldEqual(MemberExpressionNode.Get(MemberExpressionNode.Get(null, Name), PropertyName));
        }

        [Fact]
        public void TryConvertShouldReturnMemberExpression7()
        {
            var access = Expression.MakeMemberAccess(Expression.Constant(this), GetType().GetProperty(nameof(Property))!);
            var attribute = (BindingMemberAttribute) BindingSyntaxExtensionAttributeBase.TryGet(access.Member)!;
            var ctx = new ExpressionConverterContext<Expression>
            {
                Converters = new[] {new ConstantExpressionConverter()}
            };
            attribute.TryConvert(ctx, access, out var result).ShouldBeTrue();
            result.ShouldEqual(MemberExpressionNode.Get(ConstantExpressionNode.Get(this), PropertyName));
        }

        [Fact]
        public void TryConvertShouldReturnMemberExpression8()
        {
            var call = Expression.Call(typeof(BindingMemberAttributeTestExt), nameof(BindingMemberAttributeTestExt.L), Default.Array<Type>(), Expression.Constant(""));
            var attribute = (BindingMemberAttribute) BindingSyntaxExtensionAttributeBase.TryGet(call.Method)!;
            var ctx = new ExpressionConverterContext<Expression>
            {
                Converters = new[] {new ConstantExpressionConverter()}
            };
            attribute.TryConvert(ctx, call, out var result).ShouldBeTrue();
            result.ShouldEqual(MemberExpressionNode.Get(ConstantExpressionNode.Get(""), BindingMemberAttributeTestExt.Name));
        }

        [Fact]
        public void TryConvertShouldReturnMemberExpression9()
        {
            const string name = "TT";
            var call = Expression.Call(typeof(BindingMemberAttributeTestExt), nameof(BindingMemberAttributeTestExt.M), Default.Array<Type>(), Expression.Constant(""),
                Expression.Constant(name));
            var attribute = (BindingMemberAttribute) BindingSyntaxExtensionAttributeBase.TryGet(call.Method)!;
            var ctx = new ExpressionConverterContext<Expression>
            {
                Converters = new[] {new ConstantExpressionConverter()}
            };
            attribute.TryConvert(ctx, call, out var result).ShouldBeTrue();
            result.ShouldEqual(MemberExpressionNode.Get(ConstantExpressionNode.Get(""), name));
        }

        [BindingMember(PropertyName)]
        public static string StaticProperty => "";

        [BindingMember(PropertyName)]
        public string Property => "";

        [BindingMember(MethodName)]
        public static string StaticMethod() => "";

        [BindingMember(0)]
        public static string StaticMemberMethod(string name) => name;

        [BindingMember(MethodName)]
        public string Method() => "";

        [BindingMember(0)]
        public string MemberMethod(string name) => name;
    }
}