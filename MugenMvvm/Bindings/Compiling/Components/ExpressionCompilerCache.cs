using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Extensions.Components;
using MugenMvvm.Bindings.Interfaces.Compiling;
using MugenMvvm.Bindings.Interfaces.Compiling.Components;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions.Binding;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Compiling.Components
{
    public sealed class ExpressionCompilerCache : ComponentCacheBase<IExpressionCompiler, IExpressionCompilerComponent>, IExpressionCompilerComponent,
        IExpressionEqualityComparer, IEqualityComparer<IExpressionNode>, IExpressionVisitor
    {
        private static readonly IExpressionNode EmptyNode = new BindingInstanceMemberExpressionNode(ConstantExpressionNode.Null, "", 0, default, default);
        private readonly Dictionary<IExpressionNode, ICompiledExpression?> _cache;

        public ExpressionCompilerCache(int priority = CompilingComponentPriority.Cache) : base(priority)
        {
            _cache = new Dictionary<IExpressionNode, ICompiledExpression?>(59, this);
        }

        ExpressionTraversalType IExpressionVisitor.TraversalType => ExpressionTraversalType.Postorder;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsValueExpression(IExpressionNode expression) => expression.ExpressionType == ExpressionNodeType.BindingParameter;

        public override void Invalidate(object? state = null, IReadOnlyMetadataContext? metadata = null)
        {
            if (state is IExpressionNode expression)
                _cache.Remove(expression);
            else
                _cache.Clear();
        }

        public ICompiledExpression? TryCompile(IExpressionCompiler compiler, IExpressionNode expression, IReadOnlyMetadataContext? metadata)
        {
            if (!_cache.TryGetValue(expression, out var result))
            {
                result = Components.TryCompile(compiler, expression, metadata);
                _cache[expression.Accept(this, metadata)] = result;
            }

            return result;
        }

        int IEqualityComparer<IExpressionNode>.GetHashCode(IExpressionNode obj) => obj.GetHashCode(this);

        bool IEqualityComparer<IExpressionNode>.Equals(IExpressionNode? x, IExpressionNode? y) => x!.Equals(y!, this);

        bool? IExpressionEqualityComparer.Equals(IExpressionNode x, IExpressionNode y) => IsValueExpression(x) && IsValueExpression(y) ? x.MetadataEquals(y.Metadata) : null;

        int? IExpressionEqualityComparer.GetHashCode(IExpressionNode expression) => IsValueExpression(expression) ? 0 : null;

        //replacing all value expressions with null constant to prevent memory leaks
        IExpressionNode IExpressionVisitor.Visit(IExpressionNode expression, IReadOnlyMetadataContext? metadata) => IsValueExpression(expression) ? EmptyNode : expression;
    }
}