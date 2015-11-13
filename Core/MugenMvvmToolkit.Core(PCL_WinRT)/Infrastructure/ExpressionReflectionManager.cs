#region Copyright

// ****************************************************************************
// <copyright file="ExpressionReflectionManager.cs">
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using MugenMvvmToolkit.Interfaces;

namespace MugenMvvmToolkit.Infrastructure
{
    public class ExpressionReflectionManager : IReflectionManager
    {
        #region Nested types

        protected sealed class MethodDelegateCacheKeyComparer : IEqualityComparer<MethodDelegateCacheKey>
        {
            #region Fields

            public static readonly MethodDelegateCacheKeyComparer Instance;

            #endregion

            #region Constructors

            static MethodDelegateCacheKeyComparer()
            {
                Instance = new MethodDelegateCacheKeyComparer();
            }

            private MethodDelegateCacheKeyComparer()
            {
            }

            #endregion

            #region Implementation of IEqualityComparer<in MethodDelegateCacheKey>

            public bool Equals(MethodDelegateCacheKey x, MethodDelegateCacheKey y)
            {
                return x.DelegateType.Equals(y.DelegateType) &&
                       (ReferenceEquals(x.Method, y.Method) || x.Method.Equals(y.Method));
            }

            public int GetHashCode(MethodDelegateCacheKey obj)
            {
                unchecked
                {
                    return (obj.DelegateType.GetHashCode() * 397) ^ obj.Method.GetHashCode();
                }
            }

            #endregion
        }

        [StructLayout(LayoutKind.Auto)]
        protected struct MethodDelegateCacheKey
        {
            #region Fields

            public MethodInfo Method;
            public Type DelegateType;

            #endregion

            #region Constructors

            public MethodDelegateCacheKey(MethodInfo method, Type delegateType)
            {
                Method = method;
                DelegateType = delegateType;
            }

            #endregion
        }

        #endregion

        #region Fields

        private static readonly Dictionary<MethodDelegateCacheKey, MethodInfo> CachedDelegates;
        private static readonly Dictionary<ConstructorInfo, Func<object[], object>> ActivatorCache;
        private static readonly Dictionary<MethodInfo, Func<object, object[], object>> InvokeMethodCache;
        private static readonly Dictionary<MemberInfo, Delegate> MemberAccessCache;
        private static readonly Dictionary<MemberInfo, Delegate> MemberSetterCache;
        private static readonly Dictionary<MethodDelegateCacheKey, Delegate> InvokeMethodCacheDelegate;
        private static Func<Type, Expression, IEnumerable<ParameterExpression>, LambdaExpression> _createLambdaExpressionByType;
        private static Func<Expression, ParameterExpression[], LambdaExpression> _createLambdaExpression;
        private static readonly ParameterExpression EmptyParameterExpression;
        private static readonly ConstantExpression NullConstantExpression;

        #endregion

        #region Constructors

        static ExpressionReflectionManager()
        {
            _createLambdaExpression = Expression.Lambda;
            _createLambdaExpressionByType = Expression.Lambda;
            CachedDelegates = new Dictionary<MethodDelegateCacheKey, MethodInfo>(MethodDelegateCacheKeyComparer.Instance);
            ActivatorCache = new Dictionary<ConstructorInfo, Func<object[], object>>();
            InvokeMethodCache = new Dictionary<MethodInfo, Func<object, object[], object>>();
            MemberAccessCache = new Dictionary<MemberInfo, Delegate>();
            MemberSetterCache = new Dictionary<MemberInfo, Delegate>();
            InvokeMethodCacheDelegate = new Dictionary<MethodDelegateCacheKey, Delegate>(MethodDelegateCacheKeyComparer.Instance);
            EmptyParameterExpression = Expression.Parameter(typeof(object));
            NullConstantExpression = Expression.Constant(null, typeof(object));
        }

        #endregion

        #region Properties

        public static Func<Type, Expression, IEnumerable<ParameterExpression>, LambdaExpression> CreateLambdaExpressionByType
        {
            get { return _createLambdaExpressionByType; }
            set
            {
                Should.PropertyNotBeNull(value);
                _createLambdaExpressionByType = value;
            }
        }

        public static Func<Expression, ParameterExpression[], LambdaExpression> CreateLambdaExpression
        {
            get { return _createLambdaExpression; }
            set
            {
                Should.PropertyNotBeNull(value);
                _createLambdaExpression = value;
            }
        }

        #endregion

        #region Implementation of IReflectionProvider

        public virtual Delegate TryCreateDelegate(Type delegateType, object target, MethodInfo method)
        {
            MethodInfo result;
            lock (CachedDelegates)
            {
                var cacheKey = new MethodDelegateCacheKey(method, delegateType);
                if (!CachedDelegates.TryGetValue(cacheKey, out result))
                {
                    result = TryCreateMethodDelegate(delegateType, method);
                    CachedDelegates[cacheKey] = result;
                }
                if (result == null)
                    return null;
            }
#if PCL_WINRT
            if (target == null)
                return result.CreateDelegate(delegateType);
            return result.CreateDelegate(delegateType, target);
#else
            if (target == null)
                return Delegate.CreateDelegate(delegateType, result);
            return Delegate.CreateDelegate(delegateType, target, result);
#endif
        }

        public virtual Func<object[], object> GetActivatorDelegate(ConstructorInfo constructor)
        {
            Should.NotBeNull(constructor, "constructor");
            lock (ActivatorCache)
            {
                Func<object[], object> value;
                if (!ActivatorCache.TryGetValue(constructor, out value))
                {
                    ParameterExpression parameterExpression;
                    Expression[] expressions = GetParametersExpression(constructor, out parameterExpression);
                    Expression newExpression = ConvertIfNeed(Expression.New(constructor, expressions), typeof(object), false);
                    value = Expression.Lambda<Func<object[], object>>(newExpression, parameterExpression).Compile();
                }
                ActivatorCache[constructor] = value;
                return value;
            }
        }

        public virtual Func<object, object[], object> GetMethodDelegate(MethodInfo method)
        {
            Should.NotBeNull(method, "method");
            lock (InvokeMethodCache)
            {
                Func<object, object[], object> value;
                if (!InvokeMethodCache.TryGetValue(method, out value))
                {
                    value = CreateMethodInvoke(method);
                    InvokeMethodCache[method] = value;
                }
                return value;
            }
        }

        public virtual Delegate GetMethodDelegate(Type delegateType, MethodInfo method)
        {
            Should.NotBeNull(delegateType, "delegateType");
            Should.BeOfType<Delegate>(delegateType, "delegateType");
            Should.NotBeNull(method, "method");
            lock (InvokeMethodCacheDelegate)
            {
                var cacheKey = new MethodDelegateCacheKey(method, delegateType);
                Delegate value;
                if (!InvokeMethodCacheDelegate.TryGetValue(cacheKey, out value))
                {
                    MethodInfo delegateMethod = delegateType.GetMethodEx("Invoke");
                    var delegateParams = delegateMethod.GetParameters().ToList();
                    ParameterInfo[] methodParams = method.GetParameters();
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
                    for (int i = 0; i < methodParams.Length; i++)
                    {
                        ParameterExpression parameter = Expression.Parameter(delegateParams[i].ParameterType, i.ToString());
                        parameters.Add(parameter);
                        expressions.Add(ConvertIfNeed(parameter, methodParams[i].ParameterType, false));
                    }

                    Expression callExpression;
                    if (method.IsStatic)
                        callExpression = Expression.Call(null, method, expressions.ToArrayEx());
                    else
                    {
                        Expression @this = expressions[0];
                        expressions.RemoveAt(0);
                        callExpression = Expression.Call(@this, method, expressions.ToArrayEx());
                    }

                    if (delegateMethod.ReturnType != typeof(void))
                        callExpression = ConvertIfNeed(callExpression, delegateMethod.ReturnType, false);
                    var lambdaExpression = CreateLambdaExpressionByType(delegateType, callExpression, parameters);
                    value = lambdaExpression.Compile();
                    InvokeMethodCacheDelegate[cacheKey] = value;
                }
                return value;
            }
        }

        public virtual Func<object, TType> GetMemberGetter<TType>(MemberInfo member)
        {
            Should.NotBeNull(member, "member");
            lock (MemberAccessCache)
            {
                Delegate value;
                if (!MemberAccessCache.TryGetValue(member, out value) || !(value is Func<object, TType>))
                {
                    ParameterExpression target = Expression.Parameter(typeof(object), "instance");
                    MemberExpression accessExp;
                    if (IsStatic(member))
                        accessExp = Expression.MakeMemberAccess(null, member);
                    else
                    {
                        Type declaringType = member.DeclaringType;
                        accessExp = Expression.MakeMemberAccess(ConvertIfNeed(target, declaringType, false), member);
                    }
                    value = Expression
                        .Lambda<Func<object, TType>>(ConvertIfNeed(accessExp, typeof(TType), false), target)
                        .Compile();
                    MemberAccessCache[member] = value;
                }
                return (Func<object, TType>)value;
            }
        }

        public virtual Action<object, TType> GetMemberSetter<TType>(MemberInfo member)
        {
            Should.NotBeNull(member, "member");
            lock (MemberSetterCache)
            {
                Delegate action;
                if (!MemberAccessCache.TryGetValue(member, out action) || !(action is Action<object, TType>))
                {
                    var declaringType = member.DeclaringType;
                    var fieldInfo = member as FieldInfo;
#if PCL_WINRT
                    if (declaringType.GetTypeInfo().IsValueType)
#else
                    if (declaringType.IsValueType)
#endif
                    {
                        Action<object, TType> result;
                        if (fieldInfo == null)
                        {
                            var propertyInfo = (PropertyInfo)member;
                            result = propertyInfo.SetValue<TType>;
                        }
                        else
                            result = fieldInfo.SetValue<TType>;
                        MemberAccessCache[member] = result;
                        return result;
                    }

                    Expression expression;
                    var targetParameter = Expression.Parameter(typeof(object), "instance");
                    var valueParameter = Expression.Parameter(typeof(TType), "value");
                    var target = ConvertIfNeed(targetParameter, declaringType, false);
                    if (fieldInfo == null)
                    {
                        var propertyInfo = member as PropertyInfo;
                        MethodInfo setMethod = null;
                        if (propertyInfo != null)
                            setMethod = propertyInfo.GetSetMethod(true);
                        Should.MethodBeSupported(propertyInfo != null && setMethod != null,
                            "supports only properties (non-readonly) and fields");
                        var valueExpression = ConvertIfNeed(valueParameter, propertyInfo.PropertyType, false);
                        expression =
                            Expression.Call(setMethod.IsStatic ? null : ConvertIfNeed(target, declaringType, false),
                                setMethod, valueExpression);
                    }
                    else
                    {
                        expression = Expression.Field(fieldInfo.IsStatic ? null : ConvertIfNeed(target, declaringType, false), fieldInfo);
                        expression = Expression.Assign(expression, ConvertIfNeed(valueParameter, fieldInfo.FieldType, false));
                    }
                    action = Expression
                        .Lambda<Action<object, TType>>(expression, targetParameter, valueParameter)
                        .Compile();
                    MemberAccessCache[member] = action;
                }
                return (Action<object, TType>)action;
            }
        }

        #endregion

        #region Methods

        protected static MethodInfo TryCreateMethodDelegate(Type eventHandlerType, MethodInfo method)
        {
            if (!typeof(Delegate).IsAssignableFrom(eventHandlerType))
                return null;

            ParameterInfo[] mParameters = method.GetParameters();
            ParameterInfo[] eParameters = eventHandlerType.GetMethodEx("Invoke").GetParameters();
            if (mParameters.Length != eParameters.Length)
                return null;
            if (method.IsGenericMethodDefinition)
            {
                var genericArguments = method.GetGenericArguments();
                var types = new Type[genericArguments.Length];
                int index = 0;
                for (int i = 0; i < mParameters.Length; i++)
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
                    Tracer.Warn(e.Flatten(true));
                    return null;
                }
                mParameters = method.GetParameters();
            }
            for (int i = 0; i < mParameters.Length; i++)
            {
#if PCL_WINRT
                var mParameter = mParameters[i].ParameterType.GetTypeInfo();
                var eParameter = eParameters[i].ParameterType.GetTypeInfo();
                if (!mParameter.IsAssignableFrom(eParameter) || mParameter.IsValueType != eParameter.IsValueType)
                    return null;
#else
                Type mParameter = mParameters[i].ParameterType;
                Type eParameter = eParameters[i].ParameterType;
                if (!mParameter.IsAssignableFrom(eParameter) || mParameter.IsValueType != eParameter.IsValueType)
                    return null;
#endif
            }
            return method;
        }

        private static Func<object, object[], object> CreateMethodInvoke(MethodInfo methodInfo)
        {
            bool isVoid = methodInfo.ReturnType.Equals(typeof(void));
            ParameterExpression parameterExpression;
            var expressions = GetParametersExpression(methodInfo, out parameterExpression);
            Expression callExpression;
            if (methodInfo.IsStatic)
            {
                callExpression = Expression.Call(null, methodInfo, expressions);
                if (isVoid)
                    return Expression
                        .Lambda<Func<object, object[], object>>(
                            Expression.Block(callExpression, NullConstantExpression), EmptyParameterExpression,
                            parameterExpression)
                        .Compile();

                callExpression = ConvertIfNeed(callExpression, typeof(object), false);
                return Expression
                    .Lambda<Func<object, object[], object>>(callExpression, EmptyParameterExpression, parameterExpression)
                    .Compile();
            }
            Type declaringType = methodInfo.DeclaringType;
            var targetExp = Expression.Parameter(typeof(object), "target");
            callExpression = Expression.Call(ConvertIfNeed(targetExp, declaringType, false), methodInfo, expressions);
            if (isVoid)
                return Expression
                    .Lambda<Func<object, object[], object>>(Expression.Block(callExpression, NullConstantExpression),
                        targetExp, parameterExpression)
                    .Compile();
            callExpression = ConvertIfNeed(callExpression, typeof(object), false);
            return Expression
                .Lambda<Func<object, object[], object>>(callExpression, targetExp, parameterExpression)
                .Compile();
        }

        private static bool IsStatic(MemberInfo member)
        {
            var propertyInfo = member as PropertyInfo;
            if (propertyInfo != null)
            {
                MethodInfo method = propertyInfo.CanRead
                    ? propertyInfo.GetGetMethod(true)
                    : propertyInfo.GetSetMethod(true);
                return method == null || method.IsStatic;
            }
            var methodInfo = member as MethodInfo;
            if (methodInfo != null)
                return methodInfo.IsStatic;
            var fieldInfo = member as FieldInfo;
            return fieldInfo == null || fieldInfo.IsStatic;
        }

        private static Expression[] GetParametersExpression(MethodBase methodBase,
            out ParameterExpression parameterExpression)
        {
            ParameterInfo[] paramsInfo = methodBase.GetParameters();
            //create a single param of type object[]
            parameterExpression = Expression.Parameter(typeof(object[]), "args");
            var argsExp = new Expression[paramsInfo.Length];

            //pick each arg from the params array
            //and create a typed expression of them
            for (int i = 0; i < paramsInfo.Length; i++)
            {
                Expression index = Expression.Constant(i);
                Type paramType = paramsInfo[i].ParameterType;
                Expression paramAccessorExp = Expression.ArrayIndex(parameterExpression, index);
                Expression paramCastExp = ConvertIfNeed(paramAccessorExp, paramType, false);
                argsExp[i] = paramCastExp;
            }
            return argsExp;
        }

        internal static Expression ConvertIfNeed(Expression expression, Type type, bool exactly)
        {
            if (expression == null)
                return null;
            if (type.Equals(typeof(void)) || type.Equals(expression.Type))
                return expression;

#if PCL_WINRT
            var typeInfo = type.GetTypeInfo();
            if (!exactly && !expression.Type.GetTypeInfo().IsValueType && !typeInfo.IsValueType && type.IsAssignableFrom(expression.Type))
                return expression;
#else
            if (!exactly && !expression.Type.IsValueType && !type.IsValueType && type.IsAssignableFrom(expression.Type))
                return expression;
#endif
            return Expression.Convert(expression, type);
        }

        #endregion
    }
}
