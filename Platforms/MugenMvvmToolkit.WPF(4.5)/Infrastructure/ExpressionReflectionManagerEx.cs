#region Copyright

// ****************************************************************************
// <copyright file="ExpressionReflectionManagerEx.cs">
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
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using MugenMvvmToolkit.Infrastructure;

#if WPF
namespace MugenMvvmToolkit.WPF.Infrastructure
#elif ANDROID
namespace MugenMvvmToolkit.Android.Infrastructure
#elif WINFORMS
namespace MugenMvvmToolkit.WinForms.Infrastructure
#elif SILVERLIGHT
namespace MugenMvvmToolkit.Silverlight.Infrastructure
#elif WINDOWS_PHONE
namespace MugenMvvmToolkit.WinPhone.Infrastructure
#endif
{
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

        public override Delegate TryCreateDelegate(Type delegateType, object target, MethodInfo method)
        {
            Should.NotBeNull(delegateType, nameof(delegateType));
            Should.NotBeNull(method, nameof(method));
            Func<object, Delegate> value;
            lock (DelegatesCache)
            {
                var key = new MethodDelegateCacheKey(method, delegateType);
                if (!DelegatesCache.TryGetValue(key, out value))
                {
                    method = TryCreateMethodDelegate(delegateType, method);
                    if (method != null)
                    {
                        Type type = method.DeclaringType;
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

        private static void UnboxOrCast(ILGenerator il, Type type)
        {
            Type elementType = null;
            if (type.IsByRef)
                elementType = type.GetElementType();
            if (elementType == null)
                elementType = type;
            il.Emit(elementType.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, elementType);
        }

        private static DynamicMethod CreateDynamicMethod(Type type, Type[] inputValue, Type outputValue)
        {
            if (type == null)
                type = typeof(object);
#if SILVERLIGHT
            return new DynamicMethod("dynamic_" + type.Name + Guid.NewGuid().ToString("N"), outputValue, inputValue);
#else
            return new DynamicMethod("dynamic_" + type.Name + Guid.NewGuid().ToString("N"), outputValue, inputValue, type.Module, true);
#endif
        }

        #endregion
    }
}
