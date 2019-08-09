using MugenMvvm.Binding.Interfaces.Parsing.Nodes;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Parsing
{
    public interface IBindingParserContext : IMetadataOwner<IMetadataContext>
    {
        string Source { get; }

        int Position { get; }

        void SetPosition(int position);

        IExpressionNode Parse(IExpressionNode? expression, IReadOnlyMetadataContext? metadata);
    }
}