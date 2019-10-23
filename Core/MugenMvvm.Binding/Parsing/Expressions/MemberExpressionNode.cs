using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;

namespace MugenMvvm.Binding.Parsing.Expressions
{
    public sealed class MemberExpressionNode : ExpressionNodeBase, IMemberExpressionNode
    {
        #region Constructors

        private MemberExpressionNode(IExpressionNode? target, IBindingMemberAccessorInfo? memberInfo, string member)
        {
            Should.NotBeNull(member, nameof(member));
            Target = target;
            Member = memberInfo;
            MemberName = member;
        }

        public MemberExpressionNode(IExpressionNode? target, string member) : this(target, null, member)
        {
        }

        public MemberExpressionNode(IExpressionNode? target, IBindingMemberAccessorInfo member)
            : this(target, member, member?.Name!)
        {
        }

        #endregion

        #region Properties

        public override ExpressionNodeType NodeType => ExpressionNodeType.Member;

        public IBindingMemberAccessorInfo? Member { get; private set; }

        public string MemberName { get; }

        public IExpressionNode? Target { get; }

        #endregion

        #region Implementation of interfaces

        public IHasTargetExpressionNode UpdateTarget(IExpressionNode? target)
        {
            return new MemberExpressionNode(target, Member, MemberName);
        }

        #endregion

        #region Methods

        protected override IExpressionNode VisitInternal(IExpressionVisitor visitor)
        {
            if (Target == null)
                return this;
            var changed = false;
            var node = VisitWithCheck(visitor, Target, false, ref changed);
            if (changed)
                return new MemberExpressionNode(node, MemberName) {Member = Member};
            return this;
        }

        public override string ToString()
        {
            if (Target == null)
                return MemberName;
            return $"{Target}.{MemberName}";
        }

        #endregion
    }
}