using System;
using System.Linq;
using MugenMvvm.Binding.Compiling;
using MugenMvvm.Binding.Compiling.Components;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Members.Components;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.UnitTest.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Compiling
{
    public class ExpressionCompilerTest : ComponentOwnerTestBase<ExpressionCompiler>
    {
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
                var component = new TestExpressionCompilerComponent
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
                    new MethodCallExpressionNode(ConstantExpressionNode.Get(value1), "IndexOf", new IExpressionNode[] { ConstantExpressionNode.Get("1", typeof(String)) }, new string[0]),
                    ConstantExpressionNode.Get(0, typeof(Int32))),
                new MethodCallExpressionNode(ConstantExpressionNode.Get(typeof(String), typeof(Type)), "Format",
                    new IExpressionNode[] { ConstantExpressionNode.Get("{0} - {1}", typeof(String)), ConstantExpressionNode.Get(value2), ConstantExpressionNode.Get(value3) }, new string[0]),
                new ConditionExpressionNode(new BinaryExpressionNode(BinaryTokenType.GreaterThanOrEqual, ConstantExpressionNode.Get(value3), ConstantExpressionNode.Get(10, typeof(Int32))),
                    ConstantExpressionNode.Get("test", typeof(String)),
                    new BinaryExpressionNode(BinaryTokenType.NullCoalescing, ConstantExpressionNode.Get(null, typeof(Object)), ConstantExpressionNode.Get("value", typeof(String)))));
            var compiler = GetInitializedCompiler();
            compiler.Compile(node).Invoke(default, DefaultMetadata).ShouldEqual(result);
        }

        [Theory]
        [InlineData(1, 1, 2)]
        [InlineData(0, 1, 2)]
        [InlineData(21000, 1, 21)]
        public void CompileShouldCompileComplexExpression2(int value1, int value2, int value3)
        {
            var result = ((value1 - (value2 / value3)) * value1) + ((1000 * value1) - 1) >= 100;
            var node = new BinaryExpressionNode(BinaryTokenType.GreaterThanOrEqual,
                new BinaryExpressionNode(BinaryTokenType.Addition,
                    new BinaryExpressionNode(BinaryTokenType.Multiplication,
                        new BinaryExpressionNode(BinaryTokenType.Subtraction, ConstantExpressionNode.Get(value1),
                            new BinaryExpressionNode(BinaryTokenType.Division, ConstantExpressionNode.Get(value2), ConstantExpressionNode.Get(value3))), ConstantExpressionNode.Get(value1)),
                    new BinaryExpressionNode(BinaryTokenType.Subtraction, new BinaryExpressionNode(BinaryTokenType.Multiplication, ConstantExpressionNode.Get(1000, typeof(Int32)), ConstantExpressionNode.Get(value1)),
                        ConstantExpressionNode.Get(1, typeof(Int32)))), ConstantExpressionNode.Get(100, typeof(Int32)));
            var compiler = GetInitializedCompiler();
            compiler.Compile(node).Invoke(default, DefaultMetadata).ShouldEqual(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("test")]
        public void CompileShouldCompileComplexExpression3(string value)
        {
            var value1 = new[] { value };
            var result = value1.Select(s => s == null ? 10 + 4 : 3 + 10).FirstOrDefault() == 0 ? false : true || true;
            var parameterExpressionNode = new ParameterExpressionNode("s");
            var node = new ConditionExpressionNode(
                new BinaryExpressionNode(BinaryTokenType.Equality,
                    new MethodCallExpressionNode(
                        new MethodCallExpressionNode(ConstantExpressionNode.Get(value1), "Select",
                            new IExpressionNode[]
                            {
                                new LambdaExpressionNode(
                                    new ConditionExpressionNode(new BinaryExpressionNode(BinaryTokenType.Equality, parameterExpressionNode, ConstantExpressionNode.Get(null, typeof(Object))),
                                        new BinaryExpressionNode(BinaryTokenType.Addition, ConstantExpressionNode.Get(10, typeof(Int32)), ConstantExpressionNode.Get(4, typeof(Int32))),
                                        new BinaryExpressionNode(BinaryTokenType.Addition, ConstantExpressionNode.Get(3, typeof(Int32)), ConstantExpressionNode.Get(10, typeof(Int32)))),
                                    new IParameterExpressionNode[] {parameterExpressionNode})
                            }, new string[0]), "FirstOrDefault", new IExpressionNode[0], new string[0]), ConstantExpressionNode.Get(0, typeof(Int32))), ConstantExpressionNode.Get(false, typeof(Boolean)),
                new BinaryExpressionNode(BinaryTokenType.ConditionalOr, ConstantExpressionNode.Get(true, typeof(Boolean)), ConstantExpressionNode.Get(true, typeof(Boolean))));
            var compiler = GetInitializedCompiler();
            compiler.Compile(node).Invoke(default, DefaultMetadata).ShouldEqual(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("test")]
        public void CompileShouldCompileComplexExpression4(string value)
        {
            var value1 = new[] { value };
            var result = value1.Where(x => x == "test").Aggregate("seed", (s1, s2) => s1 + s2, s1 => s1.Length);
            var p1 = new ParameterExpressionNode("x");
            var p2 = new ParameterExpressionNode("s1");
            var p3 = new ParameterExpressionNode("s2");
            var node = new MethodCallExpressionNode(
                new MethodCallExpressionNode(ConstantExpressionNode.Get(value1), "Where",
                    new IExpressionNode[]
                    {
                        new LambdaExpressionNode(new BinaryExpressionNode(BinaryTokenType.Equality, p1, ConstantExpressionNode.Get("test", typeof(String))),
                            new IParameterExpressionNode[] {p1})
                    }, new string[0]), "Aggregate",
                new IExpressionNode[]
                {
                    ConstantExpressionNode.Get("seed", typeof(String)),
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
            var value1 = new[] { value };
            var result = value1.Where<string>(x => x == "test").Aggregate<string, string, int>("seed", (s1, s2) => s1 + s2, s1 => s1.Length);
            var p1 = new ParameterExpressionNode("x");
            var p2 = new ParameterExpressionNode("s1");
            var p3 = new ParameterExpressionNode("s2");
            var node = new MethodCallExpressionNode(
                new MethodCallExpressionNode(ConstantExpressionNode.Get(value1), "Where",
                    new IExpressionNode[]
                    {
                        new LambdaExpressionNode(new BinaryExpressionNode(BinaryTokenType.Equality, p1, ConstantExpressionNode.Get("test", typeof(String))),
                            new IParameterExpressionNode[] {p1})
                    }, new string[] {"string"}), "Aggregate",
                new IExpressionNode[]
                {
                    ConstantExpressionNode.Get("seed", typeof(String)),
                    new LambdaExpressionNode(new BinaryExpressionNode(BinaryTokenType.Addition, p2, p3),
                        new IParameterExpressionNode[] {p2, p3}),
                    new LambdaExpressionNode(new MemberExpressionNode(p2, "Length"), new IParameterExpressionNode[] {p2})
                }, new string[] {"string", "string", "int"});
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
                        new BinaryExpressionNode(BinaryTokenType.Addition, new UnaryExpressionNode(UnaryTokenType.Minus, ConstantExpressionNode.Get(1, typeof(Int32))),
                            new BinaryExpressionNode(BinaryTokenType.Remainder,
                                new BinaryExpressionNode(BinaryTokenType.Division,
                                    new BinaryExpressionNode(BinaryTokenType.Multiplication, ConstantExpressionNode.Get(2, typeof(Int32)),
                                        new UnaryExpressionNode(UnaryTokenType.BitwiseNegation, ConstantExpressionNode.Get(1, typeof(Int32)))), ConstantExpressionNode.Get(8, typeof(Int32))),
                                ConstantExpressionNode.Get(1, typeof(Int32)))), ConstantExpressionNode.Get(4, typeof(Int32))), ConstantExpressionNode.Get(5, typeof(Int32))),
                ConstantExpressionNode.Get(10, typeof(Int32)));
            var compiler = GetInitializedCompiler();
            compiler.Compile(node).Invoke(default, DefaultMetadata).ShouldEqual(result);
        }

        private static ExpressionCompiler GetInitializedCompiler()
        {
            var memberManager = new MemberManager();
            memberManager.AddComponent(new MemberSelectorComponent());
            memberManager.AddComponent(new MemberManagerComponent());
            memberManager.AddComponent(new ReflectionMemberProviderComponent());
            memberManager.AddComponent(new ExtensionMethodMemberProviderComponent());

            var expressionCompiler = new ExpressionCompiler();
            expressionCompiler.AddComponent(new BinaryExpressionBuilderComponent());
            expressionCompiler.AddComponent(new ConditionExpressionBuilderComponent());
            expressionCompiler.AddComponent(new ConstantExpressionBuilderComponent());
            expressionCompiler.AddComponent(new LambdaExpressionBuilderComponent());
            expressionCompiler.AddComponent(new MemberExpressionBuilderComponent(memberManager));
            expressionCompiler.AddComponent(new MethodCallIndexerExpressionBuilderComponent(memberManager));
            expressionCompiler.AddComponent(new NullConditionalExpressionBuilderComponent());
            expressionCompiler.AddComponent(new UnaryExpressionBuilderComponent());
            expressionCompiler.AddComponent(new ExpressionCompilerComponent());
            return expressionCompiler;
        }

        protected override ExpressionCompiler GetComponentOwner(IComponentCollectionProvider? collectionProvider = null)
        {
            return new ExpressionCompiler(collectionProvider);
        }

        #endregion
    }
}