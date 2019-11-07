using System;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Parsing
{
    public interface ITokenParserContext : IMetadataOwner<IMetadataContext>
    {
        int Position { get; set; }

        int? Limit { get; set; }

        int Length { get; }

        char TokenAt(int position);

        string GetValue(int start, int end);

        IExpressionNode? TryParse(IExpressionNode? expression = null, Func<ITokenParserComponent, bool>? condition = null);
    }
}