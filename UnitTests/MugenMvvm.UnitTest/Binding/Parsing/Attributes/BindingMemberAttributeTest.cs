using System;
using System.Linq.Expressions;
using MugenMvvm.Binding.Attributes;
using MugenMvvm.Binding.Parsing;
using MugenMvvm.Binding.Parsing.Components.Converters;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Parsing.Attributes
{
    public static class BindingMemberAttributeTestExt
    {
        #region Fields

        public const string Name = "LL";

        #endregion

        #region Methods

        [BindingMember(Name)]
        public static int L(this string target)
        {
            return target.Length;
        }

        [BindingMember(1)]
        public static int M(this string target, string name)
        {
            return target.Length;
        }

        #endregion
    }

    [BindingMember(Name)]
    public class BindingMemberAttributeTest : UnitTestBase
    {
        #region Fields

        private const string Name = "Test";
        private const string MethodName = "Method";
        private const string PropertyName = "P";

        #endregion

        #region Properties

        [BindingMember(PropertyName)]
        public static string StaticProperty => "";

        [BindingMember(PropertyName)]
        public string Property => "";

        #endregion

        #region Methods

        [Fact]
        public void TryConvertShouldReturnResourceExpression1()
        {
            var attribute = (BindingMemberAttribute) BindingSyntaxExtensionAttributeBase.TryGet(typeof(BindingMemberAttributeTest))!;
            var ctx = new ExpressionConverterContext<Expression>();
            attribute.TryConvert(ctx, null, out var result).ShouldBeTrue();
            result.ShouldEqual(MemberExpressionNode.Get(null, Name));
        }

        [Fact]
        public void TryConvertShouldReturnResourceExpression2()
        {
            var call = Expression.Call(typeof(BindingMemberAttributeTest), nameof(StaticMethod), Default.Array<Type>());
            var attribute = (BindingMemberAttribute) BindingSyntaxExtensionAttributeBase.TryGet(call.Method)!;
            var ctx = new ExpressionConverterContext<Expression>();
            attribute.TryConvert(ctx, call, out var result).ShouldBeTrue();
            result.ShouldEqual(MemberExpressionNode.Get(MemberExpressionNode.Get(null, Name), MethodName));
        }

        [Fact]
        public void TryConvertShouldReturnResourceExpression3()
        {
            const string name = "TT";
            var call = Expression.Call(typeof(BindingMemberAttributeTest), nameof(StaticMemberMethod), Default.Array<Type>(), Expression.Constant(name));
            var attribute = (BindingMemberAttribute) BindingSyntaxExtensionAttributeBase.TryGet(call.Method)!;
            var ctx = new ExpressionConverterContext<Expression>();
            attribute.TryConvert(ctx, call, out var result).ShouldBeTrue();
            result.ShouldEqual(MemberExpressionNode.Get(MemberExpressionNode.Get(null, Name), name));
        }

        [Fact]
        public void TryConvertShouldReturnResourceExpression4()
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
        public void TryConvertShouldReturnResourceExpression5()
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
        public void TryConvertShouldReturnResourceExpression6()
        {
            var access = Expression.MakeMemberAccess(null, GetType().GetProperty(nameof(StaticProperty)));
            var attribute = (BindingMemberAttribute) BindingSyntaxExtensionAttributeBase.TryGet(access.Member)!;
            var ctx = new ExpressionConverterContext<Expression>();
            attribute.TryConvert(ctx, access, out var result).ShouldBeTrue();
            result.ShouldEqual(MemberExpressionNode.Get(MemberExpressionNode.Get(null, Name), PropertyName));
        }

        [Fact]
        public void TryConvertShouldReturnResourceExpression7()
        {
            var access = Expression.MakeMemberAccess(Expression.Constant(this), GetType().GetProperty(nameof(Property)));
            var attribute = (BindingMemberAttribute) BindingSyntaxExtensionAttributeBase.TryGet(access.Member)!;
            var ctx = new ExpressionConverterContext<Expression>
            {
                Converters = new[] {new ConstantExpressionConverter()}
            };
            attribute.TryConvert(ctx, access, out var result).ShouldBeTrue();
            result.ShouldEqual(MemberExpressionNode.Get(ConstantExpressionNode.Get(this), PropertyName));
        }

        [Fact]
        public void TryConvertShouldReturnResourceExpression8()
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
        public void TryConvertShouldReturnResourceExpression9()
        {
            const string name = "TT";
            var call = Expression.Call(typeof(BindingMemberAttributeTestExt), nameof(BindingMemberAttributeTestExt.M), Default.Array<Type>(), Expression.Constant(""), Expression.Constant(name));
            var attribute = (BindingMemberAttribute) BindingSyntaxExtensionAttributeBase.TryGet(call.Method)!;
            var ctx = new ExpressionConverterContext<Expression>
            {
                Converters = new[] {new ConstantExpressionConverter()}
            };
            attribute.TryConvert(ctx, call, out var result).ShouldBeTrue();
            result.ShouldEqual(MemberExpressionNode.Get(ConstantExpressionNode.Get(""), name));
        }

        [BindingMember(MethodName)]
        public static string StaticMethod()
        {
            return "";
        }

        [BindingMember(0)]
        public static string StaticMemberMethod(string name)
        {
            return name;
        }

        [BindingMember(MethodName)]
        public string Method()
        {
            return "";
        }

        [BindingMember(0)]
        public string MemberMethod(string name)
        {
            return name;
        }

        #endregion
    }
}