using System.Linq.Expressions;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Compiling.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Extensions.Components
{
    public static class CompilingComponentExtensions
    {
        #region Methods

        public static ICompiledExpression? TryCompile(this IExpressionCompilerComponent[] components, IExpressionNode expression, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                var compiledExpression = components[i].TryCompile(expression, metadata);
                if (compiledExpression != null)
                    return compiledExpression;
            }

            return null;
        }

        public static Expression? TryBuild(this IExpressionBuilderCompilerComponent[] components, IExpressionBuilderContext context, IExpressionNode expression)
        {
            Should.NotBeNull(components, nameof(components));
            for (var index = 0; index < components.Length; index++)
            {
                var exp = components[index].TryBuild(context, expression);
                if (exp != null)
                    return exp;
            }

            return null;
        }

        #endregion
    }
}