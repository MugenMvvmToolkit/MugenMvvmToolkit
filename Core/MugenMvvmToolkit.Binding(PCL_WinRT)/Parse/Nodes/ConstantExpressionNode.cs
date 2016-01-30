#region Copyright

// ****************************************************************************
// <copyright file="ConstantExpressionNode.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
using MugenMvvmToolkit.Binding.Interfaces.Parse;
using MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes;
using MugenMvvmToolkit.Binding.Models;

namespace MugenMvvmToolkit.Binding.Parse.Nodes
{
    public class ConstantExpressionNode : ExpressionNode, IConstantExpressionNode
    {
        #region Fields

        private readonly Type _type;
        private readonly object _value;

        #endregion

        #region Constructors

        public ConstantExpressionNode(object value, Type type = null)
            : base(ExpressionNodeType.Constant)
        {
            if (type == null)
                type = value == null ? typeof (object) : value.GetType();
            _value = value;
            _type = type;
        }

        #endregion

        #region Implementation of IConstantExpressionNode

        public object Value => _value;

        public Type Type => _type;

        #endregion

        #region Overrides of ExpressionNode

        protected override void AcceptInternal(IExpressionVisitor visitor)
        {
        }

        protected override IExpressionNode CloneInternal()
        {
            return new ConstantExpressionNode(Value, Type);
        }

        public override string ToString()
        {
            if (Value == null)
                return "null";
            return Value.ToString();
        }

        #endregion
    }
}
