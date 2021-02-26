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
    public sealed class TypeAccessExpressionNodeTest : UnitTestBase
    {
        [Fact]
        public void AcceptShouldCreateNewNode()
        {
            var newNode = new TypeAccessExpressionNode(typeof(object));
            var visitor = new TestExpressionVisitor
            {
                Visit = (node, context) => newNode
            };
            new TypeAccessExpressionNode(typeof(string)).Accept(visitor, DefaultMetadata).ShouldEqual(newNode);
        }

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            var type = typeof(object);
            var exp = new TypeAccessExpressionNode(type);
            exp.ExpressionType.ShouldEqual(ExpressionNodeType.TypeAccess);
            exp.Type.ShouldEqual(type);
            exp.ToString().ShouldEqual(type.ToString());
        }

        [Fact]
        public void ConstructorShouldThrowNullType() => ShouldThrow<ArgumentNullException>(() => new TypeAccessExpressionNode(null!));

        [Fact]
        public void GetHashCodeEqualsShouldBeValid()
        {
            var exp1 = new TypeAccessExpressionNode(typeof(string), new Dictionary<string, object?> {{"k", null}});
            var exp2 = new TypeAccessExpressionNode(typeof(string), new Dictionary<string, object?> {{"k", null}});
            HashCode.Combine(GetBaseHashCode(exp1), exp1.Type).ShouldEqual(exp1.GetHashCode());

            exp1.Equals(exp2).ShouldBeTrue();
            exp1.Equals(exp2.UpdateMetadata(null)).ShouldBeFalse();
            exp1.Equals(new ConstantExpressionNode("2", metadata: exp1.Metadata)).ShouldBeFalse();
        }

        [Fact]
        public void GetTypeShouldUseCache()
        {
            var exp = TypeAccessExpressionNode.Get<string>();
            exp.Type.ShouldEqual(typeof(string));
            exp.ShouldBeSameAs(TypeAccessExpressionNode.Get<string>());
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

            var exp = new TypeAccessExpressionNode(typeof(object));
            var result = new IExpressionNode[] {exp};
            exp.Accept(visitor, DefaultMetadata).ShouldEqual(exp);
            result.ShouldEqual(nodes);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void UpdateMetadataShouldCheckMetadataEquality(bool equal)
        {
            var node = new TypeAccessExpressionNode(typeof(object), EmptyDictionary);
            if (equal)
                node.UpdateMetadata(EmptyDictionary).ShouldEqual(node, ReferenceEqualityComparer.Instance);
            else
            {
                var metadata = new Dictionary<string, object?> {{"k", null}};
                var updated = (TypeAccessExpressionNode) node.UpdateMetadata(metadata);
                updated.ShouldNotEqual(node, ReferenceEqualityComparer.Instance);
                updated.Metadata.ShouldEqual(metadata);
                updated.Type.ShouldEqual(node.Type);
            }
        }
    }
}