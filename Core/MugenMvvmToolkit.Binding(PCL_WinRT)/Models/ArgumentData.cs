#region Copyright

// ****************************************************************************
// <copyright file="ArgumentData.cs">
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
using System.Linq.Expressions;
using MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes;

namespace MugenMvvmToolkit.Binding.Models
{
    internal sealed class ArgumentData
    {
        #region Fields

        private readonly IExpressionNode _node;
        private readonly bool _isTypeAccess;
        private Expression _expression;
        private Type _type;
        
        #endregion

        #region Constuctors

        public ArgumentData(IExpressionNode node, Expression expression, Type type, bool isTypeAccess)
        {
            Should.NotBeNull(node, "node");
            _node = node;
            _expression = expression;
            if (type == null && expression != null)
                type = expression.Type;
            _type = type;
            _isTypeAccess = isTypeAccess;
        }

        #endregion

        #region Properties

        public bool IsLambda
        {
            get { return Node.NodeType == ExpressionNodeType.Lambda; }
        }

        public bool IsTypeAccess
        {
            get { return _isTypeAccess; }
        }

        public Type Type
        {
            get { return _type; }
        }

        public Expression Expression
        {
            get { return _expression; }
        }

        public IExpressionNode Node
        {
            get { return _node; }
        }

        #endregion

        #region Methods

        public void UpdateExpression(Expression expression)
        {
            if (_type == null)
                _type = expression.Type;
            _expression = expression;
        }

        #endregion
    }
}