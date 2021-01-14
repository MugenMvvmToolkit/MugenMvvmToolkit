using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using MugenMvvm.Bindings.Interfaces.Compiling;
using MugenMvvm.Bindings.Interfaces.Compiling.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Extensions.Components
{
    public static class CompilingComponentExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ICompiledExpression? TryCompile(this ItemOrArray<IExpressionCompilerComponent> components, IExpressionCompiler compiler, IExpressionNode expression,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(compiler, nameof(compiler));
            Should.NotBeNull(expression, nameof(expression));
            foreach (var c in components)
            {
                var compiledExpression = c.TryCompile(compiler, expression, metadata);
                if (compiledExpression != null)
                    return compiledExpression;
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Expression? TryBuild(this ItemOrArray<IExpressionBuilderComponent> components, IExpressionBuilderContext context, IExpressionNode expression)
        {
            Should.NotBeNull(expression, nameof(expression));
            foreach (var c in components)
            {
                var exp = c.TryBuild(context, expression);
                if (exp != null)
                    return exp;
            }

            return null;
        }
    }
}