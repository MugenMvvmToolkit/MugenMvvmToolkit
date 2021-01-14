using System;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Parsing;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Internal
{
    public class TestExpressionParserComponent : IExpressionParserComponent, IHasPriority
    {
        private readonly IExpressionParser? _parser;

        public TestExpressionParserComponent(IExpressionParser? parser = null)
        {
            _parser = parser;
        }

        public Func<object, IReadOnlyMetadataContext?, ItemOrIReadOnlyList<ExpressionParserResult>>? TryParse { get; set; }

        public int Priority { get; set; }

        ItemOrIReadOnlyList<ExpressionParserResult> IExpressionParserComponent.TryParse(IExpressionParser parser, object expression, IReadOnlyMetadataContext? metadata)
        {
            _parser?.ShouldEqual(parser);
            return TryParse?.Invoke(expression, metadata) ?? default;
        }
    }
}