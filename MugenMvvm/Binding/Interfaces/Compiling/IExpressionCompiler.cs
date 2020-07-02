using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Compiling
{
    public interface IExpressionCompiler : IComponentOwner<IExpressionCompiler>
    {
        ICompiledExpression? TryCompile(IExpressionNode expression, IReadOnlyMetadataContext? metadata = null);
    }
}