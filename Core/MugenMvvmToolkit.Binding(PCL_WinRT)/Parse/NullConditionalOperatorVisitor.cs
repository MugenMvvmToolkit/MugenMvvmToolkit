#region Copyright

// ****************************************************************************
// <copyright file="NullConditionalOperatorVisitor.cs">
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
using System;
using System.Collections.Generic;
using MugenMvvmToolkit.Binding.Interfaces.Parse;
using MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Parse.Nodes;

namespace MugenMvvmToolkit.Binding.Parse
{
    internal sealed class NullConditionalOperatorVisitor : IExpressionVisitor
    {
        #region Fields

        public static readonly NullConditionalOperatorVisitor Instance;
        private static readonly ConstantExpressionNode TypeConstantNode;
        private static readonly MemberExpressionNode LambdaParameterNode;
        private static readonly string[] LambdaParameters;

        #endregion

        #region Constructors

        static NullConditionalOperatorVisitor()
        {
            Instance = new NullConditionalOperatorVisitor();
            TypeConstantNode = new ConstantExpressionNode(typeof(NullConditionalOperatorVisitor));
            LambdaParameterNode = new MemberExpressionNode(null, "x");
            LambdaParameters = new[] { LambdaParameterNode.Member };
        }

        private NullConditionalOperatorVisitor()
        {
        }

        #endregion

        #region Implementation of IExpressionVisitor

        public IExpressionNode Visit(IExpressionNode node)
        {
            List<IExpressionNode> chain = null;
            var binaryExpression = node as IBinaryExpressionNode;
            while (binaryExpression != null && binaryExpression.Token == TokenType.QuestionDot)
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

            node = null;
            for (int i = 1; i < chain.Count; i++)
            {
                node = new MethodCallExpressionNode(TypeConstantNode, "NullConditionalOperatorImpl",
                    new[] { node ?? chain[i - 1], new LambdaExpressionNode(UpdateTarget(chain[i], LambdaParameterNode), LambdaParameters) }, null);
            }
            return node;
        }

        #endregion

        #region Methods

        private static IExpressionNode UpdateTarget(IExpressionNode expression, IExpressionNode target)
        {
            var memberExpressionNode = expression as IMemberExpressionNode;
            if (memberExpressionNode != null)
                return new MemberExpressionNode(target, memberExpressionNode.Member);

            var methodCallExpressionNode = expression as IMethodCallExpressionNode;
            if (methodCallExpressionNode != null)
                return new MethodCallExpressionNode(target, methodCallExpressionNode.Method, methodCallExpressionNode.Arguments, methodCallExpressionNode.TypeArgs);
            return new IndexExpressionNode(target, ((IIndexExpressionNode)expression).Arguments);
        }

        public static TResult? NullConditionalOperatorImpl<T, TResult>(T item, Func<T, TResult> getResult)
            where TResult : struct
        {
            if (item == null)
                return null;
            return getResult(item);
        }

        public static TResult NullConditionalOperatorImpl<T, TResult>(T item, Func<T, TResult> getResult, object _ = null)
            where TResult : class
        {
            if (item == null)
                return null;
            return getResult(item);
        }

        #endregion
    }
}