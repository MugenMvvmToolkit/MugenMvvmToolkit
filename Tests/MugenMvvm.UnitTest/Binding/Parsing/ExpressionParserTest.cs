using System;
using System.Text;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing;
using MugenMvvm.Binding.Parsing.Components;
using MugenMvvm.Binding.Parsing.Components.Parsers;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.UnitTest.Binding.Parsing.Internal;
using MugenMvvm.UnitTest.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Parsing
{
    public class ExpressionParserTest : ComponentOwnerTestBase<ExpressionParser>
    {
        #region Methods

        [Fact]
        public void ParseMemberPathShouldThrowEmpty()
        {
            var parser = new ExpressionParser();
            ShouldThrow<InvalidOperationException>(() => parser.Parse(this, DefaultMetadata));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ParseShouldBeHandledByComponents(int componentCount)
        {
            var parser = new ExpressionParser();
            var request = this;
            var result = new ExpressionParserResult(MemberExpressionNode.Source, ConstantExpressionNode.EmptyString, default);
            var invokeCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var isLast = i == componentCount - 1;
                var component = new TestExpressionParserComponent
                {
                    Priority = -i,
                    TryParse = (o, arg3, arg4) =>
                    {
                        ++invokeCount;
                        o.ShouldEqual(request);
                        arg3.ShouldEqual(request.GetType());
                        arg4.ShouldEqual(DefaultMetadata);
                        if (isLast)
                            return result;
                        return null;
                    }
                };
                parser.AddComponent(component);
            }

            parser.Parse(request, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(componentCount);
        }

        [Theory]
        [InlineData(1, 0)]
        [InlineData(1, 1)]
        [InlineData(1, 5)]
        [InlineData(5, 0)]
        [InlineData(5, 1)]
        [InlineData(5, 5)]
        public void ParseShouldParseExpression1(int count, int parameterCount)
        {
            var expectedResult = new ConditionExpressionNode(
                new BinaryExpressionNode(BinaryTokenType.Equality,
                    new MethodCallExpressionNode(ConstantExpressionNode.Get("1"), "IndexOf", new IExpressionNode[] { ConstantExpressionNode.Get("1", typeof(string)) }, new string[0]),
                    ConstantExpressionNode.Get(0, typeof(int))),
                new MethodCallExpressionNode(ConstantExpressionNode.Get(typeof(string)), "Format",
                    new IExpressionNode[] { ConstantExpressionNode.Get("{0} - {1}", typeof(string)), ConstantExpressionNode.Get(1), ConstantExpressionNode.Get(2) }, new string[0]),
                new ConditionExpressionNode(new BinaryExpressionNode(BinaryTokenType.GreaterThanOrEqual, ConstantExpressionNode.Get(2), ConstantExpressionNode.Get(10, typeof(int))),
                    ConstantExpressionNode.Get("test", typeof(string)),
                    new BinaryExpressionNode(BinaryTokenType.NullCoalescing, ConstantExpressionNode.Get(null, typeof(object)), ConstantExpressionNode.Get("value", typeof(string)))));
            var source = "'1'.IndexOf(\"1\") == 0 ? $\"{1} - {2}\" : 2 >= 10 ? \"test\" : null ?? \"value\"";
            ValidateExpression(source, expectedResult, count, parameterCount);
        }

        [Theory]
        [InlineData(1, 0)]
        [InlineData(1, 1)]
        [InlineData(1, 5)]
        [InlineData(5, 0)]
        [InlineData(5, 1)]
        [InlineData(5, 5)]
        public void ParseShouldParseExpression2(int count, int parameterCount)
        {
            var expectedResult = new BinaryExpressionNode(BinaryTokenType.GreaterThanOrEqual,
                new BinaryExpressionNode(BinaryTokenType.Addition,
                    new BinaryExpressionNode(BinaryTokenType.Multiplication,
                        new BinaryExpressionNode(BinaryTokenType.Subtraction, ConstantExpressionNode.Get(1),
                            new BinaryExpressionNode(BinaryTokenType.Division, ConstantExpressionNode.Get(2), ConstantExpressionNode.Get(3))), ConstantExpressionNode.Get(1)),
                    new BinaryExpressionNode(BinaryTokenType.Subtraction, new BinaryExpressionNode(BinaryTokenType.Multiplication, ConstantExpressionNode.Get(1000, typeof(int)), ConstantExpressionNode.Get(1)),
                        ConstantExpressionNode.Get(1, typeof(int)))), ConstantExpressionNode.Get(100, typeof(int)));
            var source = "((1 - (2 / 3)) * 1) + ((1000 * 1) - 1) >= 100";
            ValidateExpression(source, expectedResult, count, parameterCount);
        }

        [Theory]
        [InlineData(1, 0)]
        [InlineData(1, 1)]
        [InlineData(1, 5)]
        [InlineData(5, 0)]
        [InlineData(5, 1)]
        [InlineData(5, 5)]
        public void ParseShouldParseExpression3(int count, int parameterCount)
        {
            var expectedResult = new ConditionExpressionNode(
                new BinaryExpressionNode(BinaryTokenType.Equality,
                    new MethodCallExpressionNode(
                        new MethodCallExpressionNode(ConstantExpressionNode.Get(1), "Select",
                            new IExpressionNode[]
                            {
                                new LambdaExpressionNode(
                                    new ConditionExpressionNode(new BinaryExpressionNode(BinaryTokenType.Equality, new ParameterExpressionNode("s"), ConstantExpressionNode.Get(null, typeof(object))),
                                        new BinaryExpressionNode(BinaryTokenType.Addition, ConstantExpressionNode.Get(10, typeof(int)), ConstantExpressionNode.Get(4, typeof(int))),
                                        new BinaryExpressionNode(BinaryTokenType.Addition, ConstantExpressionNode.Get(3, typeof(int)), ConstantExpressionNode.Get(10, typeof(int)))),
                                    new IParameterExpressionNode[] {new ParameterExpressionNode("s")})
                            }, new string[0]), "FirstOrDefault", new IExpressionNode[0], new string[0]), ConstantExpressionNode.Get(0, typeof(int))), ConstantExpressionNode.Get(false, typeof(bool)),
                new BinaryExpressionNode(BinaryTokenType.ConditionalOr, ConstantExpressionNode.Get(true, typeof(bool)), ConstantExpressionNode.Get(true, typeof(bool))));
            var source = "1.Select(s => s == null ? 10 + 4 : 3 + 10).FirstOrDefault() == 0 ? false : true || true";
            ValidateExpression(source, expectedResult, count, parameterCount);
        }

        [Theory]
        [InlineData(1, 0)]
        [InlineData(1, 1)]
        [InlineData(1, 5)]
        [InlineData(5, 0)]
        [InlineData(5, 1)]
        [InlineData(5, 5)]
        public void ParseShouldParseExpression4(int count, int parameterCount)
        {
            var p1 = new ParameterExpressionNode("x");
            var p2 = new ParameterExpressionNode("s1");
            var p3 = new ParameterExpressionNode("s2");
            var expectedResult = new MethodCallExpressionNode(
                new MethodCallExpressionNode(ConstantExpressionNode.Get(1), "Where",
                    new IExpressionNode[]
                    {
                        new LambdaExpressionNode(new BinaryExpressionNode(BinaryTokenType.Equality, p1, ConstantExpressionNode.Get("test", typeof(string))),
                            new IParameterExpressionNode[] {p1})
                    }, new string[0]), "Aggregate",
                new IExpressionNode[]
                {
                    ConstantExpressionNode.Get("seed", typeof(string)),
                    new LambdaExpressionNode(new BinaryExpressionNode(BinaryTokenType.Addition, p2, p3),
                        new IParameterExpressionNode[] {p2, p3}),
                    new LambdaExpressionNode(new MemberExpressionNode(p2, "Length"), new IParameterExpressionNode[] {p2})
                }, new string[0]);
            var source = "1.Where(x => x == \"test\").Aggregate(\"seed\", (s1, s2) => s1 + s2, s1 => s1.Length)";
            ValidateExpression(source, expectedResult, count, parameterCount);
        }

        [Theory]
        [InlineData(1, 0)]
        [InlineData(1, 1)]
        [InlineData(1, 5)]
        [InlineData(5, 0)]
        [InlineData(5, 1)]
        [InlineData(5, 5)]
        public void ParseShouldParseExpression5(int count, int parameterCount)
        {
            var expectedResult = new MethodCallExpressionNode(
                new MethodCallExpressionNode(ConstantExpressionNode.Get(1), "Where",
                    new IExpressionNode[]
                    {
                        new LambdaExpressionNode(new BinaryExpressionNode(BinaryTokenType.Equality, new ParameterExpressionNode("x"), ConstantExpressionNode.Get("test", typeof(string))),
                            new IParameterExpressionNode[] {new ParameterExpressionNode("x")})
                    }, new[] { "string" }), "Aggregate",
                new IExpressionNode[]
                {
                    ConstantExpressionNode.Get("seed", typeof(string)),
                    new LambdaExpressionNode(new BinaryExpressionNode(BinaryTokenType.Addition, new ParameterExpressionNode("s1"), new ParameterExpressionNode("s2")),
                        new IParameterExpressionNode[] {new ParameterExpressionNode("s1"), new ParameterExpressionNode("s2")}),
                    new LambdaExpressionNode(new MemberExpressionNode(new ParameterExpressionNode("s1"), "Length"), new IParameterExpressionNode[] {new ParameterExpressionNode("s1")})
                }, new[] { "string", "string", "int" });
            var source = "1.Where<string>(x => x == \"test\").Aggregate<string, string, int>(\"seed\", (s1, s2) => s1 + s2, s1 => s1.Length)";
            ValidateExpression(source, expectedResult, count, parameterCount);
        }

        [Theory]
        [InlineData(1, 0)]
        [InlineData(1, 1)]
        [InlineData(1, 5)]
        [InlineData(5, 0)]
        [InlineData(5, 1)]
        [InlineData(5, 5)]
        public void ParseShouldParseExpression6(int count, int parameterCount)
        {
            var expectedResult = new BinaryExpressionNode(BinaryTokenType.LessThan,
                new BinaryExpressionNode(BinaryTokenType.RightShift,
                    new BinaryExpressionNode(BinaryTokenType.LeftShift,
                        new BinaryExpressionNode(BinaryTokenType.Addition, new UnaryExpressionNode(UnaryTokenType.Minus, ConstantExpressionNode.Get(1, typeof(int))),
                            new BinaryExpressionNode(BinaryTokenType.Remainder,
                                new BinaryExpressionNode(BinaryTokenType.Division,
                                    new BinaryExpressionNode(BinaryTokenType.Multiplication, ConstantExpressionNode.Get(2, typeof(int)),
                                        new UnaryExpressionNode(UnaryTokenType.BitwiseNegation, ConstantExpressionNode.Get(1, typeof(int)))), ConstantExpressionNode.Get(8, typeof(int))),
                                ConstantExpressionNode.Get(1, typeof(int)))), ConstantExpressionNode.Get(4, typeof(int))), ConstantExpressionNode.Get(5, typeof(int))),
                ConstantExpressionNode.Get(10, typeof(int)));
            var source = "-1 + 2 * ~1 / 8 % 1 << 4 >> 5 < 10";
            ValidateExpression(source, expectedResult, count, parameterCount);
        }

        [Theory]
        [InlineData(1, 0)]
        [InlineData(1, 1)]
        [InlineData(1, 5)]
        [InlineData(5, 0)]
        [InlineData(5, 1)]
        [InlineData(5, 5)]
        public void ParseShouldParseExpression7(int count, int parameterCount)
        {
            var expectedResult = new BinaryExpressionNode(BinaryTokenType.Addition,
                new MethodCallExpressionNode(new NullConditionalMemberExpressionNode(ConstantExpressionNode.Get(1)), "Where",
                    new IExpressionNode[]
                    {
                        new LambdaExpressionNode(
                            new BinaryExpressionNode(BinaryTokenType.Equality,
                                new IndexExpressionNode(new NullConditionalMemberExpressionNode(new ParameterExpressionNode("x")), new IExpressionNode[] {ConstantExpressionNode.Get(0, typeof(Int32))}),
                                new IndexExpressionNode(ConstantExpressionNode.Get("n", typeof(String)), new IExpressionNode[] {ConstantExpressionNode.Get(0, typeof(Int32))})),
                            new IParameterExpressionNode[] {new ParameterExpressionNode("x")})
                    }, new string[0]),
                new MethodCallExpressionNode(new IndexExpressionNode(new NullConditionalMemberExpressionNode(ConstantExpressionNode.Get(2)), new IExpressionNode[] { ConstantExpressionNode.Get(1, typeof(Int32)) }),
                    "ToString", new IExpressionNode[0], new string[0]));

            var source = "1?.Where(x => x?[0] == 'n'[0]) + 2?[1].ToString()";
            ValidateExpression(source, expectedResult, count, parameterCount);
        }

        protected override ExpressionParser GetComponentOwner(IComponentCollectionProvider? collectionProvider = null)
        {
            return new ExpressionParser(collectionProvider);
        }

        private static void ValidateExpression(string source, IExpressionNode expectedResult, int count, int parameterCount)
        {
            var targetName = "Test";
            var parameterName = "P";
            var builder = new StringBuilder();
            for (var i = 0; i < count; i++)
            {
                builder.Append($"{targetName}{i} {source}");
                for (var j = 0; j < parameterCount; j++)
                    builder.Append($",{parameterName}{j}={source}");
                builder.Append("; ");
            }

            var exp = builder.ToString();
            var list = GetInitializedExpressionParser().Parse(exp).ToArray();
            list.Length.ShouldEqual(count);
            for (var i = 0; i < count; i++)
            {
                var result = list[i];
                result.Target.ShouldEqual(new MemberExpressionNode(null, targetName + i));
                result.Source.ShouldEqual(expectedResult);
                var array = result.Parameters.ToArray();
                for (var j = 0; j < parameterCount; j++)
                {
                    var binaryExpressionNode = (BinaryExpressionNode)array[j];
                    binaryExpressionNode.Token.ShouldEqual(BinaryTokenType.Assignment);
                    binaryExpressionNode.Left.ShouldEqual(new MemberExpressionNode(null, parameterName + j));
                    binaryExpressionNode.Right.ShouldEqual(expectedResult);
                }
            }
        }



        private static IExpressionParser GetInitializedExpressionParser()
        {
            var expressionParser = new ExpressionParser();
            expressionParser.AddComponent(new AssignmentTokenParser());
            expressionParser.AddComponent(new BinaryTokenParser());
            expressionParser.AddComponent(new ConditionTokenParser());
            expressionParser.AddComponent(new ConstantTokenParser());
            expressionParser.AddComponent(new DigitTokenParser());
            expressionParser.AddComponent(new IndexerTokenParser());
            expressionParser.AddComponent(new LambdaTokenParser());
            expressionParser.AddComponent(new MemberTokenParser());
            expressionParser.AddComponent(new MethodCallTokenParser());
            expressionParser.AddComponent(new NullConditionalMemberTokenParser());
            expressionParser.AddComponent(new ParenTokenParser());
            expressionParser.AddComponent(new StringTokenParser());
            expressionParser.AddComponent(new ExpressionParserComponent());
            expressionParser.AddComponent(new UnaryTokenParser());
            return expressionParser;
        }

        #endregion
    }
}