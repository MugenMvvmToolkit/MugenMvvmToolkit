using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Parsing.Expressions
{
    public sealed class MemberExpressionNode : ExpressionNodeBase, IMemberExpressionNode
    {
        #region Fields

        public static readonly MemberExpressionNode Empty = new MemberExpressionNode(null, string.Empty);
        public static readonly MemberExpressionNode Source = new MemberExpressionNode(null, MacrosConstant.Source);
        public static readonly MemberExpressionNode Self = new MemberExpressionNode(null, MacrosConstant.Target);
        public static readonly MemberExpressionNode Context = new MemberExpressionNode(null, MacrosConstant.Context);
        public static readonly MemberExpressionNode Binding = new MemberExpressionNode(null, MacrosConstant.Binding);
        public static readonly MemberExpressionNode Args = new MemberExpressionNode(null, MacrosConstant.Args);

        #endregion

        #region Constructors

        public MemberExpressionNode(IExpressionNode? target, string member)
        {
            Should.NotBeNull(member, nameof(member));
            Target = target;
            Member = member;
        }

        #endregion

        #region Properties

        public override ExpressionNodeType ExpressionType => ExpressionNodeType.Member;

        public string Member { get; }

        public IExpressionNode? Target { get; }

        #endregion

        #region Implementation of interfaces

        public IMemberExpressionNode UpdateTarget(IExpressionNode? target)
        {
            if (ReferenceEquals(target, Target))
                return this;
            return new MemberExpressionNode(target, Member);
        }

        #endregion

        #region Methods

        public static MemberExpressionNode Get(IExpressionNode? target, string member)
        {
            if (target == null)
            {
                if (member == MacrosConstant.Self || member == MacrosConstant.This || member == MacrosConstant.Target)
                    return Self;
                if (member == MacrosConstant.Context)
                    return Context;
                if (member == MacrosConstant.Source)
                    return Source;
                if (member == MacrosConstant.Args)
                    return Args;
                if (member == MacrosConstant.Binding)
                    return Binding;
            }

            return new MemberExpressionNode(target, member);
        }

        protected override IExpressionNode VisitInternal(IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata)
        {
            if (Target == null)
                return this;
            var changed = false;
            var node = VisitWithCheck(visitor, Target, false, ref changed, metadata);
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