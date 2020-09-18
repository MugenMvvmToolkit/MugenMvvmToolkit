﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        #region Fields

        private static readonly Dictionary<Type, bool> HasClosureDictionary = new Dictionary<Type, bool>(47, InternalEqualityComparer.Type);

        #endregion

        #region Properties

        public static Func<Delegate, bool> ClosureDetector { get; set; } = DefaultClosureDetector;

        #endregion

        #region Methods

        public static bool HasClosure(this Delegate d)
        {
            if (d.Target == null)
                return false;
            return ClosureDetector(d);
        }

        public static Delegate CompileEx(this LambdaExpression lambdaExpression)
        {
            var compiler = MugenService.Optional<ILambdaExpressionCompiler>();
            if (compiler == null)
                return lambdaExpression.Compile();
            return compiler.Compile(lambdaExpression);
        }

        public static TDelegate CompileEx<TDelegate>(this Expression<TDelegate> lambdaExpression) where TDelegate : Delegate
        {
            var compiler = MugenService.Optional<ILambdaExpressionCompiler>();
            if (compiler == null)
                return lambdaExpression.Compile();
            return compiler.Compile(lambdaExpression);
        }

        public static bool IsStatic(this MemberInfo member)
        {
            Should.NotBeNull(member, nameof(member));
            if (member is PropertyInfo propertyInfo)
            {
                var method = propertyInfo.CanRead
                    ? propertyInfo.GetGetMethod(true)
                    : propertyInfo.GetSetMethod(true);
                return method != null && method.IsStatic;
            }

            if (member is Type type)
                return type.IsAbstract && type.IsSealed;

            if (member is EventInfo eventInfo)
            {
                var method = eventInfo.AddMethod ?? eventInfo.RemoveMethod;
                return method != null && method.IsStatic;
            }

            if (member is MethodBase m)
                return m.IsStatic;
            return member is FieldInfo fieldInfo && fieldInfo.IsStatic;
        }

        public static bool IsAnonymousClass(this Type type) => type.IsDefined(typeof(CompilerGeneratedAttribute), false) && type.IsClass;

        public static MethodInfo GetMethodOrThrow(this Type type, string name, BindingFlags flags, Type[]? types = null)
        {
            var method = types == null ? type.GetMethod(name, flags) : type.GetMethod(name, flags, null, types, null);
            Should.BeSupported(method != null, type.Name + "." + name);
            return method;
        }

        public static Func<object?[], object> GetActivator(this IReflectionManager reflectionManager, ConstructorInfo constructor)
        {
            Should.NotBeNull(reflectionManager, nameof(reflectionManager));
            var result = reflectionManager.TryGetActivator(constructor);
            if (result == null)
                ExceptionManager.ThrowRequestNotSupported<IActivatorReflectionDelegateProviderComponent>(reflectionManager, constructor, null);
            return result;
        }

        public static Delegate GetActivator(this IReflectionManager reflectionManager, ConstructorInfo constructor, Type delegateType)
        {
            Should.NotBeNull(reflectionManager, nameof(reflectionManager));
            var result = reflectionManager.TryGetActivator(constructor, delegateType);
            if (result == null)
                ExceptionManager.ThrowRequestNotSupported<IActivatorReflectionDelegateProviderComponent>(reflectionManager, delegateType, null);
            return result;
        }

        public static Func<object?, object?[], object?> GetMethodInvoker(this IReflectionManager reflectionManager, MethodInfo method)
        {
            Should.NotBeNull(reflectionManager, nameof(reflectionManager));
            var result = reflectionManager.TryGetMethodInvoker(method);
            if (result == null)
                ExceptionManager.ThrowRequestNotSupported<IMethodReflectionDelegateProviderComponent>(reflectionManager, method, null);
            return result;
        }

        public static Delegate GetMethodInvoker(this IReflectionManager reflectionManager, MethodInfo method, Type delegateType)
        {
            Should.NotBeNull(reflectionManager, nameof(reflectionManager));
            var result = reflectionManager.TryGetMethodInvoker(method, delegateType);
            if (result == null)
                ExceptionManager.ThrowRequestNotSupported<IMethodReflectionDelegateProviderComponent>(reflectionManager, delegateType, null);
            return result;
        }

        public static Delegate GetMemberGetter(this IReflectionManager reflectionManager, MemberInfo member, Type delegateType)
        {
            Should.NotBeNull(reflectionManager, nameof(reflectionManager));
            var result = reflectionManager.TryGetMemberGetter(member, delegateType);
            if (result == null)
                ExceptionManager.ThrowRequestNotSupported<IMemberReflectionDelegateProviderComponent>(reflectionManager, delegateType, null);
            return result;
        }

        public static Delegate GetMemberSetter(this IReflectionManager reflectionManager, MemberInfo member, Type delegateType)
        {
            Should.NotBeNull(reflectionManager, nameof(reflectionManager));
            var result = reflectionManager.TryGetMemberSetter(member, delegateType);
            if (result == null)
                ExceptionManager.ThrowRequestNotSupported<IMemberReflectionDelegateProviderComponent>(reflectionManager, delegateType, null);
            return result;
        }

        public static bool CanCreateDelegate(this Type delegateType, MethodInfo method, IReflectionManager? reflectionManager = null) => reflectionManager.DefaultIfNull().CanCreateDelegate(delegateType, method);

        public static Delegate? TryCreateDelegate(this Type delegateType, object? target, MethodInfo method, IReflectionManager? reflectionManager = null) =>
            reflectionManager.DefaultIfNull().TryCreateDelegate(delegateType, target, method);

        public static Func<object?[], object> GetActivator(this ConstructorInfo constructor, IReflectionManager? reflectionManager = null) => reflectionManager.DefaultIfNull().GetActivator(constructor);

        public static TDelegate GetActivator<TDelegate>(this ConstructorInfo constructor, IReflectionManager? reflectionManager = null)
            where TDelegate : Delegate =>
            (TDelegate) reflectionManager.DefaultIfNull().GetActivator(constructor, typeof(TDelegate));

        public static Delegate GetActivator(this ConstructorInfo constructor, Type delegateType, IReflectionManager? reflectionManager = null) => reflectionManager.DefaultIfNull().GetActivator(constructor, delegateType);

        public static TDelegate GetMethodInvoker<TDelegate>(this MethodInfo method, IReflectionManager? reflectionManager = null)
            where TDelegate : Delegate =>
            (TDelegate) reflectionManager.DefaultIfNull().GetMethodInvoker(method, typeof(TDelegate));

        public static Delegate GetMethodInvoker(this MethodInfo method, Type delegateType, IReflectionManager? reflectionManager = null) => reflectionManager.DefaultIfNull().GetMethodInvoker(method, delegateType);

        public static Func<object?, object?[], object?> GetMethodInvoker(this MethodInfo method, IReflectionManager? reflectionManager = null) => reflectionManager.DefaultIfNull().GetMethodInvoker(method);

        public static TDelegate GetMemberGetter<TDelegate>(this MemberInfo member, IReflectionManager? reflectionManager = null) where TDelegate : Delegate =>
            (TDelegate) reflectionManager.DefaultIfNull().GetMemberGetter(member, typeof(TDelegate));

        public static TDelegate GetMemberSetter<TDelegate>(this MemberInfo member, IReflectionManager? reflectionManager = null) where TDelegate : Delegate =>
            (TDelegate) reflectionManager.DefaultIfNull().GetMemberSetter(member, typeof(TDelegate));

        public static Func<TTarget, TType> GetMemberGetter<TTarget, TType>(this MemberInfo member, IReflectionManager? reflectionManager = null) =>
            (Func<TTarget, TType>) reflectionManager.DefaultIfNull().GetMemberGetter(member, typeof(Func<TTarget, TType>));

        public static Action<TTarget, TType> GetMemberSetter<TTarget, TType>(this MemberInfo member, IReflectionManager? reflectionManager = null) =>
            (Action<TTarget, TType>) reflectionManager.DefaultIfNull().GetMemberSetter(member, typeof(Action<TTarget, TType>));

        private static bool DefaultClosureDetector(Delegate d)
        {
            var key = d.Target!.GetType();
            lock (HasClosureDictionary)
            {
                if (!HasClosureDictionary.TryGetValue(key, out var value))
                {
                    value = key.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Length != 0 ||
                            key.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Length != 0;
                    HasClosureDictionary[key] = value;
                }

                return value;
            }
        }

        #endregion
    }
}