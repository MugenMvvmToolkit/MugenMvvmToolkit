using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.Tests.Bindings.Parsing;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Expressions
{
    public class ConstantExpressionNodeTest : UnitTestBase
    {
        [Fact]
        public void AcceptShouldCreateNewNode()
        {
            var newNode = new ConstantExpressionNode("-");
            var visitor = new TestExpressionVisitor
            {
                Visit = (node, context) => newNode
            };
            new ConstantExpressionNode("1").Accept(visitor, DefaultMetadata).ShouldEqual(newNode);
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

            var exp = new ConstantExpressionNode("-");
            var result = new IExpressionNode[] { exp };
            exp.Accept(visitor, DefaultMetadata).ShouldEqual(exp);
            result.ShouldEqual(nodes);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(typeof(string))]
        public void ConstructorShouldInitializeValues(Type type)
        {
            var value = "d";
            var constantExpression = Expression.Constant(value);
            var exp = new ConstantExpressionNode(value, type, constantExpression);
            exp.ExpressionType.ShouldEqual(ExpressionNodeType.Constant);
            exp.Value.ShouldEqual(value);
            exp.Type.ShouldEqual(value.GetType());
            exp.ConstantExpression.ShouldEqual(constantExpression);
            exp.ToString().ShouldEqual("\"d\"");
        }

        [Fact]
        public void ConstructorShouldThrowWrongType() => ShouldThrow<ArgumentException>(() => new ConstantExpressionNode("", typeof(int)));

        [Fact]
        public void GetBoolShouldUseCache()
        {
            ConstantExpressionNode.Get(true).ShouldEqual(ConstantExpressionNode.True);
            ConstantExpressionNode.Get(false).ShouldEqual(ConstantExpressionNode.False);
        }

        [Fact]
        public void GetHashCodeEqualsShouldBeValid()
        {
            var exp1 = new ConstantExpressionNode("1", metadata: new Dictionary<string, object?> { { "k", null } });
            var exp2 = new ConstantExpressionNode("1", metadata: new Dictionary<string, object?> { { "k", null } });
            HashCode.Combine(GetBaseHashCode(exp1), exp1.Type, exp1.Value).ShouldEqual(exp1.GetHashCode());

            exp1.Equals(exp2).ShouldBeTrue();
            exp1.Equals(exp2.UpdateMetadata(null)).ShouldBeFalse();
            exp1.Equals(new ConstantExpressionNode("2", metadata: exp1.Metadata)).ShouldBeFalse();
        }

        [Fact]
        public void GetIntShouldUseCache()
        {
            var nodes = new HashSet<ConstantExpressionNode>(ReferenceEqualityComparer.Instance);
            for (var i = -BoxingExtensions.CacheSize; i < BoxingExtensions.CacheSize; i++)
            {
                var expressionNode = ConstantExpressionNode.Get(i);
                expressionNode.Type.ShouldEqual(i.GetType());
                expressionNode.ConstantExpression!.Value.ShouldEqual(i);
                expressionNode.Value.ShouldEqual(i);

                nodes.Add(expressionNode);
                nodes.Add(ConstantExpressionNode.Get(i));
            }

            nodes.Count.ShouldEqual(BoxingExtensions.CacheSize * 2);

            nodes.Clear();
            nodes.Add(ConstantExpressionNode.Get(BoxingExtensions.CacheSize + 1));
            nodes.Add(ConstantExpressionNode.Get(BoxingExtensions.CacheSize + 1));
            nodes.Count.ShouldEqual(2);
        }

        [Fact]
        public void GetShouldUseCacheBool()
        {
            ConstantExpressionNode.Get((object)true).ShouldEqual(ConstantExpressionNode.True);
            ConstantExpressionNode.Get((object)false).ShouldEqual(ConstantExpressionNode.False);
        }

        [Fact]
        public void GetShouldUseCacheNull()
        {
            ConstantExpressionNode.Get(null, typeof(object)).ShouldEqual(ConstantExpressionNode.Null);
            ConstantExpressionNode.Get(null).ShouldEqual(ConstantExpressionNode.Null);
            var constantExpressionNode = ConstantExpressionNode.Get(null, typeof(string));
            constantExpressionNode.Type.ShouldEqual(typeof(string));
            constantExpressionNode.Value.ShouldEqual(null);
        }

        [Fact]
        public void StaticFieldsShouldBeCorrect()
        {
            ConstantExpressionNode.EmptyString.Type.ShouldEqual(typeof(string));
            ConstantExpressionNode.EmptyString.Value.ShouldEqual("");
            ConstantExpressionNode.EmptyString.ConstantExpression!.Value.ShouldEqual("");

            ConstantExpressionNode.True.Type.ShouldEqual(typeof(bool));
            ConstantExpressionNode.True.Value.ShouldEqual(true);
            ConstantExpressionNode.True.ConstantExpression!.Value.ShouldEqual(true);

            ConstantExpressionNode.False.Type.ShouldEqual(typeof(bool));
            ConstantExpressionNode.False.Value.ShouldEqual(false);
            ConstantExpressionNode.False.ConstantExpression!.Value.ShouldEqual(false);

            ConstantExpressionNode.Null.Type.ShouldEqual(typeof(object));
            ConstantExpressionNode.Null.Value.ShouldEqual(null);
            ConstantExpressionNode.Null.ConstantExpression!.Value.ShouldBeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void UpdateMetadataShouldCheckMetadataEquality(bool equal)
        {
            var node = new ConstantExpressionNode("1", null, null, EmptyDictionary);
            if (equal)
                node.UpdateMetadata(EmptyDictionary).ShouldEqual(node, ReferenceEqualityComparer.Instance);
            else
            {
                var metadata = new Dictionary<string, object?> { { "k", null } };
                var updated = (ConstantExpressionNode)node.UpdateMetadata(metadata);
                updated.ShouldNotEqual(node, ReferenceEqualityComparer.Instance);
                updated.Metadata.ShouldEqual(metadata);
                updated.Type.ShouldEqual(node.Type);
                updated.Value.ShouldEqual(node.Value);
                updated.ConstantExpression.ShouldEqual(node.ConstantExpression);
            }
        }
    }
}