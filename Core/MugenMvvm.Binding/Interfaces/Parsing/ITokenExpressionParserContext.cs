using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Parsing
{
    public interface ITokenExpressionParserContext : IExpressionParserContext //todo use span/memory?
    {
        int Position { get; }

        int Length { get; }

        char TokenAt(int position);

        string GetValue(int start, int end);

        void SetPosition(int position);

        IExpressionNode? TryParse(IExpressionNode? expression = null, IReadOnlyMetadataContext? metadata = null);
    }
}