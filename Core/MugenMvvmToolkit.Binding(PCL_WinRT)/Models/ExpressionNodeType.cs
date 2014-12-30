#region Copyright

// ****************************************************************************
// <copyright file="ExpressionNodeType.cs">
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
        public static readonly ExpressionNodeType Binary;

        /// <summary>
        ///     Represents a condition expression.
        /// </summary>
        public static readonly ExpressionNodeType Condition;

        /// <summary>
        ///     Represents a constant value expression.
        /// </summary>
        public static readonly ExpressionNodeType Constant;

        /// <summary>
        ///     Represents an indexer expression.
        /// </summary>
        public static readonly ExpressionNodeType Index;

        /// <summary>
        ///     Represents a member expression.
        /// </summary>
        public static readonly ExpressionNodeType Member;

        /// <summary>
        ///     Represents a method call expression.
        /// </summary>
        public static readonly ExpressionNodeType MethodCall;

        /// <summary>
        ///     Represents an unary expression.
        /// </summary>
        public static readonly ExpressionNodeType Unary;

        /// <summary>
        ///     Represents an binding member expression.
        /// </summary>
        public static readonly ExpressionNodeType BindingMember;

        /// <summary>
        ///     Represents a relative source expression.
        /// </summary>
        public static readonly ExpressionNodeType RelativeSource;

        /// <summary>
        ///     Represents a lambda expression.
        /// </summary>
        public static readonly ExpressionNodeType Lambda;

        /// <summary>
        ///     Represents a dynamic binding member.
        /// </summary>
        public static readonly ExpressionNodeType DynamicMember;

        #endregion

        #region Constructors

        static ExpressionNodeType()
        {
            Binary = new ExpressionNodeType("Binary");
            Condition = new ExpressionNodeType("Condition");
            Constant = new ExpressionNodeType("Constant");
            Index = new ExpressionNodeType("Index");
            Member = new ExpressionNodeType("Member");
            MethodCall = new ExpressionNodeType("MethodCall");
            Unary = new ExpressionNodeType("Unary");
            BindingMember = new ExpressionNodeType("BindingMember");
            RelativeSource = new ExpressionNodeType("RelativeSource");
            Lambda = new ExpressionNodeType("Lambda");
            DynamicMember = new ExpressionNodeType("DynamicMember");
        }

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