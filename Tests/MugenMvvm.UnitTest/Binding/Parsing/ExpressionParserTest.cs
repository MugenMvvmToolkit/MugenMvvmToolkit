using System;
using MugenMvvm.Binding.Interfaces.Parsing;
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

        protected override ExpressionParser GetComponentOwner(IComponentCollectionProvider? collectionProvider = null)
        {
            return new ExpressionParser(collectionProvider);
        }

        private static IExpressionParser GetInitializedExpressionParser()
        {
            var expressionParser = new ExpressionParser();
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