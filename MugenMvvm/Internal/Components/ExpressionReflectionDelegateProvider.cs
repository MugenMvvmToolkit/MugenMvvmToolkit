using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Internal.Components
{
    public sealed class ExpressionReflectionDelegateProvider : IReflectionDelegateProviderComponent, IActivatorReflectionDelegateProviderComponent,
        IMemberReflectionDelegateProviderComponent, IMethodReflectionDelegateProviderComponent, IHasPriority
    {
        #region Fields

        public static readonly ParameterExpression TargetParameter = MugenExtensions.GetParameterExpression<object>();
        private static readonly ParameterExpression[] TargetArgsParameters = {TargetParameter, MugenExtensions.GetParameterExpression<object[]>()};
        private static readonly ParameterExpression[] ArgsParameters = MugenExtensions.GetParametersExpression<object[]>();
        private static readonly Dictionary<KeyValuePair<Type, MethodInfo>, MethodInfo?> CacheMethodDelegates = new Dictionary<KeyValuePair<Type, MethodInfo>, MethodInfo?>(17, InternalComparer.TypeMethod);

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ExpressionReflectionDelegateProvider()
        {
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = InternalComponentPriority.DelegateProvider;

        #endregion

        #region Implementation of interfaces

        Func<object?[], object>? IActivatorReflectionDelegateProviderComponent.TryGetActivator(IReflectionManager reflectionManager, ConstructorInfo constructor) => GetActivator(constructor);

        Delegate? IActivatorReflectionDelegateProviderComponent.TryGetActivator(IReflectionManager reflectionManager, ConstructorInfo constructor, Type delegateType) => TryGetActivator(constructor, delegateType);

        Delegate? IMemberReflectionDelegateProviderComponent.TryGetMemberGetter(IReflectionManager reflectionManager, MemberInfo member, Type delegateType) => TryGetMemberGetter(member, delegateType);

        Delegate? IMemberReflectionDelegateProviderComponent.TryGetMemberSetter(IReflectionManager reflectionManager, MemberInfo member, Type delegateType) => TryGetMemberSetter(member, delegateType);

        Func<object?, object?[], object?>? IMethodReflectionDelegateProviderComponent.TryGetMethodInvoker(IReflectionManager reflectionManager, MethodInfo method) => GetMethodInvoker(method);

        Delegate? IMethodReflectionDelegateProviderComponent.TryGetMethodInvoker(IReflectionManager reflectionManager, MethodInfo method, Type delegateType) => TryGetMethodInvoker(method, delegateType);

        bool IReflectionDelegateProviderComponent.CanCreateDelegate(IReflectionManager reflectionManager, Type delegateType, MethodInfo method) => TryGetMethodDelegate(delegateType, method) != null;

        Delegate? IReflectionDelegateProviderComponent.TryCreateDelegate(IReflectionManager reflectionManager, Type delegateType, object? target, MethodInfo method)
        {
            method = TryGetMethodDelegate(delegateType, method)!;
            if (method == null)
                return null;

            if (target == null)
                return method.CreateDelegate(delegateType);
            return method.CreateDelegate(delegateType, target);
        }

        #endregion

        #region Methods

        private static MethodInfo? TryGetMethodDelegate(Type delegateType, MethodInfo method)
        {
            var key = new KeyValuePair<Type, MethodInfo>(delegateType, method);
            MethodInfo? info;
            lock (CacheMethodDelegates)
            {
                if (!CacheMethodDelegates.TryGetValue(key, out info))
                {
                    info = TryGetMethodDelegateInternal(delegateType, method);
                    CacheMethodDelegates[key] = info;
                }
            }

            return info;
        }

        public static Func<object?[], object> GetActivator(ConstructorInfo constructor) =>
            Expression
                .Lambda<Func<object?[], object>>(Expression.New(constructor, GetParametersExpression(constructor)).ConvertIfNeed(typeof(object), false), ArgsParameters)
                .CompileEx();

        public static Delegate? TryGetActivator(ConstructorInfo constructor, Type delegateType)
        {
            var delegateMethod = delegateType.GetMethodOrThrow(nameof(Action.Invoke), BindingFlagsEx.InstanceOnly);
            var methodParameters = constructor.GetParameters();
            var delegateParameters = delegateMethod.GetParameters();
            if (methodParameters.Length != delegateParameters.Length)
                return null;

            var parameters = new ParameterExpression[methodParameters.Length];
            var args = new Expression[methodParameters.Length];
            for (var i = 0; i < methodParameters.Length; i++)
            {
                parameters[i] = Expression.Parameter(delegateParameters[i].ParameterType);
                args[i] = parameters[i].ConvertIfNeed(methodParameters[i].ParameterType, false);
            }

            return Expression.Lambda(delegateType, Expression.New(constructor, args).ConvertIfNeed(delegateMethod.ReturnType, false), parameters).CompileEx();
        }

        public static Func<object?, object?[], object?> GetMethodInvoker(MethodInfo method)
        {
            var expressions = GetParametersExpression(method);
            if (method.IsStatic)
            {
                return Expression
                    .Lambda<Func<object?, object?[], object?>>(Expression
                        .Call(null, method, expressions).ConvertIfNeed(typeof(object), false), TargetArgsParameters)
                    .CompileEx();
            }

            return Expression
                .Lambda<Func<object?, object?[], object?>>(Expression
                    .Call(TargetParameter.ConvertIfNeed(method.DeclaringType, false), method, expressions).ConvertIfNeed(typeof(object), false), TargetArgsParameters)
                .CompileEx();
        }

        public static Delegate? TryGetMethodInvoker(MethodInfo method, Type delegateType)
        {
            var delegateMethod = delegateType.GetMethodOrThrow(nameof(Action.Invoke), BindingFlagsEx.InstanceOnly);
            var methodParameters = method.GetParameters();
            var delegateParameters = delegateMethod.GetParameters();
            Expression callExpression;
            ParameterExpression[] parameters;
            if (method.IsStatic)
            {
                if (methodParameters.Length != delegateParameters.Length)
                    return null;

                parameters = new ParameterExpression[methodParameters.Length];
                var args = new Expression[methodParameters.Length];
                for (var i = 0; i < methodParameters.Length; i++)
                {
                    parameters[i] = Expression.Parameter(delegateParameters[i].ParameterType);
                    args[i] = parameters[i].ConvertIfNeed(methodParameters[i].ParameterType, false);
                }

                callExpression = Expression.Call(null, method, args);
            }
            else
            {
                if (methodParameters.Length != delegateParameters.Length - 1)
                    return null;

                parameters = new ParameterExpression[methodParameters.Length + 1];
                parameters[0] = Expression.Parameter(delegateParameters[0].ParameterType);
                var args = new Expression[methodParameters.Length];
                for (var i = 1; i < parameters.Length; i++)
                {
                    parameters[i] = Expression.Parameter(delegateParameters[i].ParameterType);
                    args[i - 1] = parameters[i].ConvertIfNeed(methodParameters[i - 1].ParameterType, false);
                }

                callExpression = Expression.Call(parameters[0].ConvertIfNeed(method.DeclaringType, false), method, args);
            }

            return Expression.Lambda(delegateType, callExpression.ConvertIfNeed(delegateMethod.ReturnType, false), parameters).CompileEx();
        }

        public static Delegate? TryGetMemberGetter(MemberInfo member, Type delegateType)
        {
            var delegateMethod = delegateType.GetMethodOrThrow(nameof(Action.Invoke), BindingFlagsEx.InstanceOnly);
            var delegateParameters = delegateMethod.GetParameters();
            if (delegateMethod.ReturnType == typeof(void))
                return null;

            LambdaExpression expression;
            if (member.IsStatic())
            {
                if (delegateParameters.Length == 0)
                {
                    expression = Expression.Lambda(delegateType, Expression
                        .MakeMemberAccess(null, member)
                        .ConvertIfNeed(delegateMethod.ReturnType, false));
                }
                else if (delegateParameters.Length == 1) //ignoring first parameter
                {
                    var parameter = Expression.Parameter(delegateParameters[0].ParameterType);
                    expression = Expression.Lambda(delegateType, Expression
                        .MakeMemberAccess(null, member)
                        .ConvertIfNeed(delegateMethod.ReturnType, false), parameter);
                }
                else
                    return null;
            }
            else
            {
                if (delegateParameters.Length != 1)
                    return null;
                var targetParameter = Expression.Parameter(delegateParameters[0].ParameterType);
                expression = Expression.Lambda(delegateType, Expression
                    .MakeMemberAccess(targetParameter.ConvertIfNeed(member.DeclaringType, false), member)
                    .ConvertIfNeed(delegateMethod.ReturnType, false), targetParameter);
            }

            return expression.CompileEx();
        }

        public static Delegate? TryGetMemberSetter(MemberInfo member, Type delegateType)
        {
            var delegateMethod = delegateType.GetMethodOrThrow(nameof(Action.Invoke), BindingFlagsEx.InstanceOnly);
            var delegateParameters = delegateMethod.GetParameters();

            Expression expression;
            ParameterExpression? targetParameter;
            if (member.IsStatic())
            {
                if (delegateParameters.Length == 1)
                    targetParameter = null;
                else if (delegateParameters.Length == 2) //ignoring first parameter
                    targetParameter = Expression.Parameter(delegateParameters[0].ParameterType);
                else
                    return null;
            }
            else
            {
                if (delegateParameters.Length != 2)
                    return null;
                targetParameter = Expression.Parameter(delegateParameters[0].ParameterType);
            }

            var valueParameter = Expression.Parameter(delegateParameters[targetParameter == null ? 0 : 1].ParameterType);
            if (member is FieldInfo fieldInfo)
            {
                expression = Expression.Assign(Expression.Field(fieldInfo.IsStatic ? null : targetParameter.ConvertIfNeed(member.DeclaringType, false), fieldInfo),
                    valueParameter.ConvertIfNeed(fieldInfo.FieldType, false));
            }
            else
            {
                var propertyInfo = member as PropertyInfo;
                Should.MethodBeSupported(propertyInfo != null, MessageConstant.ShouldSupportOnlyFieldsReadonlyFields);
                expression = Expression.Assign(Expression.Property(propertyInfo.IsStatic() ? null : targetParameter.ConvertIfNeed(member.DeclaringType, false), propertyInfo),
                    valueParameter.ConvertIfNeed(propertyInfo.PropertyType, false));
            }

            if (targetParameter == null)
            {
                return Expression
                    .Lambda(delegateType, expression, valueParameter)
                    .CompileEx();
            }

            return Expression
                .Lambda(delegateType, expression, targetParameter, valueParameter)
                .CompileEx();
        }

        private static Expression[] GetParametersExpression(MethodBase methodBase)
        {
            var paramsInfo = methodBase.GetParameters();
            var argsExp = new Expression[paramsInfo.Length];
            for (var i = 0; i < paramsInfo.Length; i++)
                argsExp[i] = MugenExtensions.GetIndexExpression(i).ConvertIfNeed(paramsInfo[i].ParameterType, false);
            return argsExp;
        }

        private static MethodInfo? TryGetMethodDelegateInternal(Type delegateType, MethodInfo method)
        {
            if (!typeof(Delegate).IsAssignableFrom(delegateType))
                return null;

            var mParameters = method.GetParameters();
            var eParameters = delegateType.GetMethod(nameof(Action.Invoke), BindingFlagsEx.InstancePublic)?.GetParameters();
            if (eParameters == null || mParameters.Length != eParameters.Length)
                return null;
            if (method.IsGenericMethodDefinition)
            {
                var genericArguments = method.GetGenericArguments();
                var types = new Type[genericArguments.Length];
                var index = 0;
                for (var i = 0; i < mParameters.Length; i++)
                {
                    if (mParameters[i].ParameterType.IsGenericParameter)
                        types[index++] = eParameters[i].ParameterType;
                }

                try
                {
                    method = method.MakeGenericMethod(types);
                }
                catch (Exception e)
                {
                    Tracer.Error()?.Trace(nameof(TryGetMethodDelegate), e);
                    return null;
                }

                mParameters = method.GetParameters();
            }

            for (var i = 0; i < mParameters.Length; i++)
            {
                var mParameter = mParameters[i].ParameterType;
                var eParameter = eParameters[i].ParameterType;
                if (!mParameter.IsAssignableFrom(eParameter) || mParameter.IsValueType != eParameter.IsValueType)
                    return null;
            }

            return method;
        }

        #endregion
    }
}