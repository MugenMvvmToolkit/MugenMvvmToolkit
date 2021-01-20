using System.Collections.Generic;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Members.Components;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Visitors;
using MugenMvvm.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Visitors
{
    public class MacrosExpressionVisitorTest : UnitTestBase
    {
        private const string MemberName1 = "T1";
        private const string MemberName2 = "T2";
        private const string MemberName3 = "T3";

        [Fact]
        public void VisitorShouldConvertMacros()
        {
            var visitor = new MacrosExpressionVisitor();
            visitor.Macros.ShouldNotBeEmpty();

            foreach (var member in visitor.Macros)
            {
                var expressionNode = new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, member.Key)).Accept(visitor);
                if (member.Key == MacrosConstant.Action)
                    expressionNode.ShouldEqual(new MemberExpressionNode(null, FakeMemberProvider.FakeMemberPrefixSymbol + (Default.NextCounter() - 1).ToString()));
                else
                    expressionNode.ShouldEqual(member.Value(DefaultMetadata));
            }
        }

        [Fact]
        public void VisitorShouldConvertMacrosTarget()
        {
            var visitor = new MacrosExpressionVisitor();
            visitor.MacrosTargets.ShouldNotBeEmpty();

            var args = new IExpressionNode[]
                {new MemberExpressionNode(null, MemberName1), new MemberExpressionNode(new MemberExpressionNode(null, MemberName2), MemberName3), ConstantExpressionNode.Get(1)};
            var constantArgs = new IExpressionNode[]
                {ConstantExpressionNode.Get(MemberName1), ConstantExpressionNode.Get($"{MemberName2}.{MemberName3}"), ConstantExpressionNode.Get(1)};
            foreach (var member in visitor.MacrosTargets)
            {
                var arguments = args;
                if (visitor.AccessorMethods.TryGetValue(member.Key, out var newName))
                    arguments = constantArgs;
                new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MethodCallExpressionNode(null, member.Key, args))
                    .Accept(visitor)
                    .ShouldEqual(new MethodCallExpressionNode(member.Value, newName ?? member.Key, arguments, default, new Dictionary<string, object?>
                    {
                        {BindingParameterNameConstant.SuppressMethodAccessors, false}
                    }));
            }
        }

        [Fact]
        public void VisitorShouldConvertMethodAliases()
        {
            var visitor = new MacrosExpressionVisitor();
            visitor.MethodAliases.ShouldNotBeEmpty();

            foreach (var member in visitor.MethodAliases)
            {
                var methodCallExpressionNode = new MethodCallExpressionNode(null, member.Key,
                    new IExpressionNode[]
                    {
                        new MemberExpressionNode(null, MemberName1), new MemberExpressionNode(new MemberExpressionNode(null, MemberName2), MemberName3),
                        ConstantExpressionNode.Get(1)
                    });
                new UnaryExpressionNode(UnaryTokenType.DynamicExpression, methodCallExpressionNode)
                    .Accept(visitor)
                    .ShouldEqual(member.Value.UpdateArguments(methodCallExpressionNode.Arguments));
            }
        }

        [Fact]
        public void VisitorShouldConvertParametersToConstant()
        {
            var visitor = new MacrosExpressionVisitor();
            visitor.AccessorMethods.ShouldNotBeEmpty();

            foreach (var method in visitor.AccessorMethods)
            {
                new MethodCallExpressionNode(null, method.Key, default)
                    .Accept(visitor)
                    .ShouldEqual(new MethodCallExpressionNode(null, method.Value, default, default, new Dictionary<string, object?>
                    {
                        {BindingParameterNameConstant.SuppressMethodAccessors, false}
                    }));
            }

            var metadata = new Dictionary<string, object?>
            {
                {"test", this},
                {BindingParameterNameConstant.SuppressMethodAccessors, true}
            };
            var mergedMetadata = new Dictionary<string, object?>
            {
                {"test", this},
                {BindingParameterNameConstant.SuppressMethodAccessors, false}
            };
            foreach (var method in visitor.AccessorMethods)
            {
                new MethodCallExpressionNode(null, method.Key, default, default, metadata)
                    .Accept(visitor)
                    .ShouldEqual(new MethodCallExpressionNode(null, method.Value, default, default, mergedMetadata));
            }

            var args = new IExpressionNode[]
                {new MemberExpressionNode(null, MemberName1), new MemberExpressionNode(new MemberExpressionNode(null, MemberName2), MemberName3), ConstantExpressionNode.Get(1)};
            var expectedArgs = new IExpressionNode[]
                {ConstantExpressionNode.Get(MemberName1), ConstantExpressionNode.Get($"{MemberName2}.{MemberName3}"), ConstantExpressionNode.Get(1)};
            foreach (var method in visitor.AccessorMethods)
            {
                new MethodCallExpressionNode(null, method.Key, args)
                    .Accept(visitor)
                    .ShouldEqual(new MethodCallExpressionNode(null, method.Value, expectedArgs, default, new Dictionary<string, object?>
                    {
                        {BindingParameterNameConstant.SuppressMethodAccessors, false}
                    }));
            }
        }
    }
}