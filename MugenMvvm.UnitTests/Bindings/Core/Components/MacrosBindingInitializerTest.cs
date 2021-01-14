using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Core.Components;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Bindings.Parsing.Internal;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Core.Components
{
    public class MacrosBindingInitializerTest : UnitTestBase
    {
        [Fact]
        public void ParameterVisitorsShouldUpdateSourceExpression()
        {
            var target = new MemberExpressionNode(null, "1");
            var source = new MemberExpressionNode(null, "2");
            var parameters = new IExpressionNode[] {new MemberExpressionNode(null, "3"), new MemberExpressionNode(null, "4"), new MemberExpressionNode(null, "5")};
            var handledParameters = new List<IExpressionNode>();
            var newNode = ConstantExpressionNode.Get(1);
            var ctx = new BindingExpressionInitializerContext(this);
            ctx.Initialize(this, this, target, source, parameters.ToList(), DefaultMetadata);

            var initializer = new MacrosBindingInitializer();
            initializer.ParameterVisitors.Add(new TestExpressionVisitor
            {
                TraversalType = ExpressionTraversalType.Postorder,
                Visit = (node, context) =>
                {
                    if (node == newNode)
                        return node;
                    handledParameters.Add(node);
                    context.ShouldEqual(ctx.GetMetadataOrDefault());
                    return newNode;
                }
            });

            initializer.Initialize(null!, ctx);
            ctx.TargetExpression.ShouldEqual(target);
            ctx.SourceExpression.ShouldEqual(source);
            ctx.ParameterExpressions.AsList().ShouldEqual(new[] {newNode, newNode, newNode});
            handledParameters.ShouldEqual(parameters);
        }

        [Fact]
        public void SourceVisitorsShouldUpdateSourceExpression()
        {
            var target = new MemberExpressionNode(null, "1");
            var source = new MemberExpressionNode(null, "2");
            var parameters = new IExpressionNode[] {new MemberExpressionNode(null, "3"), new MemberExpressionNode(null, "4"), new MemberExpressionNode(null, "5")};
            var newNode = ConstantExpressionNode.Get(1);
            var ctx = new BindingExpressionInitializerContext(this);
            ctx.Initialize(this, this, target, source, parameters, DefaultMetadata);

            var initializer = new MacrosBindingInitializer();
            initializer.SourceVisitors.Add(new TestExpressionVisitor
            {
                TraversalType = ExpressionTraversalType.Postorder,
                Visit = (node, context) =>
                {
                    if (node == newNode)
                        return node;
                    node.ShouldEqual(source);
                    context.ShouldEqual(ctx.GetMetadataOrDefault());
                    return newNode;
                }
            });

            initializer.Initialize(null!, ctx);
            ctx.TargetExpression.ShouldEqual(target);
            ctx.SourceExpression.ShouldEqual(newNode);
            ctx.ParameterExpressions.AsList().ShouldEqual(parameters);
        }

        [Fact]
        public void TargetVisitorsShouldUpdateTargetExpression()
        {
            var target = new MemberExpressionNode(null, "1");
            var source = new MemberExpressionNode(null, "2");
            var parameters = new IExpressionNode[] {new MemberExpressionNode(null, "3"), new MemberExpressionNode(null, "4"), new MemberExpressionNode(null, "5")};
            var newNode = ConstantExpressionNode.Get(1);
            var ctx = new BindingExpressionInitializerContext(this);
            ctx.Initialize(this, this, target, source, parameters, DefaultMetadata);

            var initializer = new MacrosBindingInitializer();
            initializer.TargetVisitors.Add(new TestExpressionVisitor
            {
                TraversalType = ExpressionTraversalType.Postorder,
                Visit = (node, context) =>
                {
                    if (node == newNode)
                        return node;
                    node.ShouldEqual(target);
                    context.ShouldEqual(ctx.GetMetadataOrDefault());
                    return newNode;
                }
            });

            initializer.Initialize(null!, ctx);
            ctx.TargetExpression.ShouldEqual(newNode);
            ctx.SourceExpression.ShouldEqual(source);
            ctx.ParameterExpressions.AsList().ShouldEqual(parameters);
        }
    }
}