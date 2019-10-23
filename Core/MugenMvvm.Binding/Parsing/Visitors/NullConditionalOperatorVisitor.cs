using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;

namespace MugenMvvm.Binding.Parsing.Visitors
{
    public sealed class NullConditionalOperatorVisitor : IExpressionVisitor
    {
        #region Fields

        public static readonly NullConditionalOperatorVisitor Instance = new NullConditionalOperatorVisitor();

        private static readonly IConstantExpressionNode TypeConstantNode = ConstantExpressionNode.Get<NullConditionalOperatorVisitor>();
        private static readonly IParameterExpression LambdaParameterNode = new ParameterExpression("x", 0);
        private static readonly IParameterExpression[] LambdaParameters = { LambdaParameterNode };

        #endregion

        #region Constructors

        private NullConditionalOperatorVisitor()
        {
        }

        #endregion

        #region Properties

        public bool IsPostOrder => false;

        #endregion

        #region Implementation of interfaces

        public IExpressionNode? Visit(IExpressionNode node)
        {
            List<IExpressionNode>? chain = null;
            var binaryExpression = node as IBinaryExpressionNode;
            while (binaryExpression != null && binaryExpression.Token == BinaryTokenType.NullConditionalMemberAccess)
            {
                if (chain == null)
                    chain = new List<IExpressionNode>();

                chain.Insert(0, binaryExpression.Right);
                var left = binaryExpression.Left;
                binaryExpression = left as IBinaryExpressionNode;
                if (binaryExpression == null)
                    chain.Insert(0, left);
            }

            if (chain == null)
                return node;

            node = null!;
            for (var i = 1; i < chain.Count; i++)
            {
                var body = ((IHasTargetExpressionNode<IExpressionNode>)chain[i]).UpdateTarget(LambdaParameterNode);
                node = new MethodCallExpressionNode(TypeConstantNode, nameof(NullConditionalOperatorImpl), new[] { node ?? chain[i - 1], new LambdaExpressionNode(body, LambdaParameters) });
            }

            return node;
        }

        #endregion

        #region Methods

        [Preserve(Conditional = true)]
        public static TResult? NullConditionalOperatorImpl<T, TResult>(T item, Func<T, TResult> getResult)
            where TResult : struct
        {
            if (Default.IsNullable<T>() && item == null)
                return null;
            return getResult(item);
        }

        [Preserve(Conditional = true)]
        public static TResult NullConditionalOperatorImpl<T, TResult>(T item, Func<T, TResult> getResult, object? _ = null)
            where TResult : class
        {
            if (Default.IsNullable<T>() && item == null)
                return null!;
            return getResult(item);
        }

        #endregion
    }
}