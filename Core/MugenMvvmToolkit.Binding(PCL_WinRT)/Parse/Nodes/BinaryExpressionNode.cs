#region Copyright
// ****************************************************************************
// <copyright file="BinaryExpressionNode.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
    ///     Represents an expression that has a binary operator.
    /// </summary>
    public class BinaryExpressionNode : ExpressionNode, IBinaryExpressionNode
    {
        #region Filds

        private readonly TokenType _token;
        private IExpressionNode _left;
        private IExpressionNode _right;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="BinaryExpressionNode" /> class.
        /// </summary>
        public BinaryExpressionNode([NotNull] IExpressionNode left, [NotNull] IExpressionNode right, TokenType token)
            : base(ExpressionNodeType.Binary)
        {
            Should.NotBeNull(left, "left");
            Should.NotBeNull(right, "right");
            _left = left;
            _right = right;
            _token = token;
        }

        #endregion

        #region Implementation of IBinaryExpressionNode

        /// <summary>
        ///     Gets the left operand of the binary operation.
        /// </summary>
        public IExpressionNode Left
        {
            get { return _left; }
        }

        /// <summary>
        ///     Gets the right operand of the binary operation.
        /// </summary>
        public IExpressionNode Right
        {
            get { return _right; }
        }

        /// <summary>
        ///     Gets the implementing token for the binary operation.
        /// </summary>
        public TokenType Token
        {
            get { return _token; }
        }

        #endregion

        #region Overrides of ExpressionNode

        /// <summary>
        ///     Calls the visitor on the expression.
        /// </summary>
        protected override void AcceptInternal(IExpressionVisitor visitor)
        {
            _left = AcceptWithCheck(visitor, Left, true);
            _right = AcceptWithCheck(visitor, Right, true);
        }

        /// <summary>
        ///     Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        ///     A new object that is a copy of this instance.
        /// </returns>
        protected override IExpressionNode CloneInternal()
        {
            return new BinaryExpressionNode(Left.Clone(), Right.Clone(), Token);
        }

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return string.Format("({0} {1} {2})", Left, Token.Value, Right);
        }

        #endregion
    }
}