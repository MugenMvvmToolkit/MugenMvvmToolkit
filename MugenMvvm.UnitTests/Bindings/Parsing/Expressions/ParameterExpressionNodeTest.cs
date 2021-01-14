using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.UnitTests.Bindings.Parsing.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Expressions
{
    public class ParameterExpressionNodeTest : UnitTestBase
    {
        private const string Name = "test";

        [Fact]
        public void AcceptShouldCreateNewNode2()
        {
            var newNode = new ParameterExpressionNode(Name);
            var visitor = new TestExpressionVisitor
            {
                Visit = (node, context) => newNode
            };
            new ParameterExpressionNode("1").Accept(visitor, DefaultMetadata).ShouldEqual(newNode);
        }

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            var exp = new ParameterExpressionNode(Name);
            exp.ExpressionType.ShouldEqual(ExpressionNodeType.Parameter);
            exp.Name.ShouldEqual(Name);
            exp.ToString().ShouldEqual(Name);
        }

        [Fact]
        public void GetHashCodeEqualsShouldBeValid()
        {
            var exp1 = new ParameterExpressionNode("1", new Dictionary<string, object?> {{"k", null}});
            var exp2 = new ParameterExpressionNode("1", new Dictionary<string, object?> {{"k", null}});
            HashCode.Combine(GetBaseHashCode(exp1), exp1.Name).ShouldEqual(exp1.GetHashCode());

            exp1.Equals(exp2).ShouldBeTrue();
            exp1.Equals(exp2.UpdateMetadata(null)).ShouldBeFalse();
            exp1.Equals(new ParameterExpressionNode("2", exp1.Metadata)).ShouldBeFalse();
        }

        [Theory]
        [InlineData(ExpressionTraversalType.InorderValue)]
        [InlineData(ExpressionTraversalType.PreorderValue)]
        [InlineData(ExpressionTraversalType.PostorderValue)]
        public void AcceptShouldVisitWithCorrectOrder(int value)
        {
            var nodes = new List<IExpressionNode>();
            var visitor = new TestExpressionVisitor
            {
                Visit = (node, context) =>
                {
                    nodes.Add(node);
                    context.ShouldEqual(DefaultMetadata);
                    return node;
                },
                TraversalType = ExpressionTraversalType.Get(value)
            };

            var exp = new ParameterExpressionNode(Name);
            var result = new IExpressionNode[] {exp};
            exp.Accept(visitor, DefaultMetadata).ShouldEqual(exp);
            result.ShouldEqual(nodes);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void UpdateMetadataShouldCheckMetadataEquality(bool equal)
        {
            var node = new ParameterExpressionNode(Name, EmptyDictionary);
            if (equal)
                node.UpdateMetadata(EmptyDictionary).ShouldEqual(node, ReferenceEqualityComparer.Instance);
            else
            {
                var metadata = new Dictionary<string, object?> {{"k", null}};
                var updated = (ParameterExpressionNode) node.UpdateMetadata(metadata);
                updated.ShouldNotEqual(node, ReferenceEqualityComparer.Instance);
                updated.Metadata.ShouldEqual(metadata);
                updated.Name.ShouldEqual(node.Name);
            }
        }
    }
}