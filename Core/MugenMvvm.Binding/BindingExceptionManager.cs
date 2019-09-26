using System;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using static MugenMvvm.Binding.Constants.BindingMessageConstants;

namespace MugenMvvm.Binding
{
    internal static class BindingExceptionManager
    {
        #region Methods

        public static void ThrowBindingMemberMustBeWritable(IBindingMemberInfo member)
        {
            throw new InvalidOperationException(BindingMemberMustBeWritableFormat4.Format(member.Name, member.Type, member.MemberType, member.Member));
        }

        public static void ThrowBindingMemberMustBeReadable(IBindingMemberInfo member)
        {
            throw new InvalidOperationException(BindingMemberMustBeReadableFormat4.Format(member.Name, member.Type, member.MemberType, member.Member));
        }

        public static void ThrowInvalidBindingMember(Type sourceType, string path)
        {
            throw new InvalidOperationException(string.Format(InvalidBindingMemberFormat2, path, sourceType));
        }

        public static void UnexpectedExpressionNode(IExpressionNode node, Type type)
        {
            throw new InvalidOperationException(UnexpectedExpressionTyperFormat3.Format(node.NodeType, type.Name, node));
        }

        public static void CannotParseExpression<T>(in T expression)
        {
            throw new InvalidOperationException(CannotParseExpressionFormat1.Format(expression));
        }

        public static void DuplicateLambdaParameter(string parameterName)
        {
            throw new InvalidOperationException(DuplicateLambdaParameterFormat1.Format(parameterName));
        }

        public static void ExpressionNodeCannotBeNull(Type ownerType)
        {
            throw new InvalidOperationException(ExpressionNodeCannotBeNullFormat1.Format(ownerType));
        }

        #endregion
    }
}