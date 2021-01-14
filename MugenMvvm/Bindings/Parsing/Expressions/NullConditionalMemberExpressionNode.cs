using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Parsing.Expressions
{
    public sealed class NullConditionalMemberExpressionNode : ExpressionNodeBase<NullConditionalMemberExpressionNode>, IHasTargetExpressionNode<NullConditionalMemberExpressionNode>
    {
        public NullConditionalMemberExpressionNode(IExpressionNode target, IReadOnlyDictionary<string, object?>? metadata = null) : base(metadata)
        {
            Should.NotBeNull(target, nameof(target));
            Target = target;
        }

        public override ExpressionNodeType ExpressionType => ExpressionNodeType.Member;

        public IExpressionNode Target { get; }

        public override string ToString() => Target + "?";

        public NullConditionalMemberExpressionNode UpdateTarget(IExpressionNode? target)
        {
            Should.NotBeNull(target, nameof(target));
            return Target.Equals(target) ? this : new NullConditionalMemberExpressionNode(target, Metadata);
        }

        protected override IExpressionNode Visit(IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata)
        {
            var changed = false;
            var node = VisitWithCheck(visitor, Target, false, ref changed, metadata);
            if (changed)
                return new NullConditionalMemberExpressionNode(node, Metadata);
            return this;
        }

        protected override NullConditionalMemberExpressionNode Clone(IReadOnlyDictionary<string, object?> metadata) => new(Target, metadata);

        protected override bool Equals(NullConditionalMemberExpressionNode other, IExpressionEqualityComparer? comparer) => Target.Equals(other.Target, comparer);

        protected override int GetHashCode(int hashCode, IExpressionEqualityComparer? comparer) => HashCode.Combine(hashCode, Target.GetHashCode(comparer));
    }
}