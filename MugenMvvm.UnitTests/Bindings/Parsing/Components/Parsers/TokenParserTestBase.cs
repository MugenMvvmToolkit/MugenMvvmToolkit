using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Parsing;
using MugenMvvm.Collections;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Components.Parsers
{
    public abstract class TokenParserTestBase<T> : UnitTestBase where T : class, ITokenParserComponent, new()
    {
        protected readonly T Parser;
        protected readonly TokenParserContext Context;

        protected TokenParserTestBase(ItemOrArray<ITokenParserComponent> parsers = default, ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            Parser = new T();
            Context = new TokenParserContext
            {
                Parsers = parsers
            };
        }
    }
}