using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Bindings.Parsing.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Expressions
{
    public class ConstantExpressionNodeTest : UnitTestBase
    {
        #region Methods

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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AcceptShouldVisitWithCorrectOrder(bool isPostOrder)
        {
            var nodes = new List<IExpressionNode>();
            var testExpressionVisitor = new TestExpressionVisitor
            {
                Visit = (node, context) =>
                {
                    nodes.Add(node);
                    context.ShouldEqual(DefaultMetadata);
                    return node;
                },
                IsPostOrder = isPostOrder
            };

            var exp = new ConstantExpressionNode("-");
            var result = new IExpressionNode[] {exp};
            exp.Accept(testExpressionVisitor, DefaultMetadata).ShouldEqual(exp);
            result.ShouldEqual(nodes);
        }

        [Fact]
        public void AcceptShouldCreateNewNode2()
        {
            var newNode = new ConstantExpressionNode("-");
            var testExpressionVisitor = new TestExpressionVisitor
            {
                Visit = (node, context) => newNode
            };
            new ConstantExpressionNode("1").Accept(testExpressionVisitor, DefaultMetadata).ShouldEqual(newNode);
        }

        [Fact]
        public void GetTypeShouldUseCache()
        {
            var exp = ConstantExpressionNode.Get<string>();
            exp.Type.ShouldEqual(typeof(string).GetType());
            exp.ConstantExpression!.Value.ShouldEqual(typeof(string));
            exp.Value.ShouldEqual(typeof(string));
            exp.ShouldEqual(ConstantExpressionNode.Get<string>());
        }

        [Fact]
        public void GetBoolShouldUseCache()
        {
            ConstantExpressionNode.Get(true).ShouldEqual(ConstantExpressionNode.True);
            ConstantExpressionNode.Get(false).ShouldEqual(ConstantExpressionNode.False);
        }

        [Fact]
        public void GetShouldUseCacheBool()
        {
            ConstantExpressionNode.Get((object) true).ShouldEqual(ConstantExpressionNode.True);
            ConstantExpressionNode.Get((object) false).ShouldEqual(ConstantExpressionNode.False);
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
        public void GetIntShouldUseCache()
        {
            var nodes = new HashSet<ConstantExpressionNode>();
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

        #endregion
    }
}