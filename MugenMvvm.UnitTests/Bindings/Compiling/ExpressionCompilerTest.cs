using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Bindings.Compiling;
using MugenMvvm.Bindings.Compiling.Components;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Members.Components;
using MugenMvvm.Bindings.Metadata;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTests.Bindings.Compiling.Internal;
using MugenMvvm.UnitTests.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Compiling
{
    public class ExpressionCompilerTest : ComponentOwnerTestBase<ExpressionCompiler>
    {
        #region Constructors

        public ExpressionCompilerTest()
        {
            DefaultMetadata = new MetadataContext();
            DefaultMetadata.Set(CompilingMetadata.CompilingErrors, new List<string>());
        }

        #endregion

        #region Properties

        protected new IMetadataContext DefaultMetadata { get; set; }

        #endregion

        #region Methods

        [Fact]
        public void CompileShouldThrowNoComponents()
        {
            var compiler = GetComponentOwner();
            ShouldThrow<InvalidOperationException>(() => compiler.Compile(ConstantExpressionNode.False));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void CompileShouldBeHandledByComponents(int componentCount)
        {
            var compiler = GetComponentOwner();
            var expressionNode = ConstantExpressionNode.EmptyString;
            var compiledExpression = new TestCompiledExpression();
            var count = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var component = new TestExpressionCompilerComponent(compiler)
                {
                    TryCompile = (node, metadata) =>
                    {
                        ++count;
                        node.ShouldEqual(expressionNode);
                        metadata.ShouldEqual(DefaultMetadata);
                        return compiledExpression;
                    }
                };
                compiler.AddComponent(component);
            }

            compiler.Compile(expressionNode, DefaultMetadata).ShouldEqual(compiledExpression);
            count.ShouldEqual(1);
        }

        [Theory]
        [InlineData("1", 1, 2)]
        [InlineData("0", 1, 2)]
        [InlineData("0", 1, 21)]
        public void CompileShouldCompileComplexExpression1(string value1, int value2, int value3)
        {
            var result = value1.IndexOf("1") == 0 ? $"{value2} - {value3}" : value3 >= 10 ? "test" : null ?? "value";
            var node = new ConditionExpressionNode(
                new BinaryExpressionNode(BinaryTokenType.Equality,
                    new MethodCallExpressionNode(ConstantExpressionNode.Get(value1), "IndexOf", new IExpressionNode[] {ConstantExpressionNode.Get("1", typeof(string))}, new string[0]),
                    ConstantExpressionNode.Get(0, typeof(int))),
                new MethodCallExpressionNode(ConstantExpressionNode.Get(typeof(string), typeof(Type)), nameof(string.Format),
                    new IExpressionNode[] {ConstantExpressionNode.Get("{0} - {1}", typeof(string)), ConstantExpressionNode.Get(value2), ConstantExpressionNode.Get(value3)}, new string[0]),
                new ConditionExpressionNode(new BinaryExpressionNode(BinaryTokenType.GreaterThanOrEqual, ConstantExpressionNode.Get(value3), ConstantExpressionNode.Get(10, typeof(int))),
                    ConstantExpressionNode.Get("test", typeof(string)),
                    new BinaryExpressionNode(BinaryTokenType.NullCoalescing, ConstantExpressionNode.Get(null, typeof(object)), ConstantExpressionNode.Get("value", typeof(string)))));
            var compiler = GetInitializedCompiler();
            compiler.Compile(node).Invoke(default, DefaultMetadata).ShouldEqual(result);
        }

        [Theory]
        [InlineData(1, 1, 2)]
        [InlineData(0, 1, 2)]
        [InlineData(21000, 1, 21)]
        public void CompileShouldCompileComplexExpression2(int value1, int value2, int value3)
        {
            var result = (value1 - value2 / value3) * value1 + (1000 * value1 - 1) >= 100;
            var node = new BinaryExpressionNode(BinaryTokenType.GreaterThanOrEqual,
                new BinaryExpressionNode(BinaryTokenType.Addition,
                    new BinaryExpressionNode(BinaryTokenType.Multiplication,
                        new BinaryExpressionNode(BinaryTokenType.Subtraction, ConstantExpressionNode.Get(value1),
                            new BinaryExpressionNode(BinaryTokenType.Division, ConstantExpressionNode.Get(value2), ConstantExpressionNode.Get(value3))), ConstantExpressionNode.Get(value1)),
                    new BinaryExpressionNode(BinaryTokenType.Subtraction, new BinaryExpressionNode(BinaryTokenType.Multiplication, ConstantExpressionNode.Get(1000, typeof(int)), ConstantExpressionNode.Get(value1)),
                        ConstantExpressionNode.Get(1, typeof(int)))), ConstantExpressionNode.Get(100, typeof(int)));
            var compiler = GetInitializedCompiler();
            compiler.Compile(node).Invoke(default, DefaultMetadata).ShouldEqual(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("test")]
        public void CompileShouldCompileComplexExpression3(string value)
        {
            var value1 = new[] {value};
            var result = value1.Select(s => s == null ? 10 + 4 : 3 + 10).FirstOrDefault() == 0 ? false : true || true;
            var parameterExpressionNode = new ParameterExpressionNode("s");
            var node = new ConditionExpressionNode(
                new BinaryExpressionNode(BinaryTokenType.Equality,
                    new MethodCallExpressionNode(
                        new MethodCallExpressionNode(ConstantExpressionNode.Get(value1), "Select",
                            new IExpressionNode[]
                            {
                                new LambdaExpressionNode(
                                    new ConditionExpressionNode(new BinaryExpressionNode(BinaryTokenType.Equality, parameterExpressionNode, ConstantExpressionNode.Get(null, typeof(object))),
                                        new BinaryExpressionNode(BinaryTokenType.Addition, ConstantExpressionNode.Get(10, typeof(int)), ConstantExpressionNode.Get(4, typeof(int))),
                                        new BinaryExpressionNode(BinaryTokenType.Addition, ConstantExpressionNode.Get(3, typeof(int)), ConstantExpressionNode.Get(10, typeof(int)))),
                                    new IParameterExpressionNode[] {parameterExpressionNode})
                            }, new string[0]), "FirstOrDefault", new IExpressionNode[0], new string[0]), ConstantExpressionNode.Get(0, typeof(int))), ConstantExpressionNode.Get(false, typeof(bool)),
                new BinaryExpressionNode(BinaryTokenType.ConditionalOr, ConstantExpressionNode.Get(true, typeof(bool)), ConstantExpressionNode.Get(true, typeof(bool))));
            var compiler = GetInitializedCompiler();
            compiler.Compile(node).Invoke(default, DefaultMetadata).ShouldEqual(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("test")]
        public void CompileShouldCompileComplexExpression4(string value)
        {
            var value1 = new[] {value};
            var result = value1.Where(x => x == "test").Aggregate("seed", (s1, s2) => s1 + s2, s1 => s1.Length);
            var p1 = new ParameterExpressionNode("x");
            var p2 = new ParameterExpressionNode("s1");
            var p3 = new ParameterExpressionNode("s2");
            var node = new MethodCallExpressionNode(
                new MethodCallExpressionNode(ConstantExpressionNode.Get(value1), "Where",
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
            var compiler = GetInitializedCompiler();
            compiler.Compile(node).Invoke(default, DefaultMetadata).ShouldEqual(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("test")]
        public void CompileShouldCompileComplexExpression5(string value)
        {
            var value1 = new[] {value};
            var result = value1.Where(x => x == "test").Aggregate("seed", (s1, s2) => s1 + s2, s1 => s1.Length);
            var p1 = new ParameterExpressionNode("x");
            var p2 = new ParameterExpressionNode("s1");
            var p3 = new ParameterExpressionNode("s2");
            var node = new MethodCallExpressionNode(
                new MethodCallExpressionNode(ConstantExpressionNode.Get(value1), "Where",
                    new IExpressionNode[]
                    {
                        new LambdaExpressionNode(new BinaryExpressionNode(BinaryTokenType.Equality, p1, ConstantExpressionNode.Get("test", typeof(string))),
                            new IParameterExpressionNode[] {p1})
                    }, new[] {"string"}), "Aggregate",
                new IExpressionNode[]
                {
                    ConstantExpressionNode.Get("seed", typeof(string)),
                    new LambdaExpressionNode(new BinaryExpressionNode(BinaryTokenType.Addition, p2, p3),
                        new IParameterExpressionNode[] {p2, p3}),
                    new LambdaExpressionNode(new MemberExpressionNode(p2, "Length"), new IParameterExpressionNode[] {p2})
                }, new[] {"string", "string", "int"});
            var compiler = GetInitializedCompiler();
            compiler.Compile(node).Invoke(default, DefaultMetadata).ShouldEqual(result);
        }

        [Fact]
        public void CompileShouldCompileComplexExpression6()
        {
            var result = -1 + 2 * ~1 / 8 % 1 << 4 >> 5 < 10;
            var node = new BinaryExpressionNode(BinaryTokenType.LessThan,
                new BinaryExpressionNode(BinaryTokenType.RightShift,
                    new BinaryExpressionNode(BinaryTokenType.LeftShift,
                        new BinaryExpressionNode(BinaryTokenType.Addition, new UnaryExpressionNode(UnaryTokenType.Minus, ConstantExpressionNode.Get(1, typeof(int))),
                            new BinaryExpressionNode(BinaryTokenType.Remainder,
                                new BinaryExpressionNode(BinaryTokenType.Division,
                                    new BinaryExpressionNode(BinaryTokenType.Multiplication, ConstantExpressionNode.Get(2, typeof(int)),
                                        new UnaryExpressionNode(UnaryTokenType.BitwiseNegation, ConstantExpressionNode.Get(1, typeof(int)))), ConstantExpressionNode.Get(8, typeof(int))),
                                ConstantExpressionNode.Get(1, typeof(int)))), ConstantExpressionNode.Get(4, typeof(int))), ConstantExpressionNode.Get(5, typeof(int))),
                ConstantExpressionNode.Get(10, typeof(int)));
            var compiler = GetInitializedCompiler();
            compiler.Compile(node).Invoke(default, DefaultMetadata).ShouldEqual(result);
        }

        [Theory]
        [InlineData(null, null, null)]
        [InlineData("nn", null, null)]
        [InlineData("nn", "tt", null)]
        [InlineData("nn", "tt", "test")]
        [InlineData("ttt", "xx", "test")]
        [InlineData(null, "xx", "test")]
        public void CompileShouldCompileComplexExpression7(string? s, string? value2, string value3)
        {
            var value1 = new[] {s};
            var result = value1?.Where(x => x?[0] == "n"[0]).FirstOrDefault() + value2?[1] + value3?[1].ToString()?.Length + (value2 == "xx" ? null : value2);
            var parameterExp = new ParameterExpressionNode("x");
            var node = new BinaryExpressionNode(BinaryTokenType.Addition,
                new BinaryExpressionNode(BinaryTokenType.Addition,
                    new BinaryExpressionNode(BinaryTokenType.Addition,
                        new MethodCallExpressionNode(
                            new MethodCallExpressionNode(new NullConditionalMemberExpressionNode(ConstantExpressionNode.Get(value1)), "Where",
                                new IExpressionNode[]
                                {
                                    new LambdaExpressionNode(
                                        new BinaryExpressionNode(BinaryTokenType.Equality,
                                            new IndexExpressionNode(new NullConditionalMemberExpressionNode(parameterExp), new IExpressionNode[] {ConstantExpressionNode.Get(0, typeof(int))}),
                                            new IndexExpressionNode(ConstantExpressionNode.Get("n", typeof(string)), new IExpressionNode[] {ConstantExpressionNode.Get(0, typeof(int))})),
                                        new IParameterExpressionNode[] {parameterExp})
                                }, new string[0]), "FirstOrDefault", new IExpressionNode[0], new string[0]),
                        new MethodCallExpressionNode(
                            new IndexExpressionNode(new NullConditionalMemberExpressionNode(ConstantExpressionNode.Get(value2)), new IExpressionNode[] {ConstantExpressionNode.Get(1, typeof(int))}), "ToString",
                            new IExpressionNode[0], new string[0])),
                    new MemberExpressionNode(
                        new NullConditionalMemberExpressionNode(new MethodCallExpressionNode(
                            new IndexExpressionNode(new NullConditionalMemberExpressionNode(ConstantExpressionNode.Get(value3)), new IExpressionNode[] {ConstantExpressionNode.Get(1, typeof(int))}), "ToString",
                            new IExpressionNode[0], new string[0])), "Length")),
                new MethodCallExpressionNode(
                    new NullConditionalMemberExpressionNode(new ConditionExpressionNode(
                        new BinaryExpressionNode(BinaryTokenType.Equality, ConstantExpressionNode.Get(value2), ConstantExpressionNode.Get("xx", typeof(string))), ConstantExpressionNode.Get(null, typeof(object)),
                        ConstantExpressionNode.Get(value2))), "ToString", new IExpressionNode[0], new string[0]));
            var compiler = GetInitializedCompiler();
            compiler.Compile(node).Invoke(default, DefaultMetadata).ShouldEqual(result);
        }

        private static ExpressionCompiler GetInitializedCompiler()
        {
            var memberManager = new MemberManager();
            memberManager.AddComponent(new MemberSelector());
            memberManager.AddComponent(new NameRequestMemberManagerDecorator());
            memberManager.AddComponent(new ReflectionMemberProvider());
            memberManager.AddComponent(new ExtensionMethodMemberProvider());

            var expressionCompiler = new ExpressionCompiler();
            expressionCompiler.AddComponent(new BinaryExpressionBuilder());
            expressionCompiler.AddComponent(new ConditionExpressionBuilder());
            expressionCompiler.AddComponent(new ConstantExpressionBuilder());
            expressionCompiler.AddComponent(new LambdaExpressionBuilder());
            expressionCompiler.AddComponent(new MemberExpressionBuilder(memberManager));
            expressionCompiler.AddComponent(new MethodCallIndexerExpressionBuilder(memberManager));
            expressionCompiler.AddComponent(new NullConditionalExpressionBuilder());
            expressionCompiler.AddComponent(new UnaryExpressionBuilder());
            expressionCompiler.AddComponent(new ExpressionCompilerComponent());
            return expressionCompiler;
        }

        protected override ExpressionCompiler GetComponentOwner(IComponentCollectionManager? collectionProvider = null) => new(collectionProvider);

        #endregion
    }
}