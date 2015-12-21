#region Copyright

// ****************************************************************************
// <copyright file="BinaryExpressionNode.cs">
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
    public class BinaryExpressionNode : ExpressionNode, IBinaryExpressionNode
    {
        #region Filds

        private readonly TokenType _token;
        private IExpressionNode _left;
        private IExpressionNode _right;

        #endregion

        #region Constructors

        public BinaryExpressionNode([NotNull] IExpressionNode left, [NotNull] IExpressionNode right, TokenType token)
            : base(ExpressionNodeType.Binary)
        {
            Should.NotBeNull(left, nameof(left));
            Should.NotBeNull(right, nameof(right));
            _left = left;
            _right = right;
            _token = token;
        }

        #endregion

        #region Implementation of IBinaryExpressionNode

        public IExpressionNode Left
        {
            get { return _left; }
        }

        public IExpressionNode Right
        {
            get { return _right; }
        }

        public TokenType Token
        {
            get { return _token; }
        }

        #endregion

        #region Overrides of ExpressionNode

        protected override void AcceptInternal(IExpressionVisitor visitor)
        {
            _left = AcceptWithCheck(visitor, Left, true);
            _right = AcceptWithCheck(visitor, Right, true);
        }

        protected override IExpressionNode CloneInternal()
        {
            return new BinaryExpressionNode(Left.Clone(), Right.Clone(), Token);
        }

        public override string ToString()
        {
            return $"({Left} {Token.Value} {Right})";
        }

        #endregion
    }
}
