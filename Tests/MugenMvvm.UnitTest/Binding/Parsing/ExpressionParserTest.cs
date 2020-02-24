using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Parsing;
using MugenMvvm.Binding.Parsing.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.UnitTest.Components;

namespace MugenMvvm.UnitTest.Binding.Parsing
{
    public class ExpressionParserTest : ComponentOwnerTestBase<ExpressionParser>
    {
        #region Methods

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