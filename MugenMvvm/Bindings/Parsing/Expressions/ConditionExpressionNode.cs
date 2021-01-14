using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Parsing.Expressions
{
    public sealed class ConditionExpressionNode : ExpressionNodeBase<IConditionExpressionNode>, IConditionExpressionNode
    {
        public ConditionExpressionNode(IExpressionNode condition, IExpressionNode ifTrue, IExpressionNode ifFalse, IReadOnlyDictionary<string, object?>? metadata = null) :
            base(metadata)
        {
            Should.NotBeNull(condition, nameof(condition));
            Should.NotBeNull(ifTrue, nameof(ifTrue));
            Should.NotBeNull(ifFalse, nameof(ifFalse));
            Condition = condition;
            IfTrue = ifTrue;
            IfFalse = ifFalse;
        }

        public IExpressionNode Condition { get; }

        public IExpressionNode IfTrue { get; }

        public IExpressionNode IfFalse { get; }

        public override ExpressionNodeType ExpressionType => ExpressionNodeType.Condition;

        public override string ToString() => $"if ({Condition}) {{{IfTrue}}} else {{{IfFalse}}}";

        protected override IExpressionNode Visit(IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata)
        {
            var changed = false;
            var condition = VisitWithCheck(visitor, Condition, true, ref changed, metadata);
            var ifTrue = VisitWithCheck(visitor, IfTrue, true, ref changed, metadata);
            var ifFalse = VisitWithCheck(visitor, IfFalse, true, ref changed, metadata);
            if (changed)
                return new ConditionExpressionNode(condition, ifTrue, ifFalse, Metadata);
            return this;
        }

        protected override IConditionExpressionNode Clone(IReadOnlyDictionary<string, object?> metadata) => new ConditionExpressionNode(Condition, IfTrue, IfFalse, metadata);

        protected override bool Equals(IConditionExpressionNode other, IExpressionEqualityComparer? comparer) =>
            Condition.Equals(other.Condition, comparer) && IfTrue.Equals(other.IfTrue, comparer) && IfFalse.Equals(other.IfFalse, comparer);

        protected override int GetHashCode(int hashCode, IExpressionEqualityComparer? comparer) =>
            HashCode.Combine(hashCode, Condition.GetHashCode(comparer), IfTrue.GetHashCode(comparer), IfFalse.GetHashCode(comparer));
    }
}