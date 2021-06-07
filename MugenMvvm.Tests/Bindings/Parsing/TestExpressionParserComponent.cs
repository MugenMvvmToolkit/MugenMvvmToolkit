using System;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Parsing;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Bindings.Parsing
{
    public class TestExpressionParserComponent : IExpressionParserComponent, IHasPriority
    {
        public Func<IExpressionParser, object, IReadOnlyMetadataContext?, ItemOrIReadOnlyList<ExpressionParserResult>>? TryParse { get; set; }

        public int Priority { get; set; }

        ItemOrIReadOnlyList<ExpressionParserResult> IExpressionParserComponent.TryParse(IExpressionParser parser, object expression, IReadOnlyMetadataContext? metadata) =>
            TryParse?.Invoke(parser, expression, metadata) ?? default;
    }
}