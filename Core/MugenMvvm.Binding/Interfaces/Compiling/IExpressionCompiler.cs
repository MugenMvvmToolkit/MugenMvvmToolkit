using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Compiling
{
    public interface IExpressionCompiler : IComponentOwner<IExpressionCompiler>, IComponent<IMugenApplication>
    {
        ICompiledExpression? TryCompile(IExpressionNode expression, IReadOnlyMetadataContext? metadata = null);
    }
}