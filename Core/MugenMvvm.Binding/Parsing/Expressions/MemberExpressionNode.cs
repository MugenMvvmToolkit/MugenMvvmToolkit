using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;

namespace MugenMvvm.Binding.Parsing.Expressions
{
    public sealed class MemberExpressionNode : ExpressionNodeBase, IMemberExpressionNode
    {
        #region Fields

        internal static readonly MemberExpressionNode Source = new MemberExpressionNode(null, MacrosConstants.Source);
        internal static readonly MemberExpressionNode Self = new MemberExpressionNode(null, MacrosConstants.Target);
        internal static readonly MemberExpressionNode Context = new MemberExpressionNode(null, MacrosConstants.Context);
        internal static readonly MemberExpressionNode Binding = new MemberExpressionNode(null, MacrosConstants.Binding);
        internal static readonly MemberExpressionNode Args = new MemberExpressionNode(null, MacrosConstants.Args);

        #endregion

        #region Constructors

        private MemberExpressionNode(IExpressionNode? target, IMemberAccessorInfo? memberInfo, string member)
        {
            Should.NotBeNull(member, nameof(member));
            Target = target;
            Member = memberInfo;
            MemberName = member;
        }

        public MemberExpressionNode(IExpressionNode? target, string member) : this(target, null, member)
        {
        }

        public MemberExpressionNode(IExpressionNode? target, IMemberAccessorInfo member)
            : this(target, member, member?.Name!)
        {
        }

        #endregion

        #region Properties

        public override ExpressionNodeType NodeType => ExpressionNodeType.Member;

        public IMemberAccessorInfo? Member { get; }

        public string MemberName { get; }

        public IExpressionNode? Target { get; }

        #endregion

        #region Implementation of interfaces

        public IMemberExpressionNode UpdateTarget(IExpressionNode? target)
        {
            if (ReferenceEquals(target, Target))
                return this;

            return new MemberExpressionNode(target, Member, MemberName);
        }

        #endregion

        #region Methods

        public static MemberExpressionNode Get(IExpressionNode? target, string member)
        {
            if (target == null)
            {
                if (member == MacrosConstants.Self || member == MacrosConstants.This || member == MacrosConstants.Target)
                    return Self;
                if (member == MacrosConstants.Context)
                    return Context;
                if (member == MacrosConstants.Source)
                    return Source;
                if (member == MacrosConstants.Args)
                    return Args;
                if (member == MacrosConstants.Binding)
                    return Binding;
            }

            return new MemberExpressionNode(target, member);
        }

        protected override IExpressionNode VisitInternal(IExpressionVisitor visitor)
        {
            if (Target == null)
                return this;
            var changed = false;
            var node = VisitWithCheck(visitor, Target, false, ref changed);
            if (changed)
                return new MemberExpressionNode(node, Member, MemberName);
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