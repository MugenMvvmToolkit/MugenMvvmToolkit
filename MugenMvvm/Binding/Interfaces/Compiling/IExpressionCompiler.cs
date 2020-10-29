using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Interfaces.Compiling
{
    public interface IExpressionCompiler : IComponentOwner<IExpressionCompiler>
    {
        ICompiledExpression? TryCompile(IExpressionNode expression, IReadOnlyMetadataContext? metadata = null);
    }
}