using System.Reflection;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Nodes;

namespace MugenMvvm.Binding.Parsing.Nodes
{
    public sealed class MemberExpressionNode : ExpressionNodeBase, IMemberExpressionNode
    {
        #region Constructors

        public MemberExpressionNode(IExpressionNode? target, string member)
        {
            Should.NotBeNull(member, nameof(member));
            Target = target;
            MemberName = member;
        }

        public MemberExpressionNode(IExpressionNode? target, MemberInfo member)
        {
            Should.NotBeNull(member, nameof(member));
            Target = target;
            Member = member;
            MemberName = member.Name;
        }

        #endregion

        #region Properties

        public override ExpressionNodeType NodeType => ExpressionNodeType.Member;

        public MemberInfo? Member { get; private set; }

        public string MemberName { get; }

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
                return new MemberExpressionNode(node, MemberName) { Member = Member };
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