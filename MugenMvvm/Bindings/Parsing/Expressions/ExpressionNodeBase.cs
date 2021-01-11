using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Parsing.Expressions
{
    public abstract class ExpressionNodeBase<TExpression> : IExpressionNode
        where TExpression : class, IExpressionNode
    {
        #region Constructors

        protected ExpressionNodeBase(IReadOnlyDictionary<string, object?>? metadata)
        {
            Metadata = metadata ?? Default.ReadOnlyDictionary<string, object?>();
        }

        #endregion

        #region Properties

        public abstract ExpressionNodeType ExpressionType { get; }

        public IReadOnlyDictionary<string, object?> Metadata { get; }

        #endregion

        #region Implementation of interfaces

        public IExpressionNode Accept(IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(visitor, nameof(visitor));
            var node = AcceptInternal(visitor, metadata);
            return ReferenceEquals(node, this) ? node : node.Accept(visitor, metadata);
        }

        public IExpressionNode UpdateMetadata(IReadOnlyDictionary<string, object?>? metadata) => this.MetadataEquals(metadata ??= Default.ReadOnlyDictionary<string, object?>()) ? this : Clone(metadata);

        public bool Equals(IExpressionNode? other, IExpressionEqualityComparer? comparer)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            var equals = comparer?.Equals(this, other);
            if (equals.HasValue)
                return equals.Value;
            return ExpressionType == other.ExpressionType && other is TExpression exp && this.MetadataEquals(other.Metadata) && Equals(exp, comparer);
        }

        public int GetHashCode(IExpressionEqualityComparer? comparer)
        {
            var hash = comparer?.GetHashCode(this);
            if (hash.HasValue)
                return hash.Value;
            return GetHashCode(ExpressionType.Value * 397 ^ Metadata.Count, comparer);
        }

        public bool Equals(IExpressionNode? other) => Equals(other, null);

        #endregion

        #region Methods

        protected abstract IExpressionNode Visit(IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata);

        protected abstract TExpression Clone(IReadOnlyDictionary<string, object?> metadata);

        protected abstract bool Equals(TExpression other, IExpressionEqualityComparer? comparer);

        protected abstract int GetHashCode(int hashCode, IExpressionEqualityComparer? comparer);

        public sealed override bool Equals(object? obj) => Equals(obj as IExpressionNode);

        public sealed override int GetHashCode() => GetHashCode(null);

        protected virtual IExpressionNode AcceptInternal(IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata)
        {
            IExpressionNode? node;
            var changed = false;
            if (visitor.TraversalType == ExpressionTraversalType.Preorder)
            {
                node = VisitWithCheck<IExpressionNode>(visitor, this, true, ref changed, metadata);
                if (changed)
                    return node;
            }

            node = Visit(visitor, metadata);
            if (visitor.TraversalType != ExpressionTraversalType.Preorder)
                return VisitWithCheck(visitor, node, true, ref changed, metadata);
            return node;
        }

        protected T VisitWithCheck<T>(IExpressionVisitor visitor, T node, bool notNull, ref bool changed, IReadOnlyMetadataContext? metadata)
            where T : class, IExpressionNode
        {
            var result = ReferenceEquals(this, node) ? visitor.Visit(node, metadata) : node.Accept(visitor, metadata);
            if (!changed && result != node)
                changed = true;
            if (notNull && result == null)
                ExceptionManager.ThrowExpressionNodeCannotBeNull(GetType());
            return (T) result!;
        }

        protected ItemOrIReadOnlyList<T> VisitWithCheck<T>(IExpressionVisitor visitor, ItemOrIReadOnlyList<T> nodes, ref bool changed, IReadOnlyMetadataContext? metadata)
            where T : class, IExpressionNode
        {
            T[]? newArgs = null;
            for (var i = 0; i < nodes.Count; i++)
            {
                var itemsChanged = false;
                var node = VisitWithCheck(visitor, nodes[i], true, ref itemsChanged, metadata);
                if (!itemsChanged)
                    continue;

                newArgs ??= nodes.ToArray();
                newArgs[i] = node;
            }

            if (!changed && newArgs != null)
                changed = true;
            return newArgs ?? nodes;
        }

        protected static bool Equals(IExpressionNode? x1, IExpressionNode? x2, IExpressionEqualityComparer? comparer)
        {
            if (ReferenceEquals(x1, x2))
                return true;
            if (ReferenceEquals(x1, null) || ReferenceEquals(x2, null))
                return false;

            return x1.Equals(x2, comparer);
        }

        protected static bool Equals<T>(ItemOrIReadOnlyList<T> x1, ItemOrIReadOnlyList<T> x2, IExpressionEqualityComparer? comparer)
            where T : class, IExpressionNode
        {
            if (ReferenceEquals(x1.Item, x2.Item) && ReferenceEquals(x1.List, x2.List))
                return true;
            var count = x1.Count;
            if (count != x2.Count)
                return false;
            for (int i = 0; i < count; i++)
            {
                if (!x1[i].Equals(x2[i], comparer))
                    return false;
            }

            return true;
        }

        protected static int GetHashCode<T>(int hashCode, IExpressionNode? target, ItemOrIReadOnlyList<T> args, IExpressionEqualityComparer? comparer)
            where T : class, IExpressionNode
        {
            if (target == null)
                return HashCode.Combine(hashCode, args.Count);
            return HashCode.Combine(hashCode, args.Count, target.GetHashCode(comparer));
        }

        #endregion
    }
}