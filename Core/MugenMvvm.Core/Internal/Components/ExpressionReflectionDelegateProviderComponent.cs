﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using MugenMvvm.Attributes;
using MugenMvvm.Collections;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Internal.Components
{
    public sealed class ExpressionReflectionDelegateProviderComponent : IReflectionDelegateProviderComponent, IActivatorReflectionDelegateProviderComponent,
        IMemberReflectionDelegateProviderComponent, IMethodReflectionDelegateProviderComponent, IHasPriority //todo use buffers
    {
        #region Fields

        private static readonly MethodDelegateCache CacheMethodDelegates = new MethodDelegateCache(17);

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ExpressionReflectionDelegateProviderComponent()
        {
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        Func<object?[], object>? IActivatorReflectionDelegateProviderComponent.TryGetActivator(ConstructorInfo constructor)
        {
            return GetActivator(constructor);
        }

        Delegate? IActivatorReflectionDelegateProviderComponent.TryGetActivator(ConstructorInfo constructor, Type delegateType)
        {
            return TryGetActivator(constructor, delegateType);
        }

        Delegate? IMemberReflectionDelegateProviderComponent.TryGetMemberGetter(MemberInfo member, Type delegateType)
        {
            return TryGetMemberGetter(member, delegateType);
        }

        Delegate? IMemberReflectionDelegateProviderComponent.TryGetMemberSetter(MemberInfo member, Type delegateType)
        {
            return TryGetMemberSetter(member, delegateType);
        }

        Func<object?, object?[], object?>? IMethodReflectionDelegateProviderComponent.TryGetMethodInvoker(MethodInfo method)
        {
            return GetMethodInvoker(method);
        }

        Delegate? IMethodReflectionDelegateProviderComponent.TryGetMethodInvoker(MethodInfo method, Type delegateType)
        {
            return TryGetMethodInvoker(method, delegateType);
        }

        bool IReflectionDelegateProviderComponent.CanCreateDelegate(Type delegateType, MethodInfo method)
        {
            return TryGetMethodDelegate(delegateType, method) != null;
        }

        Delegate? IReflectionDelegateProviderComponent.TryCreateDelegate(Type delegateType, object? target, MethodInfo method)
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

        public static Func<object?[], object> GetActivator(ConstructorInfo constructor)
        {
            var expressions = GetParametersExpression(constructor, out var parameterExpression);
            return Expression.Lambda<Func<object?[], object>>(Expression.New(constructor, expressions).ConvertIfNeed(typeof(object), false), parameterExpression).CompileEx();
        }

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
            var expressions = GetParametersExpression(method, out var parameterExpression);
            if (method.IsStatic)
            {
                return Expression
                    .Lambda<Func<object?, object?[], object?>>(Expression
                        .Call(null, method, expressions)
                        .ConvertIfNeed(typeof(object), false), Extensions.MugenExtensions.GetParameterExpression<object>(), parameterExpression)
                    .CompileEx();
            }

            var target = Extensions.MugenExtensions.GetParameterExpression<object>();
            return Expression
                .Lambda<Func<object?, object?[], object?>>(Expression
                    .Call(target.ConvertIfNeed(method.DeclaringType, false), method, expressions)
                    .ConvertIfNeed(typeof(object), false), target, parameterExpression)
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
            if (delegateParameters.Length != 1 || delegateMethod.ReturnType == typeof(void))
                return null;

            var target = Expression.Parameter(delegateParameters[0].ParameterType);
            return Expression
                .Lambda(delegateType, Expression
                    .MakeMemberAccess(member.IsStatic() ? null : target.ConvertIfNeed(member.DeclaringType, false), member)
                    .ConvertIfNeed(delegateMethod.ReturnType, false), target)
                .CompileEx();
        }

        public static Delegate? TryGetMemberSetter(MemberInfo member, Type delegateType)
        {
            var delegateMethod = delegateType.GetMethodOrThrow(nameof(Action.Invoke), BindingFlagsEx.InstanceOnly);
            var delegateParameters = delegateMethod.GetParameters();
            if (delegateParameters.Length != 2)
                return null;

            var fieldInfo = member as FieldInfo;
            Expression expression;
            var targetParameter = Expression.Parameter(delegateParameters[0].ParameterType);
            var valueParameter = Expression.Parameter(delegateParameters[1].ParameterType);
            if (fieldInfo == null)
            {
                var propertyInfo = member as PropertyInfo;
                Should.MethodBeSupported(propertyInfo != null, MessageConstant.ShouldSupportOnlyFieldsReadonlyFields);
                expression = Expression.Assign(Expression.Property(propertyInfo.IsStatic() ? null : targetParameter.ConvertIfNeed(member.DeclaringType, false), propertyInfo),
                    valueParameter.ConvertIfNeed(propertyInfo.PropertyType, false));
            }
            else
            {
                expression = Expression.Assign(Expression.Field(fieldInfo.IsStatic ? null : targetParameter.ConvertIfNeed(member.DeclaringType, false), fieldInfo),
                    valueParameter.ConvertIfNeed(fieldInfo.FieldType, false));
            }

            return Expression
                .Lambda(delegateType, expression, targetParameter, valueParameter)
                .CompileEx();
        }

        private static Expression[] GetParametersExpression(MethodBase methodBase, out ParameterExpression parameterExpression)
        {
            parameterExpression = Extensions.MugenExtensions.GetParameterExpression<object[]>();
            var paramsInfo = methodBase.GetParameters();
            var argsExp = new Expression[paramsInfo.Length];
            for (var i = 0; i < paramsInfo.Length; i++)
                argsExp[i] = Extensions.MugenExtensions.GetIndexExpression(i).ConvertIfNeed(paramsInfo[i].ParameterType, false);
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
                    Tracer.Warn()?.Trace(e.Flatten(true));
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

        #region Nested types

        private sealed class MethodDelegateCache : LightDictionary<KeyValuePair<Type, MethodInfo>, MethodInfo?>
        {
            #region Constructors

            public MethodDelegateCache(int capacity) : base(capacity)
            {
            }

            #endregion

            #region Methods

            protected override bool Equals(KeyValuePair<Type, MethodInfo> x, KeyValuePair<Type, MethodInfo> y)
            {
                return x.Key == y.Key && x.Value == y.Value;
            }

            protected override int GetHashCode(KeyValuePair<Type, MethodInfo> key)
            {
                return HashCode.Combine(key.Key, key.Value);
            }

            #endregion
        }

        #endregion
    }
}