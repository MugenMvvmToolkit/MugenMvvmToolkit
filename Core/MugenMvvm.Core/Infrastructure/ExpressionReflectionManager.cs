using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using MugenMvvm.Attributes;
using MugenMvvm.Infrastructure.Internal;
using MugenMvvm.Interfaces;
using MugenMvvm.Models;

namespace MugenMvvm.Infrastructure
{
    public class ExpressionReflectionManager : IReflectionManager
    {
        #region Fields

        private static readonly Dictionary<MethodInfo, Func<object?, object?[], object?>> InvokeMethodCache;
        private static readonly Dictionary<MethodDelegateCacheKey, Delegate> InvokeMethodCacheDelegate;
        private static readonly Dictionary<MemberInfoDelegateCacheKey, Delegate> MemberGetterCache;
        private static readonly ParameterExpression EmptyParameterExpression;
        private static readonly ConstantExpression NullConstantExpression;

        #endregion

        #region Constructors

        static ExpressionReflectionManager()
        {
            InvokeMethodCache = new Dictionary<MethodInfo, Func<object?, object?[], object?>>(MemberInfoComparer.Instance);
            InvokeMethodCacheDelegate = new Dictionary<MethodDelegateCacheKey, Delegate>(MemberCacheKeyComparer.Instance);
            MemberGetterCache = new Dictionary<MemberInfoDelegateCacheKey, Delegate>(MemberCacheKeyComparer.Instance);
            EmptyParameterExpression = Expression.Parameter(typeof(object));
            NullConstantExpression = Expression.Constant(null, typeof(object));
        }

        [Preserve(Conditional = true)]
        public ExpressionReflectionManager()
        {
        }

        #endregion

        #region Implementation of interfaces

        public Func<object?, object?[], object?> GetMethodDelegate(MethodInfo method)
        {
            Should.NotBeNull(method, nameof(method));
            return GetMethodDelegateInternal(method);
        }

        public Delegate GetMethodDelegate(Type delegateType, MethodInfo method)
        {
            Should.NotBeNull(delegateType, nameof(delegateType));
            Should.NotBeNull(method, nameof(method));
            return GetMethodDelegateInternal(delegateType, method);
        }

        #endregion

        #region Methods

        public Func<object, TType> GetMemberGetter<TType>(MemberInfo member)
        {
            Should.NotBeNull(member, nameof(member));
            return GetMemberGetterInternal<TType>(member);
        }

        protected virtual Func<object?, TType> GetMemberGetterInternal<TType>(MemberInfo member)
        {
            var key = new MemberInfoDelegateCacheKey(member, typeof(TType));
            lock (MemberGetterCache)
            {
                if (!MemberGetterCache.TryGetValue(key, out var value))
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

                    value = Expression
                        .Lambda<Func<object?, TType>>(ConvertIfNeed(accessExp, typeof(TType), false), target)
                        .Compile();
                    MemberGetterCache[key] = value;
                }

                return (Func<object?, TType>) value;
            }
        }

        protected virtual Delegate GetMethodDelegateInternal(Type delegateType, MethodInfo method)
        {
            lock (InvokeMethodCacheDelegate)
            {
                var cacheKey = new MethodDelegateCacheKey(method, delegateType);
                if (!InvokeMethodCacheDelegate.TryGetValue(cacheKey, out var value))
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
                    value = lambdaExpression.Compile();
                    InvokeMethodCacheDelegate[cacheKey] = value;
                }

                return value;
            }
        }

        protected virtual Func<object?, object?[], object?> GetMethodDelegateInternal(MethodInfo method)
        {
            lock (InvokeMethodCache)
            {
                if (!InvokeMethodCache.TryGetValue(method, out var value))
                {
                    value = CreateMethodInvoke(method);
                    InvokeMethodCache[method] = value;
                }

                return value;
            }
        }

        private static Func<object?, object?[], object?> CreateMethodInvoke(MethodInfo methodInfo)
        {
            var isVoid = methodInfo.ReturnType.EqualsEx(typeof(void));
            var expressions = GetParametersExpression(methodInfo, out var parameterExpression);
            Expression callExpression;
            if (methodInfo.IsStatic)
            {
                callExpression = Expression.Call(null, methodInfo, expressions);
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

            var declaringType = methodInfo.DeclaringType;
            var targetExp = Expression.Parameter(typeof(object), "target");
            callExpression = Expression.Call(ConvertIfNeed(targetExp, declaringType, false), methodInfo, expressions);
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

        private static Expression[] GetParametersExpression(MethodBase methodBase,
            out ParameterExpression parameterExpression)
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

        internal static Expression ConvertIfNeed(Expression expression, Type type, bool exactly)
        {
            if (expression == null)
                return null;
            if (type.EqualsEx(typeof(void)) || type.EqualsEx(expression.Type))
                return expression;
            if (!exactly && !expression.Type.IsValueTypeUnified() && !type.IsValueTypeUnified() && type.IsAssignableFromUnified(expression.Type))
                return expression;
            return Expression.Convert(expression, type);
        }

        #endregion

        #region Nested types

        protected sealed class MemberCacheKeyComparer : IEqualityComparer<MethodDelegateCacheKey>, IEqualityComparer<MemberInfoDelegateCacheKey>
        {
            #region Fields

            public static readonly MemberCacheKeyComparer Instance;

            #endregion

            #region Constructors

            static MemberCacheKeyComparer()
            {
                Instance = new MemberCacheKeyComparer();
            }

            private MemberCacheKeyComparer()
            {
            }

            #endregion

            #region Implementation of interfaces

            bool IEqualityComparer<MemberInfoDelegateCacheKey>.Equals(MemberInfoDelegateCacheKey x, MemberInfoDelegateCacheKey y)
            {
                return x.DelegateType.EqualsEx(y.DelegateType) && x.Member.EqualsEx(y.Member);
            }

            int IEqualityComparer<MemberInfoDelegateCacheKey>.GetHashCode(MemberInfoDelegateCacheKey obj)
            {
                unchecked
                {
                    return obj.DelegateType.GetHashCode() * 397 ^ obj.Member.GetHashCode();
                }
            }

            bool IEqualityComparer<MethodDelegateCacheKey>.Equals(MethodDelegateCacheKey x, MethodDelegateCacheKey y)
            {
                return x.DelegateType.EqualsEx(y.DelegateType) && x.Method.EqualsEx(y.Method);
            }

            int IEqualityComparer<MethodDelegateCacheKey>.GetHashCode(MethodDelegateCacheKey obj)
            {
                unchecked
                {
                    return obj.DelegateType.GetHashCode() * 397 ^ obj.Method.GetHashCode();
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

        [StructLayout(LayoutKind.Auto)]
        protected struct MemberInfoDelegateCacheKey
        {
            #region Fields

            public MemberInfo Member;
            public Type DelegateType;

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