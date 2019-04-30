using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Infrastructure.Internal
{
    public class ExpressionReflectionDelegateFactory : IReflectionDelegateFactory
    {
        #region Fields

        private static readonly ParameterExpression EmptyParameterExpression;
        private static readonly ConstantExpression NullConstantExpression;

        #endregion

        #region Constructors

        static ExpressionReflectionDelegateFactory()
        {
            EmptyParameterExpression = Expression.Parameter(typeof(object));
            NullConstantExpression = Expression.Constant(null, typeof(object));
        }

        [Preserve(Conditional = true)]
        public ExpressionReflectionDelegateFactory()
        {
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        public Func<object[], object> TryGetActivatorDelegate(IReflectionDelegateProvider provider, ConstructorInfo constructor)
        {
            var expressions = GetParametersExpression(constructor, out var parameterExpression);
            var newExpression = ConvertIfNeed(Expression.New(constructor, expressions), typeof(object), false);
            return Expression.Lambda<Func<object?[], object>>(newExpression, parameterExpression).Compile();
        }

        public Func<object, object[], object> TryGetMethodDelegate(IReflectionDelegateProvider provider, MethodInfo method)
        {
            var isVoid = method.ReturnType.EqualsEx(typeof(void));
            var expressions = GetParametersExpression(method, out var parameterExpression);
            Expression callExpression;
            if (method.IsStatic)
            {
                callExpression = Expression.Call(null, method, expressions);
                if (isVoid)
                {
                    return Expression
                        .Lambda<Func<object?, object?[], object?>>(
                            Expression.Block(callExpression, NullConstantExpression), EmptyParameterExpression,
                            parameterExpression)
                        .Compile();
                }

                callExpression = ConvertIfNeed(callExpression, typeof(object), false);
                return Expression
                    .Lambda<Func<object?, object?[], object?>>(callExpression, EmptyParameterExpression, parameterExpression)
                    .Compile();
            }

            var declaringType = method.DeclaringType;
            var targetExp = Expression.Parameter(typeof(object), "target");
            callExpression = Expression.Call(ConvertIfNeed(targetExp, declaringType, false), method, expressions);
            if (isVoid)
            {
                return Expression
                    .Lambda<Func<object?, object?[], object?>>(Expression.Block(callExpression, NullConstantExpression),
                        targetExp, parameterExpression)
                    .Compile();
            }

            callExpression = ConvertIfNeed(callExpression, typeof(object), false);
            return Expression
                .Lambda<Func<object?, object?[], object?>>(callExpression, targetExp, parameterExpression)
                .Compile();
        }

        public Delegate TryGetMethodDelegate(IReflectionDelegateProvider provider, Type delegateType, MethodInfo method)
        {
            var delegateMethod = delegateType.GetMethodUnified(nameof(Action.Invoke), MemberFlags.InstanceOnly);
            if (delegateMethod == null)
                throw new ArgumentException(string.Empty, nameof(delegateType));

            var delegateParams = delegateMethod.GetParameters().ToList();
            var methodParams = method.GetParameters();
            var expressions = new List<Expression>();
            var parameters = new List<ParameterExpression>();
            if (!method.IsStatic)
            {
                var thisParam = Expression.Parameter(delegateParams[0].ParameterType, "@this");
                parameters.Add(thisParam);
                expressions.Add(ConvertIfNeed(thisParam, method.DeclaringType, false));
                delegateParams.RemoveAt(0);
            }

            Should.BeValid("delegateType", delegateParams.Count == methodParams.Length);
            for (var i = 0; i < methodParams.Length; i++)
            {
                var parameter = Expression.Parameter(delegateParams[i].ParameterType, i.ToString());
                parameters.Add(parameter);
                expressions.Add(ConvertIfNeed(parameter, methodParams[i].ParameterType, false));
            }

            Expression callExpression;
            if (method.IsStatic)
                callExpression = Expression.Call(null, method, expressions.ToArray());
            else
            {
                var @this = expressions[0];
                expressions.RemoveAt(0);
                callExpression = Expression.Call(@this, method, expressions.ToArray());
            }

            if (delegateMethod.ReturnType != typeof(void))
                callExpression = ConvertIfNeed(callExpression, delegateMethod.ReturnType, false);
            var lambdaExpression = Expression.Lambda(delegateType, callExpression, parameters);
            return lambdaExpression.Compile();
        }

        public Func<object, TType> TryGetMemberGetter<TType>(IReflectionDelegateProvider provider, MemberInfo member)
        {
            var target = Expression.Parameter(typeof(object), "instance");
            MemberExpression accessExp;
            if (member.IsStatic())
                accessExp = Expression.MakeMemberAccess(null, member);
            else
            {
                var declaringType = member.DeclaringType;
                accessExp = Expression.MakeMemberAccess(ConvertIfNeed(target, declaringType, false), member);
            }

            return Expression
                .Lambda<Func<object?, TType>>(ConvertIfNeed(accessExp, typeof(TType), false), target)
                .Compile();
        }

        public Action<object, TType> TryGetMemberSetter<TType>(IReflectionDelegateProvider provider, MemberInfo member)
        {
            var declaringType = member.DeclaringType;
            var fieldInfo = member as FieldInfo;
            if (declaringType.IsValueTypeUnified())
            {
                if (fieldInfo == null)
                {
                    var propertyInfo = (PropertyInfo) member;
                    return propertyInfo.SetValue<TType>;
                }

                return fieldInfo.SetValue<TType>;
            }

            Expression expression;
            var targetParameter = Expression.Parameter(typeof(object), "instance");
            var valueParameter = Expression.Parameter(typeof(TType), "value");
            var target = ConvertIfNeed(targetParameter, declaringType, false);
            if (fieldInfo == null)
            {
                var propertyInfo = member as PropertyInfo;
                MethodInfo? setMethod = null;
                if (propertyInfo != null)
                    setMethod = propertyInfo.GetSetMethodUnified(true);
                Should.MethodBeSupported(propertyInfo != null && setMethod != null, MessageConstants.ShouldSupportOnlyFieldsReadonlyFields);
                var valueExpression = ConvertIfNeed(valueParameter, propertyInfo.PropertyType, false);
                expression = Expression.Call(setMethod.IsStatic ? null : ConvertIfNeed(target, declaringType, false), setMethod, valueExpression);
            }
            else
            {
                expression = Expression.Field(fieldInfo.IsStatic ? null : ConvertIfNeed(target, declaringType, false), fieldInfo);
                expression = Expression.Assign(expression, ConvertIfNeed(valueParameter, fieldInfo.FieldType, false));
            }

            return Expression
                .Lambda<Action<object, TType>>(expression, targetParameter, valueParameter)
                .Compile();
        }

        #endregion

        #region Methods

        private static Expression[] GetParametersExpression(MethodBase methodBase, out ParameterExpression parameterExpression)
        {
            var paramsInfo = methodBase.GetParameters();
            //create a single param of type object[]
            parameterExpression = Expression.Parameter(typeof(object[]), "args");
            var argsExp = new Expression[paramsInfo.Length];

            //pick each arg from the params array
            //and create a typed expression of them
            for (var i = 0; i < paramsInfo.Length; i++)
            {
                Expression index = Expression.Constant(i);
                var paramType = paramsInfo[i].ParameterType;
                Expression paramAccessorExp = Expression.ArrayIndex(parameterExpression, index);
                var paramCastExp = ConvertIfNeed(paramAccessorExp, paramType, false);
                argsExp[i] = paramCastExp;
            }

            return argsExp;
        }

        private static Expression ConvertIfNeed(Expression? expression, Type type, bool exactly)
        {
            if (expression == null)
                return null!;
            if (type.EqualsEx(typeof(void)) || type.EqualsEx(expression.Type))
                return expression;
            if (!exactly && !expression.Type.IsValueTypeUnified() && !type.IsValueTypeUnified() && type.IsAssignableFromUnified(expression.Type))
                return expression;
            return Expression.Convert(expression, type);
        }

        #endregion
    }
}