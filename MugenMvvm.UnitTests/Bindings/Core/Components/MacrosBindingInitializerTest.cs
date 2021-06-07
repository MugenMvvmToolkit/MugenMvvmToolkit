using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Core.Components;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.Tests.Bindings.Parsing;
using MugenMvvm.UnitTests.Bindings.Parsing.Internal;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Core.Components
{
    public class MacrosBindingInitializerTest : UnitTestBase
    {
        private readonly BindingExpressionInitializerContext _context;
        private readonly MacrosBindingInitializer _initializer;

        public MacrosBindingInitializerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _context = new BindingExpressionInitializerContext(this);
            _initializer = new MacrosBindingInitializer();
        }

        [Fact]
        public void ParameterVisitorsShouldUpdateSourceExpression()
        {
            var target = new MemberExpressionNode(null, "1");
            var source = new MemberExpressionNode(null, "2");
            var parameters = new IExpressionNode[] {new MemberExpressionNode(null, "3"), new MemberExpressionNode(null, "4"), new MemberExpressionNode(null, "5")};
            var handledParameters = new List<IExpressionNode>();
            var newNode = ConstantExpressionNode.Get(1);

            _context.Initialize(this, this, target, source, parameters.ToList(), DefaultMetadata);
            _initializer.ParameterVisitors.Add(new TestExpressionVisitor
            {
                TraversalType = ExpressionTraversalType.Postorder,
                Visit = (node, context) =>
                {
                    if (node == newNode)
                        return node;
                    handledParameters.Add(node);
                    context.ShouldEqual(_context.GetMetadataOrDefault());
                    return newNode;
                }
            });

            _initializer.Initialize(null!, _context);
            _context.TargetExpression.ShouldEqual(target);
            _context.SourceExpression.ShouldEqual(source);
            _context.ParameterExpressions.AsList().ShouldEqual(new[] {newNode, newNode, newNode});
            handledParameters.ShouldEqual(parameters);
        }

        [Fact]
        public void SourceVisitorsShouldUpdateSourceExpression()
        {
            var target = new MemberExpressionNode(null, "1");
            var source = new MemberExpressionNode(null, "2");
            var parameters = new IExpressionNode[] {new MemberExpressionNode(null, "3"), new MemberExpressionNode(null, "4"), new MemberExpressionNode(null, "5")};
            var newNode = ConstantExpressionNode.Get(1);

            _context.Initialize(this, this, target, source, parameters, DefaultMetadata);
            _initializer.SourceVisitors.Add(new TestExpressionVisitor
            {
                TraversalType = ExpressionTraversalType.Postorder,
                Visit = (node, context) =>
                {
                    if (node == newNode)
                        return node;
                    node.ShouldEqual(source);
                    context.ShouldEqual(_context.GetMetadataOrDefault());
                    return newNode;
                }
            });

            _initializer.Initialize(null!, _context);
            _context.TargetExpression.ShouldEqual(target);
            _context.SourceExpression.ShouldEqual(newNode);
            _context.ParameterExpressions.AsList().ShouldEqual(parameters);
        }

        [Fact]
        public void TargetVisitorsShouldUpdateTargetExpression()
        {
            var target = new MemberExpressionNode(null, "1");
            var source = new MemberExpressionNode(null, "2");
            var parameters = new IExpressionNode[] {new MemberExpressionNode(null, "3"), new MemberExpressionNode(null, "4"), new MemberExpressionNode(null, "5")};
            var newNode = ConstantExpressionNode.Get(1);

            _context.Initialize(this, this, target, source, parameters, DefaultMetadata);
            _initializer.TargetVisitors.Add(new TestExpressionVisitor
            {
                TraversalType = ExpressionTraversalType.Postorder,
                Visit = (node, context) =>
                {
                    if (node == newNode)
                        return node;
                    node.ShouldEqual(target);
                    context.ShouldEqual(_context.GetMetadataOrDefault());
                    return newNode;
                }
            });

            _initializer.Initialize(null!, _context);
            _context.TargetExpression.ShouldEqual(newNode);
            _context.SourceExpression.ShouldEqual(source);
            _context.ParameterExpressions.AsList().ShouldEqual(parameters);
        }
    }
}