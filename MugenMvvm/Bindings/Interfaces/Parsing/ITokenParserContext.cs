using System;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;

namespace MugenMvvm.Bindings.Interfaces.Parsing
{
    public interface ITokenParserContext : IParserContext
    {
        int Position { get; set; }

        int? Limit { get; set; }

        int Length { get; }

        char TokenAt(int position);

        string GetValue(int start, int end);

#if SPAN_API
        ReadOnlySpan<char> GetValueSpan(int start, int end);
#endif

        IExpressionNode? TryParse(IExpressionNode? expression = null, Func<ITokenParserContext, ITokenParserComponent, bool>? condition = null);
    }
}