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
    /// <summary>
    ///     Represents accessing a field or property.
    /// </summary>
    public class MemberExpressionNode : ExpressionNode, IMemberExpressionNode
    {
        #region Fields

        private readonly string _member;
        private IExpressionNode _target;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="MemberExpressionNode" /> class.
        /// </summary>
        public MemberExpressionNode(IExpressionNode target, [NotNull] string member)
            : base(ExpressionNodeType.Member)
        {
            Should.NotBeNull(member, "member");
            _target = target;
            _member = member;
        }

        #endregion

        #region Implementation of IMemberExpressionNode

        /// <summary>
        ///     Gets the containing object of the field or property.
        /// </summary>
        public IExpressionNode Target
        {
            get { return _target; }
        }

        /// <summary>
        ///     Gets the field or property to be accessed.
        /// </summary>
        public string Member
        {
            get { return _member; }
        }

        #endregion

        #region Overrides of ExpressionNode

        /// <summary>
        ///     Calls the visitor on the expression.
        /// </summary>
        protected override void AcceptInternal(IExpressionVisitor visitor)
        {
            if (Target != null)
                _target = AcceptWithCheck(visitor, Target, false);
        }

        /// <summary>
        ///     Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        ///     A new object that is a copy of this instance.
        /// </returns>
        protected override IExpressionNode CloneInternal()
        {
            return new MemberExpressionNode(Target == null ? null : Target.Clone(), Member);
        }

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            if (Target == null)
                return string.IsNullOrEmpty(Member) ? AttachedMemberConstants.DataContext : Member;
            return string.Format("{0}.{1}", Target, Member);
        }

        #endregion
    }
}