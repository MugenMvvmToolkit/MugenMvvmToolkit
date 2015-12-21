#region Copyright

// ****************************************************************************
// <copyright file="MemberExpressionNode.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces.Parse;
using MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes;
using MugenMvvmToolkit.Binding.Models;

namespace MugenMvvmToolkit.Binding.Parse.Nodes
{
    public class MemberExpressionNode : ExpressionNode, IMemberExpressionNode
    {
        #region Fields

        private readonly string _member;
        private IExpressionNode _target;

        #endregion

        #region Constructors

        public MemberExpressionNode(IExpressionNode target, [NotNull] string member)
            : base(ExpressionNodeType.Member)
        {
            Should.NotBeNull(member, nameof(member));
            _target = target;
            _member = member;
        }

        #endregion

        #region Implementation of IMemberExpressionNode

        public IExpressionNode Target
        {
            get { return _target; }
        }

        public string Member
        {
            get { return _member; }
        }

        #endregion

        #region Overrides of ExpressionNode

        protected override void AcceptInternal(IExpressionVisitor visitor)
        {
            if (Target != null)
                _target = AcceptWithCheck(visitor, Target, false);
        }

        protected override IExpressionNode CloneInternal()
        {
            return new MemberExpressionNode(Target == null ? null : Target.Clone(), Member);
        }

        public override string ToString()
        {
            if (Target == null)
                return string.IsNullOrEmpty(Member) ? AttachedMemberConstants.DataContext : Member;
            return $"{Target}.{Member}";
        }

        #endregion
    }
}
