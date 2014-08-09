#region Copyright
// ****************************************************************************
// <copyright file="UnaryExressionNode.cs">
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
    ///     Represents an expression that has a unary operator.
    /// </summary>
    public class UnaryExressionNode : ExpressionNode, IUnaryExressionNode
    {
        #region Fields

        private readonly TokenType _token;
        private IExpressionNode _operand;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="UnaryExressionNode" /> class.
        /// </summary>
        public UnaryExressionNode([NotNull] IExpressionNode operand, TokenType token)
            : base(ExpressionNodeType.Unary)
        {
            Should.NotBeNull(operand, "operand");
            _operand = operand;
            _token = token;
        }

        #endregion

        #region Implementation of IUnaryExressionNode

        /// <summary>
        ///     Gets the token of the unary operation.
        /// </summary>
        public TokenType Token
        {
            get { return _token; }
        }

        /// <summary>
        ///     Gets the operand of the unary operation.
        /// </summary>
        public IExpressionNode Operand
        {
            get { return _operand; }
        }

        #endregion

        #region Overrides of ExpressionNode

        /// <summary>
        ///     Calls the visitor on the expression.
        /// </summary>
        protected override void AcceptInternal(IExpressionVisitor visitor)
        {
            _operand = AcceptWithCheck(visitor, Operand, true);
        }

        /// <summary>
        ///     Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        ///     A new object that is a copy of this instance.
        /// </returns>
        protected override IExpressionNode CloneInternal()
        {
            return new UnaryExressionNode(Operand.Clone(), Token);
        }

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0}{1}", Token.Value, Operand);
        }

        #endregion
    }
}