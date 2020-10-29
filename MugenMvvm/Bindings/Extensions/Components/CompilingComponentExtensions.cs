using System.Linq.Expressions;
using MugenMvvm.Bindings.Interfaces.Compiling;
using MugenMvvm.Bindings.Interfaces.Compiling.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Extensions.Components
{
    public static class CompilingComponentExtensions
    {
        #region Methods

        public static ICompiledExpression? TryCompile(this IExpressionCompilerComponent[] components, IExpressionCompiler compiler, IExpressionNode expression, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(compiler, nameof(compiler));
            Should.NotBeNull(expression, nameof(expression));
            for (var i = 0; i < components.Length; i++)
            {
                var compiledExpression = components[i].TryCompile(compiler, expression, metadata);
                if (compiledExpression != null)
                    return compiledExpression;
            }

            return null;
        }

        public static Expression? TryBuild(this IExpressionBuilderComponent[] components, IExpressionBuilderContext context, IExpressionNode expression)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(expression, nameof(expression));
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