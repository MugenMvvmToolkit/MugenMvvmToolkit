using System;
using MugenMvvm.Binding.Compiling;
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

        protected override ExpressionCompiler GetComponentOwner(IComponentCollectionProvider? collectionProvider = null)
        {
            return new ExpressionCompiler(collectionProvider);
        }

        #endregion
    }
}