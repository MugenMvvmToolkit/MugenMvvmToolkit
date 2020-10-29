using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Parsing.Expressions
{
    public sealed class NullConditionalMemberExpressionNode : ExpressionNodeBase, IHasTargetExpressionNode<NullConditionalMemberExpressionNode>
    {
        #region Constructors

        public NullConditionalMemberExpressionNode(IExpressionNode target)
        {
            Should.NotBeNull(target, nameof(target));
            Target = target;
        }

        #endregion

        #region Properties

        public override ExpressionNodeType ExpressionType => ExpressionNodeType.Member;

        public IExpressionNode? Target { get; }

        #endregion

        #region Implementation of interfaces

        public NullConditionalMemberExpressionNode UpdateTarget(IExpressionNode? target)
        {
            if (target == Target)
                return this;
            return new NullConditionalMemberExpressionNode(target!);
        }

        #endregion

        #region Methods

        protected override IExpressionNode Visit(IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata)
        {
            if (Target == null)
                return this;
            var changed = false;
            var node = VisitWithCheck(visitor, Target, false, ref changed, metadata);
            if (changed)
                return new NullConditionalMemberExpressionNode(node);
            return this;
        }

        public override string ToString() => Target + "?";

        #endregion
    }
}