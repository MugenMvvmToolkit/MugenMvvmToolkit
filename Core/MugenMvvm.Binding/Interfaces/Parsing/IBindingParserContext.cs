using MugenMvvm.Binding.Interfaces.Parsing.Nodes;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Parsing
{
    public interface IBindingParserContext : IMetadataOwner<IMetadataContext>//todo use span/memory?
    {
        int Position { get; }

        int Length { get; }

        char TokenAt(int position);

        string GetValue(int start, int end);

        void SetPosition(int position);

        void SetLimit(int? limit);

        IExpressionNode? TryParse(IExpressionNode? expression = null, IReadOnlyMetadataContext? metadata = null);
    }
}