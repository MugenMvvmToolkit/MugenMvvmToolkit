﻿using MugenMvvm.Enums;

namespace MugenMvvm.Binding.Enums
{
    public class ExpressionNodeType : EnumBase<ExpressionNodeType, int>
    {
        #region Fields

        public static readonly ExpressionNodeType Binary = new ExpressionNodeType(1);
        public static readonly ExpressionNodeType Condition = new ExpressionNodeType(2);
        public static readonly ExpressionNodeType Constant = new ExpressionNodeType(3);
        public static readonly ExpressionNodeType Index = new ExpressionNodeType(4);
        public static readonly ExpressionNodeType Member = new ExpressionNodeType(5);
        public static readonly ExpressionNodeType MethodCall = new ExpressionNodeType(6);
        public static readonly ExpressionNodeType Unary = new ExpressionNodeType(7);
        public static readonly ExpressionNodeType Lambda = new ExpressionNodeType(8);

        #endregion

        #region Constructors

        protected ExpressionNodeType()
        {
        }

        public ExpressionNodeType(int value) : base(value)
        {
        }

        public ExpressionNodeType(int value, string displayName) : base(value, displayName)
        {
        }

        #endregion
    }
}