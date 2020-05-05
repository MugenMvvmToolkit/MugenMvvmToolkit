using System;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Parsing;
using MugenMvvm.Binding.Parsing.Components;
using MugenMvvm.Binding.Parsing.Components.Parsers;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
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

        protected override ExpressionParser GetComponentOwner(IComponentCollectionProvider? collectionProvider = null)
        {
            return new ExpressionParser(collectionProvider);
        }

        private static IExpressionParser GetInitializedExpressionParser()
        {
            var expressionParser = new ExpressionParser();
            expressionParser.AddComponent(new BinaryTokenParserComponent());
            expressionParser.AddComponent(new ConditionTokenParserComponent());
            expressionParser.AddComponent(new ConstantTokenParserComponent());
            expressionParser.AddComponent(new DigitTokenParserComponent());
            expressionParser.AddComponent(new IndexerTokenParserComponent());
            expressionParser.AddComponent(new LambdaTokenParserComponent());
            expressionParser.AddComponent(new MemberTokenParserComponent());
            expressionParser.AddComponent(new MethodCallTokenParserComponent());
            expressionParser.AddComponent(new NullConditionalMemberTokenParserComponent());
            expressionParser.AddComponent(new ParenTokenParserComponent());
            expressionParser.AddComponent(new StringTokenParserComponent());
            expressionParser.AddComponent(new TokenExpressionParserComponent());
            expressionParser.AddComponent(new UnaryTokenParserComponent());
            return expressionParser;
        }

        #endregion
    }
}