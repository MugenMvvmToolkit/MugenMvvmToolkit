#region Copyright
// ****************************************************************************
// <copyright file="ExpressionReflectionManagerEx.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace MugenMvvmToolkit.Infrastructure
{
    /// <summary>
    ///     Represents the reflection access provider that uses the <see cref="Expression" />.
    /// </summary>
    public class ExpressionReflectionManagerEx : ExpressionReflectionManager
    {
        #region Fields

        private static readonly Dictionary<MethodDelegateCacheKey, Func<object, Delegate>> DelegatesCache;

        #endregion

        #region Constructors

        static ExpressionReflectionManagerEx()
        {
            DelegatesCache = new Dictionary<MethodDelegateCacheKey, Func<object, Delegate>>(MethodDelegateCacheKeyComparer.Instance);
        }

        #endregion

        #region Overrides of ExpressionReflectionProvider

        /// <summary>
        ///     Tries to creates a delegate of the specified type that represents the specified static or instance method, with the
        ///     specified first argument.
        /// </summary>
        /// <returns>
        ///     A delegate of the specified type that represents the specified static method of the specified class.
        /// </returns>
        /// <param name="delegateType">The <see cref="T:System.Type" /> of delegate to create. </param>
        /// <param name="target">
        ///     The <see cref="T:System.Type" /> representing the class that implements <paramref name="method" />
        ///     .
        /// </param>
        /// <param name="method">The name of the static method that the delegate is to represent. </param>
        public override Delegate TryCreateDelegate(Type delegateType, object target, MethodInfo method)
        {
            Func<object, Delegate> value;
            lock (DelegatesCache)
            {
                var key = new MethodDelegateCacheKey(method, delegateType);
                if (!DelegatesCache.TryGetValue(key, out value))
                {
                    method = TryCreateMethodDelegate(delegateType, method);
                    if (method != null)
                    {
                        Type type = method.DeclaringType ?? method.ReflectedType;
                        DynamicMethod dynamicMethod = CreateDynamicMethod(type, new[] { typeof(object) },
                            typeof(Delegate));
                        ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
                        if (method.IsStatic)
                            ilGenerator.Emit(OpCodes.Ldnull);
                        else
                        {
                            ilGenerator.Emit(OpCodes.Ldarg_0);
                            UnboxOrCast(ilGenerator, type);
                        }
                        if (method.IsVirtual)
                        {
                            ilGenerator.Emit(OpCodes.Dup);
                            ilGenerator.Emit(OpCodes.Ldvirtftn, method);
                        }
                        else
                            ilGenerator.Emit(OpCodes.Ldftn, method);
                        ilGenerator.Emit(OpCodes.Newobj, delegateType.GetConstructors()[0]);
                        ilGenerator.Emit(OpCodes.Ret);
                        value = (Func<object, Delegate>)dynamicMethod.CreateDelegate(typeof(Func<object, Delegate>));
                    }
                    DelegatesCache[key] = value;
                }
            }
            if (value == null)
                return null;
            return value(target);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Unbox or cast specified type.
        /// </summary>
        private static void UnboxOrCast(ILGenerator il, Type type)
        {
            Type elementType = null;
            if (type.IsByRef)
                elementType = type.GetElementType();
            if (elementType == null)
                elementType = type;
            il.Emit(elementType.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, elementType);
        }

        /// <summary>
        ///     Creates dynamic method with skip visibility.
        /// </summary>
        private static DynamicMethod CreateDynamicMethod(Type type, Type[] inputValue, Type outputValue)
        {
            if (type == null)
                type = typeof(object);
#if WPF
#if NETFX_CORE
            var typeInfo = type.GetTypeInfo();
#else
            Type typeInfo = type;
#endif
            return
                new DynamicMethod("dynamic_" + type.Name + Guid.NewGuid().ToString("N"), outputValue,
                    inputValue, typeInfo.Module, true);
#else
            return new DynamicMethod("dynamic_" + type.Name + Guid.NewGuid().ToString("N"),
                outputValue,
                inputValue);
#endif
        }

        #endregion
    }
}