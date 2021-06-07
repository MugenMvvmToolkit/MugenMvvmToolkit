using System;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Bindings.Parsing
{
    public class TestTokenParserComponent : ITokenParserComponent, IHasPriority
    {
        public Func<ITokenParserContext, IExpressionNode?, IExpressionNode?>? TryParse { get; set; }

        public int Priority { get; set; }

        IExpressionNode? ITokenParserComponent.TryParse(ITokenParserContext context, IExpressionNode? expression) => TryParse?.Invoke(context, expression);
    }
}