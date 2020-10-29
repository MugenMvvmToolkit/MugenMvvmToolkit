using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Interfaces.Compiling.Components
{
    public interface IExpressionCompilerComponent : IComponent<IExpressionCompiler>
    {
        ICompiledExpression? TryCompile(IExpressionCompiler compiler, IExpressionNode expression, IReadOnlyMetadataContext? metadata);
    }
}