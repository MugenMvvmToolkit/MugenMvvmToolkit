#region Copyright
// ****************************************************************************
// <copyright file="ExpressionNodeType.cs">
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
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Models
{
    /// <summary>
    ///     Represents the type of node expression.
    /// </summary>
    public class ExpressionNodeType : StringConstantBase<ExpressionNodeType>
    {
        #region Fields

        /// <summary>
        ///     Represents a binary expression.
        /// </summary>
        public static readonly ExpressionNodeType Binary = new ExpressionNodeType("Binary");

        /// <summary>
        ///     Represents a condition expression.
        /// </summary>
        public static readonly ExpressionNodeType Condition = new ExpressionNodeType("Condition");

        /// <summary>
        ///     Represents a constant value expression.
        /// </summary>
        public static readonly ExpressionNodeType Constant = new ExpressionNodeType("Constant");

        /// <summary>
        ///     Represents an indexer expression.
        /// </summary>
        public static readonly ExpressionNodeType Index = new ExpressionNodeType("Index");

        /// <summary>
        ///     Represents a member expression.
        /// </summary>
        public static readonly ExpressionNodeType Member = new ExpressionNodeType("Member");

        /// <summary>
        ///     Represents a method call expression.
        /// </summary>
        public static readonly ExpressionNodeType MethodCall = new ExpressionNodeType("MethodCall");

        /// <summary>
        ///     Represents an unary expression.
        /// </summary>
        public static readonly ExpressionNodeType Unary = new ExpressionNodeType("Unary");

        /// <summary>
        ///     Represents an binding member expression.
        /// </summary>
        public static readonly ExpressionNodeType BindingMember = new ExpressionNodeType("BindingMember");

        /// <summary>
        ///     Represents a relative source expression.
        /// </summary>
        public static readonly ExpressionNodeType RelativeSource = new ExpressionNodeType("RelativeSource");

        /// <summary>
        ///     Represents a lambda expression.
        /// </summary>
        public static readonly ExpressionNodeType Lambda = new ExpressionNodeType("Lambda");

        /// <summary>
        ///     Represents a dynamic binding member.
        /// </summary>
        public static readonly ExpressionNodeType DynamicMember = new ExpressionNodeType("DynamicMember");

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExpressionNodeType" /> class.
        /// </summary>
        public ExpressionNodeType(string id)
            : base(id)
        {
        }

        #endregion
    }
}