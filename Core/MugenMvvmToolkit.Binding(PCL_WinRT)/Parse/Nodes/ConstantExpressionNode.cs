#region Copyright
// ****************************************************************************
// <copyright file="ConstantExpressionNode.cs">
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
using System;
using MugenMvvmToolkit.Binding.Interfaces.Parse;
using MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes;
using MugenMvvmToolkit.Binding.Models;

namespace MugenMvvmToolkit.Binding.Parse.Nodes
{
    /// <summary>
    ///     Represents an expression that has a constant value.
    /// </summary>
    public class ConstantExpressionNode : ExpressionNode, IConstantExpressionNode
    {
        #region Fields

        private readonly Type _type;
        private readonly object _value;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConstantExpressionNode" /> class.
        /// </summary>
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

        /// <summary>
        ///     Gets the value of the constant expression.
        /// </summary>
        public object Value
        {
            get { return _value; }
        }

        /// <summary>
        ///     Gets the type of the value.
        /// </summary>
        public Type Type
        {
            get { return _type; }
        }

        #endregion

        #region Overrides of ExpressionNode

        /// <summary>
        ///     Calls the visitor on the expression.
        /// </summary>
        protected override void AcceptInternal(IExpressionVisitor visitor)
        {
        }

        /// <summary>
        ///     Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        ///     A new object that is a copy of this instance.
        /// </returns>
        protected override IExpressionNode CloneInternal()
        {
            return new ConstantExpressionNode(Value, Type);
        }

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            if (Value == null)
                return "null";
            return Value.ToString();
        }

        #endregion
    }
}