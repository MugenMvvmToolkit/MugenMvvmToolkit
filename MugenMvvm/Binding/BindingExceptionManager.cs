using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Extensions;
using static MugenMvvm.Binding.Constants.BindingMessageConstant;

namespace MugenMvvm.Binding
{
    internal static class BindingExceptionManager
    {
        #region Methods

        [DoesNotReturn]
        public static void ThrowAmbiguousMatchFound()
        {
            throw new AmbiguousMatchException();
        }

        [DoesNotReturn]
        public static void ThrowBindingMemberMustBeWritable(IMemberInfo member)
        {
            throw new InvalidOperationException(BindingMemberMustBeWritableFormat4.Format(member.Name, member.Type, member.MemberType, member.UnderlyingMember));
        }

        [DoesNotReturn]
        public static void ThrowBindingMemberMustBeReadable(IMemberInfo member)
        {
            throw new InvalidOperationException(BindingMemberMustBeReadableFormat4.Format(member.Name, member.Type, member.MemberType, member.UnderlyingMember));
        }

        [DoesNotReturn]
        public static void ThrowInvalidBindingMember(Type sourceType, string path)
        {
            throw new InvalidOperationException(string.Format(InvalidBindingMemberFormat2, path, sourceType));
        }

        [DoesNotReturn]
        public static void ThrowUnexpectedExpressionNode(IExpressionNode node, Type type)
        {
            throw new InvalidOperationException(UnexpectedExpressionTyperFormat3.Format(node.ExpressionType, type.Name, node));
        }

        [DoesNotReturn]
        public static void ThrowCannotParseExpression<T>(in T expression, string? hint = null)
        {
            throw new InvalidOperationException(CannotParseExpressionFormat2.Format(expression!.ToString(), hint));
        }

        [DoesNotReturn]
        public static void ThrowCannotCompileExpression(IExpressionNode expression, string? hint = null)
        {
            throw new InvalidOperationException(CannotCompileExpressionFormat2.Format(expression, hint));
        }

        [DoesNotReturn]
        public static void ThrowCannotUseExpressionExpected(IExpressionNode expression, Type expectedType)
        {
            throw new InvalidOperationException(CannotUseExpressionExpected.Format(expression, expectedType));
        }

        [DoesNotReturn]
        public static void ThrowDuplicateLambdaParameter(string parameterName)
        {
            throw new InvalidOperationException(DuplicateLambdaParameterFormat1.Format(parameterName));
        }

        [DoesNotReturn]
        public static void ThrowExpressionNodeCannotBeNull(Type ownerType)
        {
            throw new InvalidOperationException(ExpressionNodeCannotBeNullFormat1.Format(ownerType));
        }

        [DoesNotReturn]
        public static void ThrowCannotResolveType(string typeName)
        {
            throw new InvalidOperationException(string.Format(CannotResolveTypeFormat1, typeName));
        }

        [DoesNotReturn]
        public static void ThrowCannotResolveResource(string resource)
        {
            throw new InvalidOperationException(string.Format(CannotResolveResourceFormat1, resource));
        }

        [DoesNotReturn]
        public static void ThrowCannotParseBindingParameter(string parameterName, object expectedValue, object currentValue)
        {
            throw new InvalidOperationException(string.Format(CannotParseBindingParameterFormat3, parameterName, expectedValue, currentValue));
        }

        [DoesNotReturn]
        public static void ThrowCannotConvertType(object? value, Type type)
        {
            throw new InvalidOperationException(string.Format(CannotConvertTypeFormat2, value, type));
        }

        [DoesNotReturn]
        public static void ThrowCannotUseExpressionClosure(object expression)
        {
            throw new InvalidOperationException(string.Format(CannotUseExpressionClosureFormat1, expression));
        }

        #endregion
    }
}