#region Copyright

// ****************************************************************************
// <copyright file="UnaryExressionNode.cs">
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
    public class UnaryExressionNode : ExpressionNode, IUnaryExressionNode
    {
        #region Fields

        private readonly TokenType _token;
        private IExpressionNode _operand;

        #endregion

        #region Constructors

        public UnaryExressionNode([NotNull] IExpressionNode operand, TokenType token)
            : base(ExpressionNodeType.Unary)
        {
            Should.NotBeNull(operand, nameof(operand));
            _operand = operand;
            _token = token;
        }

        #endregion

        #region Implementation of IUnaryExressionNode

        public TokenType Token
        {
            get { return _token; }
        }

        public IExpressionNode Operand
        {
            get { return _operand; }
        }

        #endregion

        #region Overrides of ExpressionNode

        protected override void AcceptInternal(IExpressionVisitor visitor)
        {
            _operand = AcceptWithCheck(visitor, Operand, true);
        }

        protected override IExpressionNode CloneInternal()
        {
            return new UnaryExressionNode(Operand.Clone(), Token);
        }

        public override string ToString()
        {
            return $"{Token.Value}{Operand}";
        }

        #endregion
    }
}
