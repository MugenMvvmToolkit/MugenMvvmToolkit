using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Metadata;
using MugenMvvm.Binding.Parsing;
using MugenMvvm.Binding.Parsing.Components;
using MugenMvvm.Binding.Parsing.Components.Converters;
using MugenMvvm.Binding.Parsing.Components.Parsers;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTest.Binding.Parsing.Internal;
using MugenMvvm.UnitTest.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Parsing
{
    public class ExpressionParserTest : ComponentOwnerTestBase<ExpressionParser>
    {
        #region Constructors

        public ExpressionParserTest()
        {
            DefaultMetadata = new MetadataContext();
            DefaultMetadata.Set(ParsingMetadata.ParsingErrors, new List<string>());
        }

        #endregion

        #region Properties

        protected new IMetadataContext DefaultMetadata { get; set; }

        public object? Test { get; set; }

        public string? StringProperty { get; set; }

        #endregion

        #region Methods

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

            parser.TryParse(request, DefaultMetadata).ShouldEqual(result);
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
                new BinaryExpressionNode(BinaryTokenType.Addition,
                    new BinaryExpressionNode(BinaryTokenType.Addition,
                        new MethodCallExpressionNode(
                            new MethodCallExpressionNode(new NullConditionalMemberExpressionNode(new MemberExpressionNode(null, "value1")), "Where",
                                new IExpressionNode[]
                                {
                                    new LambdaExpressionNode(
                                        new BinaryExpressionNode(BinaryTokenType.Equality,
                                            new IndexExpressionNode(new NullConditionalMemberExpressionNode(new ParameterExpressionNode("x")), new IExpressionNode[] {ConstantExpressionNode.Get(0, typeof(int))}),
                                            new IndexExpressionNode(ConstantExpressionNode.Get("n", typeof(string)), new IExpressionNode[] {ConstantExpressionNode.Get(0, typeof(int))})),
                                        new IParameterExpressionNode[] {new ParameterExpressionNode("x")})
                                }, new string[0]), "FirstOrDefault", new IExpressionNode[0], new string[0]),
                        new MethodCallExpressionNode(
                            new IndexExpressionNode(new NullConditionalMemberExpressionNode(new MemberExpressionNode(null, "value2")), new IExpressionNode[] { ConstantExpressionNode.Get(1, typeof(int)) }), "ToString",
                            new IExpressionNode[0], new string[0])),
                    new MemberExpressionNode(
                        new NullConditionalMemberExpressionNode(new MethodCallExpressionNode(
                            new IndexExpressionNode(new NullConditionalMemberExpressionNode(new MemberExpressionNode(null, "value3")), new IExpressionNode[] { ConstantExpressionNode.Get(1, typeof(int)) }), "ToString",
                            new IExpressionNode[0], new string[0])), "Length")),
                new MethodCallExpressionNode(
                    new NullConditionalMemberExpressionNode(new ConditionExpressionNode(
                        new BinaryExpressionNode(BinaryTokenType.Equality, new MemberExpressionNode(null, "value2"), ConstantExpressionNode.Get("xx", typeof(string))), ConstantExpressionNode.Get(null, typeof(object)),
                        new MemberExpressionNode(null, "value2"))), "ToString", new IExpressionNode[0], new string[0]));

            var source = "value1?.Where(x => x?[0] == 'n'[0]).FirstOrDefault() + value2?[1].ToString() + value3?[1].ToString()?.Length + (value2 == 'xx' ? null : value2)?.ToString()";
            ValidateExpression(source, expectedResult, count, parameterCount);
        }

        [Fact]
        public void ParserShouldParseActionExpression()
        {
            var result = GetInitializedExpressionParser().TryParse("@1+2; @1+2", DefaultMetadata).AsList();
            result.Count.ShouldEqual(2);
            for (var i = 0; i < result.Count; i++)
            {
                result[i].Target.ShouldEqual(UnaryExpressionNode.ActionMacros);
                result[i].Source.ShouldEqual(new BinaryExpressionNode(BinaryTokenType.Addition, ConstantExpressionNode.Get(1), ConstantExpressionNode.Get(2)));
                result[i].Parameters.IsNullOrEmpty().ShouldBeTrue();
            }
        }

        [Theory]
        [InlineData(1, 0)]
        [InlineData(1, 1)]
        [InlineData(1, 5)]
        [InlineData(5, 0)]
        [InlineData(5, 1)]
        [InlineData(5, 5)]
        public void ParseShouldConvertExpression1(int count, int parameterCount)
        {
            var expectedResult = new ConditionExpressionNode(
                new BinaryExpressionNode(BinaryTokenType.Equality,
                    new MethodCallExpressionNode(new MemberExpressionNode(null, nameof(StringProperty)), "IndexOf", new IExpressionNode[] { ConstantExpressionNode.Get("1", typeof(string)) }, new string[0]),
                    ConstantExpressionNode.Get(0, typeof(int))),
                new MethodCallExpressionNode(ConstantExpressionNode.Get(typeof(string)), "Format",
                    new IExpressionNode[] { ConstantExpressionNode.Get("{0} - {1}", typeof(string)), ConstantExpressionNode.Get(1, typeof(int)), ConstantExpressionNode.Get(2, typeof(int)) }, new string[0]),
                new ConditionExpressionNode(
                    new BinaryExpressionNode(BinaryTokenType.GreaterThanOrEqual, new MethodCallExpressionNode(ConstantExpressionNode.Get(2, typeof(int)), "GetHashCode", new IExpressionNode[0], new string[0]),
                        ConstantExpressionNode.Get(10, typeof(int))), ConstantExpressionNode.Get("test", typeof(string)),
                    new BinaryExpressionNode(BinaryTokenType.NullCoalescing, new MethodCallExpressionNode(ConstantExpressionNode.Get("v", typeof(string)), "ToString", new IExpressionNode[0], new string[0]),
                        ConstantExpressionNode.Get("value", typeof(string)))));

            // ReSharper disable once RedundantToStringCall
            ValidateExpression<ExpressionParserTest, ExpressionParserTest>(nameof(Test), o => o.Test,
                test => test.StringProperty!.IndexOf("1") == 0 ? string.Format("{0} - {1}", 1, 2) : 2.GetHashCode() >= 10 ? "test" : "v".ToString() ?? "value", expectedResult, count, parameterCount);
        }

        [Theory]
        [InlineData(1, 0)]
        [InlineData(1, 1)]
        [InlineData(1, 5)]
        [InlineData(5, 0)]
        [InlineData(5, 1)]
        [InlineData(5, 5)]
        public void ParseShouldConvertExpression2(int count, int parameterCount)
        {
            var expectedResult = new BinaryExpressionNode(BinaryTokenType.GreaterThanOrEqual,
                new BinaryExpressionNode(BinaryTokenType.Addition,
                    new BinaryExpressionNode(BinaryTokenType.Multiplication,
                        new BinaryExpressionNode(BinaryTokenType.Subtraction, new MethodCallExpressionNode(ConstantExpressionNode.Get(1, typeof(int)), "GetHashCode", new IExpressionNode[0], new string[0]),
                            new BinaryExpressionNode(BinaryTokenType.Division, new MethodCallExpressionNode(ConstantExpressionNode.Get(2, typeof(int)), "GetHashCode", new IExpressionNode[0], new string[0]),
                                new MethodCallExpressionNode(ConstantExpressionNode.Get(3, typeof(int)), "GetHashCode", new IExpressionNode[0], new string[0]))),
                        new MethodCallExpressionNode(ConstantExpressionNode.Get(1, typeof(int)), "GetHashCode", new IExpressionNode[0], new string[0])),
                    new BinaryExpressionNode(BinaryTokenType.Subtraction,
                        new BinaryExpressionNode(BinaryTokenType.Multiplication, new MethodCallExpressionNode(ConstantExpressionNode.Get(1000, typeof(int)), "GetHashCode", new IExpressionNode[0], new string[0]),
                            new MethodCallExpressionNode(ConstantExpressionNode.Get(1, typeof(int)), "GetHashCode", new IExpressionNode[0], new string[0])),
                        new MethodCallExpressionNode(ConstantExpressionNode.Get(1, typeof(int)), "GetHashCode", new IExpressionNode[0], new string[0]))),
                new MethodCallExpressionNode(ConstantExpressionNode.Get(100, typeof(int)), "GetHashCode", new IExpressionNode[0], new string[0]));
            ValidateExpression<ExpressionParserTest, ExpressionParserTest>(nameof(StringProperty), test => test.StringProperty,
                test => (1.GetHashCode() - 2.GetHashCode() / 3.GetHashCode()) * 1.GetHashCode() + (1000.GetHashCode() * 1.GetHashCode() - 1.GetHashCode()) >= 100.GetHashCode(), expectedResult, count, parameterCount);
        }

        [Theory]
        [InlineData(1, 0)]
        [InlineData(1, 1)]
        [InlineData(1, 5)]
        [InlineData(5, 0)]
        [InlineData(5, 1)]
        [InlineData(5, 5)]
        public void ParseShouldConvertExpression3(int count, int parameterCount)
        {
            var expectedResult = new MethodCallExpressionNode(
                new MethodCallExpressionNode(new MemberExpressionNode(null, nameof(Test)), "Where",
                    new IExpressionNode[]
                    {
                        new LambdaExpressionNode(new BinaryExpressionNode(BinaryTokenType.Equality, new ParameterExpressionNode("x"), ConstantExpressionNode.Get("test", typeof(string))),
                            new IParameterExpressionNode[] {new ParameterExpressionNode("x")})
                    }, new[] { typeof(string).AssemblyQualifiedName }), "Aggregate",
                new IExpressionNode[]
                {
                    new MemberExpressionNode(null, nameof(StringProperty)),
                    new LambdaExpressionNode(new BinaryExpressionNode(BinaryTokenType.Addition, new ParameterExpressionNode("s1"), new ParameterExpressionNode("s2")),
                        new IParameterExpressionNode[] {new ParameterExpressionNode("s1"), new ParameterExpressionNode("s2")}),
                    new LambdaExpressionNode(new MemberExpressionNode(new ParameterExpressionNode("s1"), "Length"), new IParameterExpressionNode[] {new ParameterExpressionNode("s1")})
                }, new[] { typeof(string).AssemblyQualifiedName, typeof(string).AssemblyQualifiedName, typeof(int).AssemblyQualifiedName });
            ValidateExpression<ExpressionParserTest, ExpressionParserTest>(nameof(Test), test => test.Test,
                test => ((string[])test.Test!).Where(x => x == "test").Aggregate(test.StringProperty, (s1, s2) => s1 + s2, s1 => s1!.Length), expectedResult, count, parameterCount);
        }

        [Fact]
        public void ParserShouldConvertExpressions1()
        {
            var parser = GetInitializedExpressionParser();
            var item = parser.TryParse(new ExpressionConverterRequest(nameof(Test), null, default), DefaultMetadata).Item;
            item.Target.ShouldEqual(new MemberExpressionNode(null, nameof(Test)));
            item.Source.ShouldEqual(MemberExpressionNode.Empty);
            item.Parameters.IsNullOrEmpty().ShouldBeTrue();

            item = parser.TryParse(new ExpressionConverterRequest(nameof(Test), nameof(StringProperty), default), DefaultMetadata).Item;
            item.Target.ShouldEqual(new MemberExpressionNode(null, nameof(Test)));
            item.Source.ShouldEqual(new MemberExpressionNode(null, nameof(StringProperty)));
            item.Parameters.IsNullOrEmpty().ShouldBeTrue();

            item = parser.TryParse(new ExpressionConverterRequest(MemberExpressionNode.Empty, nameof(StringProperty), default), DefaultMetadata).Item;
            item.Target.ShouldEqual(MemberExpressionNode.Empty);
            item.Source.ShouldEqual(new MemberExpressionNode(null, nameof(StringProperty)));
            item.Parameters.IsNullOrEmpty().ShouldBeTrue();


            item = parser.TryParse(new ExpressionConverterRequest(MemberExpressionNode.Empty, MemberExpressionNode.Empty,
                new KeyValuePair<string?, object>(nameof(StringProperty), nameof(Test))), DefaultMetadata).Item;
            item.Target.ShouldEqual(MemberExpressionNode.Empty);
            item.Source.ShouldEqual(MemberExpressionNode.Empty);
            item.Parameters.Item.ShouldEqual(new BinaryExpressionNode(BinaryTokenType.Assignment, new MemberExpressionNode(null, nameof(StringProperty)), MemberExpressionNode.Get(null, nameof(Test))));

            item = parser.TryParse(new ExpressionConverterRequest(MemberExpressionNode.Empty, MemberExpressionNode.Empty,
                new KeyValuePair<string?, object>(nameof(StringProperty), MemberExpressionNode.Empty)), DefaultMetadata).Item;
            item.Target.ShouldEqual(MemberExpressionNode.Empty);
            item.Source.ShouldEqual(MemberExpressionNode.Empty);
            item.Parameters.Item.ShouldEqual(new BinaryExpressionNode(BinaryTokenType.Assignment, new MemberExpressionNode(null, nameof(StringProperty)), MemberExpressionNode.Empty));

            item = parser.TryParse(new ExpressionConverterRequest(MemberExpressionNode.Empty, MemberExpressionNode.Empty,
                new KeyValuePair<string?, object>(nameof(StringProperty), Expression.Constant(nameof(Test)))), DefaultMetadata).Item;
            item.Target.ShouldEqual(MemberExpressionNode.Empty);
            item.Source.ShouldEqual(MemberExpressionNode.Empty);
            item.Parameters.Item.ShouldEqual(new BinaryExpressionNode(BinaryTokenType.Assignment, new MemberExpressionNode(null, nameof(StringProperty)), ConstantExpressionNode.Get(nameof(Test))));


            item = parser.TryParse(new ExpressionConverterRequest(MemberExpressionNode.Empty, MemberExpressionNode.Empty, new KeyValuePair<string?, object>(null, nameof(Test))), DefaultMetadata).Item;
            item.Target.ShouldEqual(MemberExpressionNode.Empty);
            item.Source.ShouldEqual(MemberExpressionNode.Empty);
            item.Parameters.Item.ShouldEqual(new MemberExpressionNode(null, nameof(Test)));

            item = parser.TryParse(new ExpressionConverterRequest(MemberExpressionNode.Empty, MemberExpressionNode.Empty,
                new KeyValuePair<string?, object>(null, MemberExpressionNode.Empty)), DefaultMetadata).Item;
            item.Target.ShouldEqual(MemberExpressionNode.Empty);
            item.Source.ShouldEqual(MemberExpressionNode.Empty);
            item.Parameters.Item.ShouldEqual(MemberExpressionNode.Empty);

            item = parser.TryParse(new ExpressionConverterRequest(MemberExpressionNode.Empty, MemberExpressionNode.Empty,
                new KeyValuePair<string?, object>(null, Expression.Constant(nameof(Test)))), DefaultMetadata).Item;
            item.Target.ShouldEqual(MemberExpressionNode.Empty);
            item.Source.ShouldEqual(MemberExpressionNode.Empty);
            item.Parameters.Item.ShouldEqual(ConstantExpressionNode.Get(nameof(Test)));
        }

        [Fact]
        public void ParserShouldConvertExpressions2()
        {
            var parser = GetInitializedExpressionParser();
            var selfExpression = GetExpression(this, arg => arg);
            var testExpression = GetExpression(this, arg => arg.Test);
            var stPropertyExpression = GetExpression(this, arg => arg.StringProperty);
            var item = parser.TryParse(new ExpressionConverterRequest(testExpression, null, default), DefaultMetadata).Item;
            item.Target.ShouldEqual(new MemberExpressionNode(null, nameof(Test)));
            item.Source.ShouldEqual(MemberExpressionNode.Empty);
            item.Parameters.IsNullOrEmpty().ShouldBeTrue();

            item = parser.TryParse(new ExpressionConverterRequest(nameof(Test), stPropertyExpression, default), DefaultMetadata).Item;
            item.Target.ShouldEqual(new MemberExpressionNode(null, nameof(Test)));
            item.Source.ShouldEqual(new MemberExpressionNode(null, nameof(StringProperty)));
            item.Parameters.IsNullOrEmpty().ShouldBeTrue();

            item = parser.TryParse(new ExpressionConverterRequest(testExpression, stPropertyExpression,
                new KeyValuePair<string?, object>(nameof(StringProperty), stPropertyExpression)), DefaultMetadata).Item;
            item.Target.ShouldEqual(new MemberExpressionNode(null, nameof(Test)));
            item.Source.ShouldEqual(new MemberExpressionNode(null, nameof(StringProperty)));
            item.Parameters.Item.ShouldEqual(new BinaryExpressionNode(BinaryTokenType.Assignment, new MemberExpressionNode(null, nameof(StringProperty)), new MemberExpressionNode(null, nameof(StringProperty))));

            item = parser.TryParse(new ExpressionConverterRequest(selfExpression, selfExpression, new KeyValuePair<string?, object>(nameof(StringProperty), selfExpression)), DefaultMetadata).Item;
            item.Target.ShouldEqual(ConstantExpressionNode.Null);
            item.Source.ShouldEqual(ConstantExpressionNode.Null);
            item.Parameters.Item.ShouldEqual(new BinaryExpressionNode(BinaryTokenType.Assignment, new MemberExpressionNode(null, nameof(StringProperty)), ConstantExpressionNode.Null));
        }

        [Fact]
        public void ParserShouldConvertExpressions3()
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

            var parser = GetInitializedExpressionParser();

            var item = parser.TryParse(new ExpressionConverterRequest(source, source, new KeyValuePair<string?, object>(null, source)), DefaultMetadata).Item;
            item.Target.ShouldEqual(expectedResult);
            item.Source.ShouldEqual(expectedResult);
            item.Parameters.Item.ShouldEqual(expectedResult);
        }

        protected override ExpressionParser GetComponentOwner(IComponentCollectionProvider? collectionProvider = null)
        {
            return new ExpressionParser(collectionProvider);
        }

        private void ValidateExpression<TTarget, TSource>(string targetName, Expression<Func<TTarget, object?>> target, Expression<Func<TSource, object?>> source, IExpressionNode expectedResult, int count,
            int parameterCount)
        {
            var parameterName = "P";
            var requests = new List<ExpressionConverterRequest>();
            for (var i = 0; i < count; i++)
            {
                var parameters = new List<KeyValuePair<string?, object>>();
                for (var j = 0; j < parameterCount; j++)
                    parameters.Add(new KeyValuePair<string?, object>(parameterName + j, source));
                requests.Add(new ExpressionConverterRequest(target, source, parameters));
            }

            var parser = GetInitializedExpressionParser();
            var list = (requests.Count == 1 ? parser.TryParse(requests[0], DefaultMetadata) : parser.TryParse(requests, DefaultMetadata)).AsList();
            list.Count.ShouldEqual(count);
            for (var i = 0; i < count; i++)
            {
                var result = list[i];
                result.Target.ShouldEqual(new MemberExpressionNode(null, targetName));
                result.Source.ShouldEqual(expectedResult);
                var array = result.Parameters.AsList();
                for (var j = 0; j < parameterCount; j++)
                {
                    var binaryExpressionNode = (BinaryExpressionNode)array[j];
                    binaryExpressionNode.Token.ShouldEqual(BinaryTokenType.Assignment);
                    binaryExpressionNode.Left.ShouldEqual(new MemberExpressionNode(null, parameterName + j));
                    binaryExpressionNode.Right.ShouldEqual(expectedResult);
                }
            }
        }

        private void ValidateExpression(string source, IExpressionNode expectedResult, int count, int parameterCount)
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
            var list = GetInitializedExpressionParser().TryParse(exp, DefaultMetadata).AsList();
            list.Count.ShouldEqual(count);
            for (var i = 0; i < count; i++)
            {
                var result = list[i];
                result.Target.ShouldEqual(new MemberExpressionNode(null, targetName + i));
                result.Source.ShouldEqual(expectedResult);
                var array = result.Parameters.AsList();
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
            //parsers
            expressionParser.AddComponent(new ExpressionParserComponent());
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
            expressionParser.AddComponent(new UnaryTokenParser());

            //converters
            expressionParser.AddComponent(new ExpressionConverterComponent());
            expressionParser.AddComponent(new BinaryExpressionConverter());
            expressionParser.AddComponent(new ConditionExpressionConverter());
            expressionParser.AddComponent(new ConstantExpressionConverter());
            expressionParser.AddComponent(new DefaultExpressionConverter());
            expressionParser.AddComponent(new IndexerExpressionConverter());
            expressionParser.AddComponent(new LambdaExpressionConverter());
            expressionParser.AddComponent(new MemberExpressionConverter());
            expressionParser.AddComponent(new MethodCallExpressionConverter());
            expressionParser.AddComponent(new NewArrayExpressionConverter());
            expressionParser.AddComponent(new UnaryExpressionConverter());

            return expressionParser;
        }

        private static Expression<Func<T, TResult>> GetExpression<T, TResult>([AllowNull]T t, Expression<Func<T, TResult>> expression)
        {
            return expression;
        }

        #endregion
    }
}