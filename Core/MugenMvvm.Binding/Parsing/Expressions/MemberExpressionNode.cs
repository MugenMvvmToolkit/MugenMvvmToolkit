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

        public static readonly MemberExpressionNode Action = new MemberExpressionNode(null, MacrosConstant.Action);
        public static readonly MemberExpressionNode EventArgs = new MemberExpressionNode(null, MacrosConstant.EventArgs);
        public static readonly MemberExpressionNode Source = new MemberExpressionNode(null, MacrosConstant.Source);
        public static readonly MemberExpressionNode Self = new MemberExpressionNode(null, MacrosConstant.Target);
        public static readonly MemberExpressionNode Context = new MemberExpressionNode(null, MacrosConstant.Context);
        public static readonly MemberExpressionNode Binding = new MemberExpressionNode(null, MacrosConstant.Binding);
        public static readonly MemberExpressionNode Empty = new MemberExpressionNode(null, string.Empty);

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
                if (member == MacrosConstant.EventArgs)
                    return EventArgs;
                if (member == MacrosConstant.Binding)
                    return Binding;
                if (member == MacrosConstant.Action)
                    return Action;
                if (member == "")
                    return Empty;
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