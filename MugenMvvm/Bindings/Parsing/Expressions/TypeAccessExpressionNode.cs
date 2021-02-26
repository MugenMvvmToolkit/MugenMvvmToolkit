using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Parsing.Expressions
{
    public sealed class TypeAccessExpressionNode : ExpressionNodeBase<ITypeAccessExpressionNode>, ITypeAccessExpressionNode
    {
        public TypeAccessExpressionNode(Type type, IReadOnlyDictionary<string, object?>? metadata = null) : base(metadata)
        {
            Should.NotBeNull(type, nameof(type));
            Type = type;
        }

        public override ExpressionNodeType ExpressionType => ExpressionNodeType.TypeAccess;

        public Type Type { get; }

        public static TypeAccessExpressionNode Get(Type type, IReadOnlyDictionary<string, object?>? metadata = null) => new(type, metadata);

        public static TypeAccessExpressionNode Get<TType>() => TypeCache<TType>.Instance;

        public override string ToString() => Type.ToString();

        protected override IExpressionNode Visit(IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata) => this;

        protected override ITypeAccessExpressionNode Clone(IReadOnlyDictionary<string, object?> metadata) => new TypeAccessExpressionNode(Type, metadata);

        protected override bool Equals(ITypeAccessExpressionNode other, IExpressionEqualityComparer? comparer) => Type == other.Type;

        protected override int GetHashCode(int hashCode, IExpressionEqualityComparer? comparer) => HashCode.Combine(hashCode, Type);

        private static class TypeCache<TType>
        {
            public static readonly TypeAccessExpressionNode Instance = new(typeof(TType));
        }
    }
}