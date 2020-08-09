using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Parsing.Expressions
{
    public sealed class ConditionExpressionNode : ExpressionNodeBase, IConditionExpressionNode
    {
        #region Constructors

        public ConditionExpressionNode(IExpressionNode condition, IExpressionNode ifTrue, IExpressionNode ifFalse)
        {
            Should.NotBeNull(condition, nameof(condition));
            Should.NotBeNull(ifTrue, nameof(ifTrue));
            Should.NotBeNull(ifFalse, nameof(ifFalse));
            Condition = condition;
            IfTrue = ifTrue;
            IfFalse = ifFalse;
        }

        #endregion

        #region Properties

        public override ExpressionNodeType ExpressionType => ExpressionNodeType.Condition;

        public IExpressionNode Condition { get; }

        public IExpressionNode IfTrue { get; }

        public IExpressionNode IfFalse { get; }

        #endregion

        #region Methods

        protected override IExpressionNode Visit(IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata)
        {
            var changed = false;
            var condition = VisitWithCheck(visitor, Condition, true, ref changed, metadata);
            var ifTrue = VisitWithCheck(visitor, IfTrue, true, ref changed, metadata);
            var ifFalse = VisitWithCheck(visitor, IfFalse, true, ref changed, metadata);
            if (changed)
                return new ConditionExpressionNode(condition, ifTrue, ifFalse);
            return this;
        }

        public override string ToString() => $"if ({Condition}) {{{IfTrue}}} else {{{IfFalse}}}";

        #endregion
    }
}