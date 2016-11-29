#region Copyright

// ****************************************************************************
// <copyright file="ExpressionReflectionManager.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Infrastructure
{
    public class ExpressionReflectionManager : IReflectionManager
    {
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

            #region Implementation of IEqualityComparer<in MemberInfoDelegateCacheKey>

            bool IEqualityComparer<MemberInfoDelegateCacheKey>.Equals(MemberInfoDelegateCacheKey x, MemberInfoDelegateCacheKey y)
            {
                return x.DelegateType.Equals(y.DelegateType) &&
                       (ReferenceEquals(x.Member, y.Member) || x.Member.Equals(y.Member));
            }

            int IEqualityComparer<MemberInfoDelegateCacheKey>.GetHashCode(MemberInfoDelegateCacheKey obj)
            {
                unchecked
                {
                    return (obj.DelegateType.GetHashCode() * 397) ^ obj.Member.GetHashCode();
                }
            }

            #endregion

            #region Implementation of IEqualityComparer<in MethodDelegateCacheKey>

            bool IEqualityComparer<MethodDelegateCacheKey>.Equals(MethodDelegateCacheKey x, MethodDelegateCacheKey y)
            {
                return x.DelegateType.Equals(y.DelegateType) &&
                       (ReferenceEquals(x.Method, y.Method) || x.Method.Equals(y.Method));
            }

            int IEqualityComparer<MethodDelegateCacheKey>.GetHashCode(MethodDelegateCacheKey obj)
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

        #region Fields

        private static readonly Dictionary<MethodDelegateCacheKey, MethodInfo> CachedDelegates;
        private static readonly Dictionary<ConstructorInfo, Func<object[], object>> ActivatorCache;
        private static readonly Dictionary<MethodInfo, Func<object, object[], object>> InvokeMethodCache;
        private static readonly Dictionary<MemberInfoDelegateCacheKey, Delegate> MemberGetterCache;
        private static readonly Dictionary<MemberInfoDelegateCacheKey, Delegate> MemberSetterCache;
        private static readonly Dictionary<MethodDelegateCacheKey, Delegate> InvokeMethodCacheDelegate;
        private static readonly ParameterExpression EmptyParameterExpression;
        private static readonly ConstantExpression NullConstantExpression;

        private static readonly Dictionary<MethodDelegateCacheKey, Func<object, Delegate>> CompiledCachedDelegates;

        #endregion

        #region Constructors

        static ExpressionReflectionManager()
        {
            CompiledCachedDelegates = new Dictionary<MethodDelegateCacheKey, Func<object, Delegate>>(MemberCacheKeyComparer.Instance);
            CachedDelegates = new Dictionary<MethodDelegateCacheKey, MethodInfo>(MemberCacheKeyComparer.Instance);
            ActivatorCache = new Dictionary<ConstructorInfo, Func<object[], object>>();
            InvokeMethodCache = new Dictionary<MethodInfo, Func<object, object[], object>>();
            MemberGetterCache = new Dictionary<MemberInfoDelegateCacheKey, Delegate>(MemberCacheKeyComparer.Instance);
            MemberSetterCache = new Dictionary<MemberInfoDelegateCacheKey, Delegate>(MemberCacheKeyComparer.Instance);
            InvokeMethodCacheDelegate = new Dictionary<MethodDelegateCacheKey, Delegate>(MemberCacheKeyComparer.Instance);
            EmptyParameterExpression = Expression.Parameter(typeof(object));
            NullConstantExpression = Expression.Constant(null, typeof(object));
        }

        #endregion

        #region Implementation of IReflectionProvider

        public Delegate TryCreateDelegate(Type delegateType, object target, MethodInfo method)
        {
            Should.NotBeNull(delegateType, nameof(delegateType));
            Should.NotBeNull(method, nameof(method));
            Func<object, Delegate> result;
            if (CompiledCachedDelegates.Count != 0 && CompiledCachedDelegates.TryGetValue(new MethodDelegateCacheKey(method, delegateType), out result))
                return result(target);
            return TryCreateDelegateInternal(delegateType, target, method);
        }

        public Func<object[], object> GetActivatorDelegate(ConstructorInfo constructor)
        {
            Should.NotBeNull(constructor, nameof(constructor));
            return GetActivatorDelegateInternal(constructor);
        }

        public Func<object, object[], object> GetMethodDelegate(MethodInfo method)
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

        public Func<object, TType> GetMemberGetter<TType>(MemberInfo member)
        {
            Should.NotBeNull(member, nameof(member));
            return GetMemberGetterInternal<TType>(member);
        }

        public Action<object, TType> GetMemberSetter<TType>(MemberInfo member)
        {
            Should.NotBeNull(member, nameof(member));
            return GetMemberSetterInternal<TType>(member);
        }

        #endregion

        #region Methods

        public static void AddCompiledDelegateFactory(Type type, string methodName, Type delegateType, Func<object, Delegate> createDelegate, params Type[] args)
        {
            var method = GetMethod(type, methodName, args);
            if (method != null)
                CompiledCachedDelegates[new MethodDelegateCacheKey(method, delegateType)] = createDelegate;
        }

        public static void AddCompiledActivator(Type type, Func<object[], object> createInstance, params Type[] args)
        {
            var constructor = type.GetConstructor(args);
            if (constructor != null)
                ActivatorCache[constructor] = createInstance;
        }

        public static void AddCompiledMethodDelegate(Type type, string methodName, Func<object, object[], object> methodInvoke, params Type[] args)
        {
            var method = GetMethod(type, methodName, args);
            if (method != null)
                InvokeMethodCache[method] = methodInvoke;
        }

        public static void AddCompiledMethodDelegate<TDelegate>(Type type, string methodName, TDelegate methodInvoke, params Type[] args)
            where TDelegate : class
        {
            var method = GetMethod(type, methodName, args);
            if (method != null)
                InvokeMethodCacheDelegate[new MethodDelegateCacheKey(method, typeof(TDelegate))] = (Delegate)(object)methodInvoke;
        }

        public static void AddCompiledMemberGetter<TResult>(Type type, string memberName, bool isProperty, Func<object, TResult> getter)
        {
            var member = GetMember(type, memberName, isProperty);
            if (member != null)
                MemberGetterCache[new MemberInfoDelegateCacheKey(member, typeof(TResult))] = getter;
        }

        public static void AddCompiledMemberSetter<TValue>(Type type, string memberName, bool isProperty, Action<object, TValue> setter)
        {
            var member = GetMember(type, memberName, isProperty);
            if (member != null)
                MemberSetterCache[new MemberInfoDelegateCacheKey(member, typeof(TValue))] = setter;
        }

        private static MethodInfo GetMethod(Type type, string methodName, Type[] args)
        {
            var method = type.GetMethodEx(methodName, args, MemberFlags.Static | MemberFlags.Instance | MemberFlags.Public);
            if (method != null)
                return method;
            var methods = type.GetMethodsEx(MemberFlags.Static | MemberFlags.Instance | MemberFlags.Public)
                .Where(info => info.Name == methodName && info.GetParameters().Length == args.Length)
                .ToList();
            if (methods.Count == 1)
                return methods[0];
            return null;
        }

        private static MemberInfo GetMember(Type type, string member, bool isProperty)
        {
            if (isProperty)
                return type.GetPropertyEx(member, MemberFlags.Static | MemberFlags.Instance | MemberFlags.Public);
            return type.GetFieldEx(member, MemberFlags.Static | MemberFlags.Instance | MemberFlags.Public);
        }

        protected static void GenerateDelegateFactoryCode(Type delegateType, MethodInfo method)
        {
            var builder = ServiceProvider.BootstrapCodeBuilder;
            if (builder == null || !method.IsPublic || !delegateType.IsPublic())
                return;
            var reflectedType = GetReflectedType(method);
            if (!reflectedType.IsPublic())
                return;
            var parameters = method.GetParameters();
            if (parameters.Any(info => !info.ParameterType.IsPublic()))
                return;
            var types = GenerateParameterTypes(parameters);
            string invoke;
            if (method.IsStatic)
                invoke = $"{method.DeclaringType.GetPrettyName()}.{method.Name}";
            else
                invoke = $"(({method.DeclaringType.GetPrettyName()})item).{method.Name}";
            builder.AppendStatic(nameof(ExpressionReflectionManager),
                $"{typeof(ExpressionReflectionManager).FullName}.{nameof(AddCompiledDelegateFactory)}(typeof({reflectedType.GetPrettyName()}), \"{method.Name}\", typeof({delegateType.GetPrettyName()}), item => new {delegateType.GetPrettyName()}({invoke}){types});");
        }

        protected static void GenerateActivatorCode(ConstructorInfo constructor)
        {
            var builder = ServiceProvider.BootstrapCodeBuilder;
            if (builder == null || !constructor.IsPublic || !constructor.DeclaringType.IsPublic())
                return;
            var parameters = constructor.GetParameters();
            if (parameters.Any(info => !info.ParameterType.IsPublic()))
                return;
            var types = GenerateParameterTypes(parameters);
            var parametersCast = string.Join(", ", parameters.Select((info, i) => $"({info.ParameterType.GetPrettyName()})args[{i}]"));
            var invoke = $"args => new {constructor.DeclaringType.GetPrettyName()}({parametersCast})";

            builder.AppendStatic(nameof(ExpressionReflectionManager),
                $"{typeof(ExpressionReflectionManager).FullName}.{nameof(AddCompiledActivator)}(typeof({GetReflectedType(constructor).GetPrettyName()}), {invoke}{types});");
        }

        protected static void GenerateInvokeMethodCode(MethodInfo method, Type delegateType)
        {
            var builder = ServiceProvider.BootstrapCodeBuilder;
            if (builder == null || !method.IsPublic || !delegateType.IsPublic())
                return;
            var reflectedType = GetReflectedType(method);
            if (!reflectedType.IsPublic())
                return;
            var parameters = method.GetParameters();
            if (parameters.Any(info => !info.ParameterType.IsPublic()))
                return;
            var types = GenerateParameterTypes(parameters);
            var lambdaParameters = string.Join(", ", parameters.Select((info, i) => $"p{i}"));
            var parametersCast = string.Join(", ", parameters.Select((info, i) => $"({info.ParameterType.GetPrettyName()})p{i}"));
            string invoke;
            if (method.IsStatic)
                invoke = $"({lambdaParameters}) => {method.DeclaringType.GetPrettyName()}.{method.Name}({parametersCast})";
            else
                invoke = $"(item, {lambdaParameters}) => (({method.DeclaringType.GetPrettyName()})item).{method.Name}({parametersCast})";
            if (method.ReturnType != typeof(void))
                invoke = $"({method.ReturnType.GetPrettyName()}){invoke}";

            builder.AppendStatic(nameof(ExpressionReflectionManager),
                $"{typeof(ExpressionReflectionManager).FullName}.{nameof(AddCompiledMethodDelegate)}(typeof({reflectedType.GetPrettyName()}), \"{method.Name}\", new {delegateType.GetPrettyName()}({invoke}){types});");
        }

        protected static void GenerateInvokeMethodCode(MethodInfo method)
        {
            var builder = ServiceProvider.BootstrapCodeBuilder;
            if (builder == null || !method.IsPublic)
                return;
            var reflectedType = GetReflectedType(method);
            if (!reflectedType.IsPublic())
                return;
            var parameters = method.GetParameters();
            if (parameters.Any(info => !info.ParameterType.IsPublic()))
                return;

            var types = GenerateParameterTypes(parameters);
            var parametersCast = string.Join(", ", parameters.Select((info, i) => $"({info.ParameterType.GetPrettyName()})args[{i}]"));

            string eventName = null;
            bool indexGet = false;
            bool indexSet = false;
            bool isDelete = false;
            if (method.Name.StartsWith("add_"))
                eventName = method.Name.Substring(4);
            else if (method.Name.StartsWith("delete_"))
            {
                eventName = method.Name.Substring(7);
                isDelete = true;
            }
            else if (method.Name.StartsWith("get_"))
                indexGet = true;
            else if (method.Name.StartsWith("set_"))
                indexSet = true;

            string invoke;
            if (method.IsStatic)
            {
                if (eventName != null)
                    invoke = $"{method.DeclaringType.GetPrettyName()}.{eventName} {(isDelete ? "-" : "+")}= {parametersCast})";
                else if (indexGet)
                    invoke = $"{method.DeclaringType.GetPrettyName()}[{parametersCast}]";
                else if (indexSet)
                {
                    var setterCast = string.Join(", ", parameters.Take(parameters.Length - 1).Select((info, i) => $"({info.ParameterType.GetPrettyName()})args[{i}]"));
                    invoke = $"{method.DeclaringType.GetPrettyName()}[{setterCast}] = ({parameters.Last().ParameterType.GetPrettyName()}) args[{parameters.Length - 1}]";
                }
                else
                    invoke = $"{method.DeclaringType.GetPrettyName()}.{method.Name}({parametersCast})";
            }
            else
            {
                var itemAccess = $"(({ method.DeclaringType.GetPrettyName()})item)";
                if (eventName != null)
                    invoke = $"{itemAccess}.{eventName} {(isDelete ? "-" : "+")}= {parametersCast}";
                else if (indexGet)
                    invoke = $"{itemAccess}[{parametersCast}]";
                else if (indexSet)
                {
                    var setterCast = string.Join(", ", parameters.Take(parameters.Length - 1).Select((info, i) => $"({info.ParameterType.GetPrettyName()})args[{i}]"));
                    invoke = $"{itemAccess}[{setterCast}] = ({parameters.Last().ParameterType.GetPrettyName()}) args[{parameters.Length - 1}]";
                }
                else
                    invoke = $"{itemAccess}.{method.Name}({parametersCast})";
            }
            if (method.ReturnType == typeof(void))
                invoke = "{" + $"{invoke}; return null;" + "}";

            builder.AppendStatic(nameof(ExpressionReflectionManager),
                $"{typeof(ExpressionReflectionManager).FullName}.{nameof(AddCompiledMethodDelegate)}(typeof({reflectedType.GetPrettyName()}), \"{method.Name}\", (item, args) => {invoke}{types});");
        }

        protected static void GenerateGetterCode(MemberInfo member, Type resultType)
        {
            var builder = ServiceProvider.BootstrapCodeBuilder;
            if (builder == null || !resultType.IsPublic())
                return;
            var propertyInfo = member as PropertyInfo;
            if (propertyInfo == null)
            {
                if (!((FieldInfo)member).IsPublic)
                    return;
            }
            else if (propertyInfo.GetGetMethod(false) == null)
                return;

            var reflectedType = GetReflectedType(member);
            if (!reflectedType.IsPublic())
                return;

            string getter;
            if (IsStatic(member))
                getter = $"_ => ({resultType.GetPrettyName()}) {member.DeclaringType.GetPrettyName()}.{member.Name}";
            else
                getter = $"item => ({resultType.GetPrettyName()}) (({member.DeclaringType.GetPrettyName()})item).{member.Name}";
            builder.AppendStatic(nameof(ExpressionReflectionManager),
                $"{typeof(ExpressionReflectionManager).FullName}.{nameof(AddCompiledMemberGetter)}(typeof({reflectedType.GetPrettyName()}), \"{member.Name}\", {(member is PropertyInfo ? "true" : "false")}, {getter});");
        }

        protected static void GenerateSetterCode(MemberInfo member, Type delegateType, Type memberType)
        {
            var builder = ServiceProvider.BootstrapCodeBuilder;
            if (builder == null || !delegateType.IsPublic() || !memberType.IsPublic())
                return;
            var propertyInfo = member as PropertyInfo;
            if (propertyInfo == null)
            {
                if (!((FieldInfo)member).IsPublic)
                    return;
            }
            else if (propertyInfo.GetSetMethod(false) == null)
                return;
            var reflectedType = GetReflectedType(member);
            if (!reflectedType.IsPublic())
                return;

            string setter;
            if (IsStatic(member))
                setter = $"(_, value) => {member.DeclaringType.GetPrettyName()}.{member.Name} = ({memberType.GetPrettyName()})value";
            else
                setter = $"(item, value) => (({member.DeclaringType.GetPrettyName()})item).{member.Name} = ({memberType.GetPrettyName()})value";
            builder.AppendStatic(nameof(ExpressionReflectionManager),
                $"{typeof(ExpressionReflectionManager).FullName}.{nameof(AddCompiledMemberSetter)}<{delegateType.GetPrettyName()}>(typeof({reflectedType.GetPrettyName()}), \"{member.Name}\", {(member is PropertyInfo ? "true" : "false")}, {setter});");
        }

        private static Type GetReflectedType(MemberInfo member)
        {
            try
            {
                var p = member.GetType().GetPropertyEx("ReflectedType", MemberFlags.Instance | MemberFlags.Public);
                if (p == null)
                    return member.DeclaringType;
                return (Type)p.GetValue(member, null);
            }
            catch (Exception)
            {
                return member.DeclaringType;
            }
        }

        private static string GenerateParameterTypes(ParameterInfo[] parameters)
        {
            if (parameters.Length > 0)
                return ", " + string.Join(",", parameters.Select(info => $"typeof({info.ParameterType.GetPrettyName()})"));
            return $", {typeof(Empty).FullName}.{nameof(Empty.Array)}<{typeof(Type).FullName}>()";
        }

        protected virtual Delegate TryCreateDelegateInternal(Type delegateType, object target, MethodInfo method)
        {
            MethodInfo result;
            lock (CachedDelegates)
            {
                var cacheKey = new MethodDelegateCacheKey(method, delegateType);
                if (!CachedDelegates.TryGetValue(cacheKey, out result))
                {
                    result = TryCreateMethodDelegate(delegateType, method);
                    CachedDelegates[cacheKey] = result;
                    if (result != null)
                        GenerateDelegateFactoryCode(delegateType, result);
                }
                if (result == null)
                    return null;
            }
#if NET_STANDARD
            if (target == null)
                return result.CreateDelegate(delegateType);
            return result.CreateDelegate(delegateType, target);
#else
            if (target == null)
                return Delegate.CreateDelegate(delegateType, result);
            return Delegate.CreateDelegate(delegateType, target, result);
#endif
        }

        protected virtual Func<object[], object> GetActivatorDelegateInternal(ConstructorInfo constructor)
        {
            lock (ActivatorCache)
            {
                Func<object[], object> value;
                if (!ActivatorCache.TryGetValue(constructor, out value))
                {
                    GenerateActivatorCode(constructor);
                    ParameterExpression parameterExpression;
                    Expression[] expressions = GetParametersExpression(constructor, out parameterExpression);
                    Expression newExpression = ConvertIfNeed(Expression.New(constructor, expressions), typeof(object), false);
                    value = Expression.Lambda<Func<object[], object>>(newExpression, parameterExpression).Compile();
                }
                ActivatorCache[constructor] = value;
                return value;
            }
        }

        protected virtual Func<object, object[], object> GetMethodDelegateInternal(MethodInfo method)
        {
            lock (InvokeMethodCache)
            {
                Func<object, object[], object> value;
                if (!InvokeMethodCache.TryGetValue(method, out value))
                {
                    GenerateInvokeMethodCode(method);
                    value = CreateMethodInvoke(method);
                    InvokeMethodCache[method] = value;
                }
                return value;
            }
        }

        protected virtual Delegate GetMethodDelegateInternal(Type delegateType, MethodInfo method)
        {
            lock (InvokeMethodCacheDelegate)
            {
                var cacheKey = new MethodDelegateCacheKey(method, delegateType);
                Delegate value;
                if (!InvokeMethodCacheDelegate.TryGetValue(cacheKey, out value))
                {
                    GenerateInvokeMethodCode(method, delegateType);
                    MethodInfo delegateMethod = delegateType.GetMethodEx(nameof(Action.Invoke));
                    if (delegateMethod == null)
                        throw new ArgumentException(string.Empty, nameof(delegateType));

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
                    var lambdaExpression = Expression.Lambda(delegateType, callExpression, parameters);
                    value = lambdaExpression.Compile();
                    InvokeMethodCacheDelegate[cacheKey] = value;
                }
                return value;
            }
        }

        protected virtual Func<object, TType> GetMemberGetterInternal<TType>(MemberInfo member)
        {
            var key = new MemberInfoDelegateCacheKey(member, typeof(TType));
            lock (MemberGetterCache)
            {
                Delegate value;
                if (!MemberGetterCache.TryGetValue(key, out value))
                {
                    GenerateGetterCode(member, typeof(TType));
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
                    MemberGetterCache[key] = value;
                }
                return (Func<object, TType>)value;
            }
        }

        protected virtual Action<object, TType> GetMemberSetterInternal<TType>(MemberInfo member)
        {
            var key = new MemberInfoDelegateCacheKey(member, typeof(TType));
            lock (MemberSetterCache)
            {
                Delegate action;
                if (!MemberSetterCache.TryGetValue(key, out action))
                {
                    var declaringType = member.DeclaringType;
                    var fieldInfo = member as FieldInfo;
#if NET_STANDARD
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
                        MemberSetterCache[key] = result;
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
                        expression = Expression.Call(setMethod.IsStatic ? null : ConvertIfNeed(target, declaringType, false), setMethod, valueExpression);
                        GenerateSetterCode(member, typeof(TType), propertyInfo.PropertyType);
                    }
                    else
                    {
                        expression = Expression.Field(fieldInfo.IsStatic ? null : ConvertIfNeed(target, declaringType, false), fieldInfo);
                        expression = Expression.Assign(expression, ConvertIfNeed(valueParameter, fieldInfo.FieldType, false));
                        GenerateSetterCode(member, typeof(TType), fieldInfo.FieldType);
                    }
                    action = Expression
                        .Lambda<Action<object, TType>>(expression, targetParameter, valueParameter)
                        .Compile();
                    MemberSetterCache[key] = action;
                }
                return (Action<object, TType>)action;
            }
        }

        protected static MethodInfo TryCreateMethodDelegate(Type eventHandlerType, MethodInfo method)
        {
            if (!typeof(Delegate).IsAssignableFrom(eventHandlerType))
                return null;

            ParameterInfo[] mParameters = method.GetParameters();
            ParameterInfo[] eParameters = eventHandlerType.GetMethodEx(nameof(Action.Invoke)).GetParameters();
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
#if NET_STANDARD
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
                return method != null && method.IsStatic;
            }
            var methodInfo = member as MethodInfo;
            if (methodInfo != null)
                return methodInfo.IsStatic;
            var fieldInfo = member as FieldInfo;
            return fieldInfo != null && fieldInfo.IsStatic;
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

#if NET_STANDARD
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