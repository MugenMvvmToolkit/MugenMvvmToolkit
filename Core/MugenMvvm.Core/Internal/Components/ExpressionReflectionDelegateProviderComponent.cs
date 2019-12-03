using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using MugenMvvm.Attributes;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Internal;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Internal.Components
{
    public sealed class ExpressionReflectionDelegateProviderComponent : IReflectionDelegateProviderComponent, IActivatorReflectionDelegateProviderComponent,
        IMemberReflectionDelegateProviderComponent, IMethodReflectionDelegateProviderComponent, IHasPriority//todo use buffers
    {
        #region Fields

        private static readonly MemberInfoDelegateCache<MethodInfo?> CachedDelegates =
            new MemberInfoDelegateCache<MethodInfo?>();

        private static readonly MemberInfoLightDictionary<ConstructorInfo, Func<object?[], object>> ActivatorCache =
            new MemberInfoLightDictionary<ConstructorInfo, Func<object?[], object>>(59);

        private static readonly MemberInfoDelegateCache<Delegate?> ActivatorCacheDelegate =
            new MemberInfoDelegateCache<Delegate?>();

        private static readonly MemberInfoLightDictionary<MethodInfo, Func<object?, object?[], object?>> InvokeMethodCache =
            new MemberInfoLightDictionary<MethodInfo, Func<object?, object?[], object?>>(59);

        private static readonly MemberInfoDelegateCache<Delegate?> InvokeMethodCacheDelegate =
            new MemberInfoDelegateCache<Delegate?>();

        private static readonly MemberInfoDelegateCache<Delegate?> MemberGetterCache =
            new MemberInfoDelegateCache<Delegate?>();

        private static readonly MemberInfoDelegateCache<Delegate?> MemberSetterCache =
            new MemberInfoDelegateCache<Delegate?>();

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
            lock (ActivatorCache)
            {
                if (!ActivatorCache.TryGetValue(constructor, out var value))
                {
                    value = GetActivator(constructor);
                    ActivatorCache[constructor] = value;
                }

                return value;
            }
        }

        Delegate? IActivatorReflectionDelegateProviderComponent.TryGetActivator(ConstructorInfo constructor, Type delegateType)
        {
            var cacheKey = new MemberInfoDelegateCacheKey(constructor, delegateType);
            lock (ActivatorCacheDelegate)
            {
                if (!ActivatorCacheDelegate.TryGetValue(cacheKey, out var value))
                {
                    value = TryGetActivator(constructor, delegateType);
                    ActivatorCacheDelegate[cacheKey] = value;
                }

                return value;
            }
        }

        Delegate? IMemberReflectionDelegateProviderComponent.TryGetMemberGetter(MemberInfo member, Type delegateType)
        {
            var key = new MemberInfoDelegateCacheKey(member, delegateType);
            lock (MemberGetterCache)
            {
                if (!MemberGetterCache.TryGetValue(key, out var value))
                {
                    value = TryGetMemberGetter(member, delegateType);
                    MemberGetterCache[key] = value;
                }

                return value;
            }
        }

        Delegate? IMemberReflectionDelegateProviderComponent.TryGetMemberSetter(MemberInfo member, Type delegateType)
        {
            var key = new MemberInfoDelegateCacheKey(member, delegateType);
            lock (MemberSetterCache)
            {
                if (!MemberSetterCache.TryGetValue(key, out var value))
                {
                    value = TryGetMemberSetter(member, delegateType);
                    MemberSetterCache[key] = value;
                }

                return value;
            }
        }

        Func<object?, object?[], object?>? IMethodReflectionDelegateProviderComponent.TryGetMethodInvoker(MethodInfo method)
        {
            lock (InvokeMethodCache)
            {
                if (!InvokeMethodCache.TryGetValue(method, out var value))
                {
                    value = GetMethodInvoker(method);
                    InvokeMethodCache[method] = value;
                }

                return value;
            }
        }

        Delegate? IMethodReflectionDelegateProviderComponent.TryGetMethodInvoker(MethodInfo method, Type delegateType)
        {
            var cacheKey = new MemberInfoDelegateCacheKey(method, delegateType);
            lock (InvokeMethodCacheDelegate)
            {
                if (!InvokeMethodCacheDelegate.TryGetValue(cacheKey, out var value))
                {
                    value = TryGetMethodInvoker(method, delegateType);
                    InvokeMethodCacheDelegate[cacheKey] = value;
                }

                return value;
            }
        }

        bool IReflectionDelegateProviderComponent.CanCreateDelegate(Type delegateType, MethodInfo method)
        {
            return TryGetMethodDelegateInternal(delegateType, method) != null;
        }

        Delegate? IReflectionDelegateProviderComponent.TryCreateDelegate(Type delegateType, object? target, MethodInfo method)
        {
            method = TryGetMethodDelegateInternal(delegateType, method)!;
            if (method == null)
                return null;

            if (target == null)
                return method.CreateDelegate(delegateType);
            return method.CreateDelegate(delegateType, target);
        }

        #endregion

        #region Methods

        public static MethodInfo? TryGetMethodDelegate(Type delegateType, MethodInfo method)
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
            for (int i = 0; i < methodParameters.Length; i++)
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
                        .ConvertIfNeed(typeof(object), false), MugenExtensions.GetParameterExpression<object>(), parameterExpression)
                    .CompileEx();
            }

            var target = MugenExtensions.GetParameterExpression<object>();
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
                for (int i = 0; i < methodParameters.Length; i++)
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
                for (int i = 1; i < parameters.Length; i++)
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
            parameterExpression = MugenExtensions.GetParameterExpression<object[]>();
            var paramsInfo = methodBase.GetParameters();
            var argsExp = new Expression[paramsInfo.Length];
            for (var i = 0; i < paramsInfo.Length; i++)
                argsExp[i] = MugenExtensions.GetIndexExpression(i).ConvertIfNeed(paramsInfo[i].ParameterType, false);
            return argsExp;
        }

        private static MethodInfo? TryGetMethodDelegateInternal(Type delegateType, MethodInfo method)
        {
            var key = new MemberInfoDelegateCacheKey(method, delegateType);
            MethodInfo? info;
            lock (CachedDelegates)
            {
                if (!CachedDelegates.TryGetValue(key, out info))
                {
                    info = TryGetMethodDelegate(delegateType, method);
                    CachedDelegates[key] = info;
                }
            }

            return info;
        }

        #endregion

        #region Nested types

        private sealed class MemberInfoDelegateCache<TValue> : LightDictionary<MemberInfoDelegateCacheKey, TValue>
        {
            #region Constructors

            public MemberInfoDelegateCache() : base(59)
            {
            }

            #endregion

            #region Methods

            protected override bool Equals(MemberInfoDelegateCacheKey x, MemberInfoDelegateCacheKey y)
            {
                return x.DelegateType == y.DelegateType && x.Member == y.Member;
            }

            protected override int GetHashCode(MemberInfoDelegateCacheKey key)
            {
                return HashCode.Combine(key.DelegateType, key.Member);
            }

            #endregion
        }

        [StructLayout(LayoutKind.Auto)]
        private readonly struct MemberInfoDelegateCacheKey
        {
            #region Fields

            public readonly MemberInfo Member;
            public readonly Type DelegateType;

            #endregion

            #region Constructors

            public MemberInfoDelegateCacheKey(MemberInfo member, Type delegateType)
            {
                Member = member;
                DelegateType = delegateType;
            }

            #endregion
        }

        #endregion
    }
}