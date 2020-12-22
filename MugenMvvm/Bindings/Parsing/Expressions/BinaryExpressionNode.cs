using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Parsing.Expressions
{
    public sealed class BinaryExpressionNode : ExpressionNodeBase, IBinaryExpressionNode
    {
        #region Constructors

        public BinaryExpressionNode(BinaryTokenType token, IExpressionNode left, IExpressionNode right, IDictionary<string, object?>? metadata = null) : base(metadata)
        {
            Should.NotBeNull(token, nameof(token));
            Should.NotBeNull(left, nameof(left));
            Should.NotBeNull(right, nameof(right));
            Left = left;
            Right = right;
            Token = token;
        }

        #endregion

        #region Properties

        public override ExpressionNodeType ExpressionType => ExpressionNodeType.Binary;

        public IExpressionNode Left { get; }

        public IExpressionNode Right { get; }

        public BinaryTokenType Token { get; }

        #endregion

        #region Methods

        protected override IExpressionNode Visit(IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata)
        {
            var changed = false;
            var left = VisitWithCheck(visitor, Left, true, ref changed, metadata);
            var right = VisitWithCheck(visitor, Right, true, ref changed, metadata);
            if (changed)
                return new BinaryExpressionNode(Token, left, right, MetadataRaw);
            return this;
        }

        public override string ToString() => $"({Left} {Token.Value} {Right})";

        #endregion
    }
}