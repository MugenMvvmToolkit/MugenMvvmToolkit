using System;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Parsing
{
    public interface ITokenParserContext : IMetadataOwner<IMetadataContext>
    {
        int Position { get; }

        int Length { get; }

        char TokenAt(int position);

        string GetValue(int start, int end);

        void SetPosition(int position);

        void SetLimit(int? limit);

        IExpressionNode? TryParse(IExpressionNode? expression = null, Func<ITokenParserComponent, bool>? condition = null);
    }
}