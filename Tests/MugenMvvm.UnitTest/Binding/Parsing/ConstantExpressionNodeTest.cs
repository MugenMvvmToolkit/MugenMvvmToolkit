using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Extensions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Parsing
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
            ConstantExpressionNode.Null.ConstantExpression!.Value.ShouldEqual(null);
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
        }

        [Fact]
        public void ConstructorShouldThrowWrongType()
        {
            ShouldThrow<ArgumentException>(() => new ConstantExpressionNode("", typeof(int)));
        }

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
            var result = new IExpressionNode[] { exp };
            exp.Accept(testExpressionVisitor, DefaultMetadata).ShouldEqual(exp);
            result.SequenceEqual(nodes).ShouldBeTrue();
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
            exp.Type.ShouldEqual(typeof(Type));
            exp.ConstantExpression.Value.ShouldEqual(typeof(string));
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
        public void GetByteShouldUseCache()
        {
            var nodes = new HashSet<ConstantExpressionNode>();
            for (byte i = 0; i < byte.MaxValue; i++)
            {
                var expressionNode = ConstantExpressionNode.Get(i);
                expressionNode.Type.ShouldEqual(i.GetType());
                expressionNode.ConstantExpression.Value.ShouldEqual(i);
                expressionNode.Value.ShouldEqual(i);

                nodes.Add(expressionNode);
                nodes.Add(ConstantExpressionNode.Get(i));
            }

            nodes.Count.ShouldEqual(byte.MaxValue);
        }

        [Fact]
        public void GetSByteShouldUseCache()
        {
            var nodes = new HashSet<ConstantExpressionNode>();
            for (var i = sbyte.MinValue; i < sbyte.MaxValue; i++)
            {
                var expressionNode = ConstantExpressionNode.Get(i);
                expressionNode.Type.ShouldEqual(i.GetType());
                expressionNode.ConstantExpression.Value.ShouldEqual(i);
                expressionNode.Value.ShouldEqual(i);

                nodes.Add(expressionNode);
                nodes.Add(ConstantExpressionNode.Get(i));
            }

            nodes.Count.ShouldEqual(byte.MaxValue);
        }

        [Fact]
        public void GetUShortShouldUseCache()
        {
            var nodes = new HashSet<ConstantExpressionNode>();
            for (ushort i = 0; i < BoxingExtensions.CacheSize; i++)
            {
                var expressionNode = ConstantExpressionNode.Get(i);
                expressionNode.Type.ShouldEqual(i.GetType());
                expressionNode.ConstantExpression.Value.ShouldEqual(i);
                expressionNode.Value.ShouldEqual(i);

                nodes.Add(expressionNode);
                nodes.Add(ConstantExpressionNode.Get(i));
            }

            nodes.Count.ShouldEqual(BoxingExtensions.CacheSize);

            nodes.Clear();
            nodes.Add(ConstantExpressionNode.Get((ushort)(BoxingExtensions.CacheSize + 1)));
            nodes.Add(ConstantExpressionNode.Get((ushort)(BoxingExtensions.CacheSize + 1)));
            nodes.Count.ShouldEqual(2);
        }

        [Fact]
        public void GetShortShouldUseCache()
        {
            var nodes = new HashSet<ConstantExpressionNode>();
            for (short i = -BoxingExtensions.CacheSize; i < BoxingExtensions.CacheSize; i++)
            {
                var expressionNode = ConstantExpressionNode.Get(i);
                expressionNode.Type.ShouldEqual(i.GetType());
                expressionNode.ConstantExpression.Value.ShouldEqual(i);
                expressionNode.Value.ShouldEqual(i);

                nodes.Add(expressionNode);
                nodes.Add(ConstantExpressionNode.Get(i));
            }

            nodes.Count.ShouldEqual(BoxingExtensions.CacheSize * 2);

            nodes.Clear();
            nodes.Add(ConstantExpressionNode.Get((short)(BoxingExtensions.CacheSize + 1)));
            nodes.Add(ConstantExpressionNode.Get((short)(BoxingExtensions.CacheSize + 1)));
            nodes.Count.ShouldEqual(2);
        }

        [Fact]
        public void GetUIntShouldUseCache()
        {
            var nodes = new HashSet<ConstantExpressionNode>();
            for (uint i = 0; i < BoxingExtensions.CacheSize; i++)
            {
                var expressionNode = ConstantExpressionNode.Get(i);
                expressionNode.Type.ShouldEqual(i.GetType());
                expressionNode.ConstantExpression.Value.ShouldEqual(i);
                expressionNode.Value.ShouldEqual(i);

                nodes.Add(expressionNode);
                nodes.Add(ConstantExpressionNode.Get(i));
            }

            nodes.Count.ShouldEqual(BoxingExtensions.CacheSize);

            nodes.Clear();
            nodes.Add(ConstantExpressionNode.Get((uint)(BoxingExtensions.CacheSize + 1)));
            nodes.Add(ConstantExpressionNode.Get((uint)(BoxingExtensions.CacheSize + 1)));
            nodes.Count.ShouldEqual(2);
        }

        [Fact]
        public void GetIntShouldUseCache()
        {
            var nodes = new HashSet<ConstantExpressionNode>();
            for (var i = -BoxingExtensions.CacheSize; i < BoxingExtensions.CacheSize; i++)
            {
                var expressionNode = ConstantExpressionNode.Get(i);
                expressionNode.Type.ShouldEqual(i.GetType());
                expressionNode.ConstantExpression.Value.ShouldEqual(i);
                expressionNode.Value.ShouldEqual(i);

                nodes.Add(expressionNode);
                nodes.Add(ConstantExpressionNode.Get(i));
            }

            nodes.Count.ShouldEqual(BoxingExtensions.CacheSize * 2);

            nodes.Clear();
            nodes.Add(ConstantExpressionNode.Get((int)(BoxingExtensions.CacheSize + 1)));
            nodes.Add(ConstantExpressionNode.Get((int)(BoxingExtensions.CacheSize + 1)));
            nodes.Count.ShouldEqual(2);
        }

        [Fact]
        public void GetULongShouldUseCache()
        {
            var nodes = new HashSet<ConstantExpressionNode>();
            for (ulong i = 0; i < BoxingExtensions.CacheSize; i++)
            {
                var expressionNode = ConstantExpressionNode.Get(i);
                expressionNode.Type.ShouldEqual(i.GetType());
                expressionNode.ConstantExpression.Value.ShouldEqual(i);
                expressionNode.Value.ShouldEqual(i);

                nodes.Add(expressionNode);
                nodes.Add(ConstantExpressionNode.Get(i));
            }

            nodes.Count.ShouldEqual(BoxingExtensions.CacheSize);

            nodes.Clear();
            nodes.Add(ConstantExpressionNode.Get((ulong)(BoxingExtensions.CacheSize + 1)));
            nodes.Add(ConstantExpressionNode.Get((ulong)(BoxingExtensions.CacheSize + 1)));
            nodes.Count.ShouldEqual(2);
        }

        [Fact]
        public void GetLongShouldUseCache()
        {
            var nodes = new HashSet<ConstantExpressionNode>();
            for (long i = -BoxingExtensions.CacheSize; i < BoxingExtensions.CacheSize; i++)
            {
                var expressionNode = ConstantExpressionNode.Get(i);
                expressionNode.Type.ShouldEqual(i.GetType());
                expressionNode.ConstantExpression.Value.ShouldEqual(i);
                expressionNode.Value.ShouldEqual(i);

                nodes.Add(expressionNode);
                nodes.Add(ConstantExpressionNode.Get(i));
            }

            nodes.Count.ShouldEqual(BoxingExtensions.CacheSize * 2);

            nodes.Clear();
            nodes.Add(ConstantExpressionNode.Get((long)(BoxingExtensions.CacheSize + 1)));
            nodes.Add(ConstantExpressionNode.Get((long)(BoxingExtensions.CacheSize + 1)));
            nodes.Count.ShouldEqual(2);
        }

        #endregion
    }
}