#region Copyright

// ****************************************************************************
// <copyright file="ExpressionNodeType.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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
    public class ExpressionNodeType : StringConstantBase<ExpressionNodeType>
    {
        #region Fields

        public static readonly ExpressionNodeType Binary;

        public static readonly ExpressionNodeType Condition;

        public static readonly ExpressionNodeType Constant;

        public static readonly ExpressionNodeType Index;

        public static readonly ExpressionNodeType Member;

        public static readonly ExpressionNodeType MethodCall;

        public static readonly ExpressionNodeType Unary;

        public static readonly ExpressionNodeType BindingMember;

        public static readonly ExpressionNodeType RelativeSource;

        public static readonly ExpressionNodeType Lambda;

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

        public ExpressionNodeType(string id)
            : base(id)
        {
        }

        #endregion
    }
}
