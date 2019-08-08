using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing;

namespace MugenMvvm.Binding.Parsing
{
    public sealed class MemberExpressionNode : ExpressionNodeBase, IMemberExpressionNode
    {
        #region Constructors

        public MemberExpressionNode(IExpressionNode? target, string member)
        {
            Should.NotBeNull(member, nameof(member));
            Target = target;
            Member = member;
        }

        #endregion

        #region Properties

        public override ExpressionNodeType NodeType => ExpressionNodeType.Member;

        public string Member { get; }

        public IExpressionNode? Target { get; }

        #endregion

        #region Methods

        protected override IExpressionNode VisitInternal(IExpressionVisitor visitor)
        {
            if (Target == null)
                return this;
            var changed = false;
            var node = VisitWithCheck(visitor, Target, false, ref changed);
            if (changed)
                return new MemberExpressionNode(node, Member);
            return this;
        }

        public override string ToString()
        {
            if (Target == null)
                return Member;
            return $"{Target}.{Member}";
        }

        #endregion
    }
}