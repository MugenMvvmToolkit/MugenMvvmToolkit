#region Copyright
// ****************************************************************************
// <copyright file="ReflectionExtensions.cs">
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
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Validation;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

// ReSharper disable once CheckNamespace
namespace MugenMvvmToolkit
{
    /// <summary>
    ///     Represents the reflection extensions.
    /// </summary>
    public static class ReflectionExtensions
    {
        #region Nested types

        /// <summary>
        /// Represents the weak reference event wrapper.
        /// </summary>
        public interface IWeakEventHandler<in TArg>
        {
            /// <summary>
            /// Invokes the event.
            /// </summary>
            void Handle(object sender, TArg arg);
        }

        private sealed class WeakEventHandler<TTarget, TArg, TDelegate> : IWeakEventHandler<TArg>
            where TTarget : class
            where TDelegate : class
        {
            #region Fields

            public TDelegate HandlerDelegate;
            private readonly WeakReference _targetReference;
            private readonly Action<TTarget, object, TArg> _invokeAction;
            private readonly Action<object, TDelegate> _unsubscribeAction;

            #endregion

            #region Constructors

            public WeakEventHandler(TTarget target, Action<TTarget, object, TArg> invokeAction, Action<object, TDelegate> unsubscribeAction)
            {
                Should.NotBeNull(target, "target");
                Should.NotBeNull(invokeAction, "invokeAction");
                _invokeAction = invokeAction;
                _unsubscribeAction = unsubscribeAction;
                _targetReference = MvvmExtensions.GetWeakReference(target);
            }

            #endregion

            #region Methods

            public void Handle(object sender, TArg arg)
            {
                var target = (TTarget)_targetReference.Target;
                if (target == null)
                {
                    if (_unsubscribeAction != null)
                        _unsubscribeAction.Invoke(sender, HandlerDelegate);
                }
                else
                    _invokeAction(target, sender, arg);
            }

            #endregion
        }

        #endregion

        #region Fields

        private static readonly Action<object, PropertyChangedEventHandler> UnsubscribePropertyChangedDelegate;
        private static readonly Action<object, EventHandler<DataErrorsChangedEventArgs>> UnsubscribeErrorsChangedDelegate;
        private static readonly Func<IWeakEventHandler<PropertyChangedEventArgs>, PropertyChangedEventHandler> CreatePropertyChangedHandlerDelegate;
        private static readonly Func<IWeakEventHandler<DataErrorsChangedEventArgs>, EventHandler<DataErrorsChangedEventArgs>> CreateErrorsChangedHandlerDelegate;

        private static readonly Dictionary<MemberInfo, Attribute[]> CachedAttributes;
        private static readonly Dictionary<Type, Func<object, object>> GetDataContextDelegateCache;
        private static readonly Dictionary<Type, Action<object, object>> SetDataContextDelegateCache;

        #endregion

        #region Constructors

        static ReflectionExtensions()
        {
            CreatePropertyChangedHandlerDelegate = CreateHandler;
            CreateErrorsChangedHandlerDelegate = CreateHandler;
            UnsubscribePropertyChangedDelegate = UnsubscribePropertyChanged;
            UnsubscribeErrorsChangedDelegate = UnsubscribeErrorsChanged;
            CachedAttributes = new Dictionary<MemberInfo, Attribute[]>();
            GetDataContextDelegateCache = new Dictionary<Type, Func<object, object>>();
            SetDataContextDelegateCache = new Dictionary<Type, Action<object, object>>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the delegate to get types.
        /// </summary>
        [CanBeNull]
        public static Func<Assembly, Type[]> GetTypesDefault { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///     Returns a weak-reference version of a delegate.
        /// </summary>
        public static IWeakEventHandler<TArg> CreateWeakEventHandler<TTarget, TArg>([NotNull] TTarget target, [NotNull]Action<TTarget, object, TArg> invokeAction)
            where TTarget : class
        {
            return new WeakEventHandler<TTarget, TArg, object>(target, invokeAction, null);
        }

        /// <summary>
        ///     Returns a weak-reference version of a delegate.
        /// </summary>
        public static TResult CreateWeakDelegate<TTarget, TArg, TResult>([NotNull] TTarget target,
            [NotNull]Action<TTarget, object, TArg> invokeAction, [CanBeNull] Action<object, TResult> unsubscribeAction,
            [NotNull] Func<IWeakEventHandler<TArg>, TResult> createHandler)
            where TTarget : class
            where TResult : class
        {
            Should.NotBeNull(createHandler, "createHandler");
            var weakEventHandler = new WeakEventHandler<TTarget, TArg, TResult>(target, invokeAction, unsubscribeAction);
            var handler = createHandler(weakEventHandler);
            if (unsubscribeAction == null)
                return handler;
            weakEventHandler.HandlerDelegate = handler;
            return handler;
        }

        /// <summary>
        ///     Returns a weak-reference version of a delegate.
        /// </summary>
        public static PropertyChangedEventHandler MakeWeakPropertyChangedHandler<TTarget>([NotNull]TTarget target,
            [NotNull]Action<TTarget, object, PropertyChangedEventArgs> invokeAction)
            where TTarget : class
        {
            return CreateWeakDelegate(target, invokeAction, UnsubscribePropertyChangedDelegate, CreatePropertyChangedHandlerDelegate);
        }

        /// <summary>
        ///     Returns a weak-reference version of a delegate.
        /// </summary>
        public static EventHandler<DataErrorsChangedEventArgs> MakeWeakErrorsChangedHandler<TTarget>([NotNull]TTarget target,
            [NotNull]Action<TTarget, object, DataErrorsChangedEventArgs> invokeAction)
            where TTarget : class
        {
            return CreateWeakDelegate(target, invokeAction, UnsubscribeErrorsChangedDelegate, CreateErrorsChangedHandlerDelegate);
        }

        /// <summary>
        /// Gets the types defined in this assembly.
        /// </summary>
        public static IList<Type> SafeGetTypes(this Assembly assembly, bool throwOnError)
        {
            try
            {
                var func = GetTypesDefault;
                if (func != null)
                    return func(assembly);
#if PCL_WINRT
                return assembly.DefinedTypes.Select(info => info.AsType()).ToList();
#else
                return assembly.GetTypes();
#endif
            }
            catch (ReflectionTypeLoadException e)
            {
                if (throwOnError)
                    throw;
                Tracer.Error("SafeGetTypes {0} - error {1}", assembly.FullName, e.Flatten(true));
            }
            return EmptyValue<Type>.ListInstance;
        }

        /// <summary>
        ///     Invokes the constructor using the current <see cref="IReflectionManager" />.
        /// </summary>
        public static object InvokeEx([NotNull] this ConstructorInfo constructor)
        {
            return constructor.InvokeEx(EmptyValue<object>.ArrayInstance);
        }

        /// <summary>
        ///     Invokes the constructor using the current <see cref="IReflectionManager" />.
        /// </summary>
        public static object InvokeEx([NotNull] this ConstructorInfo constructor, params object[] parameters)
        {
            return ServiceProvider.ReflectionManager.GetActivatorDelegate(constructor).Invoke(parameters);
        }

        /// <summary>
        ///     Invokes the method using the current <see cref="IReflectionManager" />.
        /// </summary>
        public static object InvokeEx([NotNull] this MethodInfo method, object target)
        {
            return method.InvokeEx(target, EmptyValue<object>.ArrayInstance);
        }

        /// <summary>
        ///     Invokes the method using the current <see cref="IReflectionManager" />.
        /// </summary>
        public static object InvokeEx([NotNull] this MethodInfo method, object target, params object[] parameters)
        {
            return ServiceProvider.ReflectionManager.GetMethodDelegate(method).Invoke(target, parameters);
        }

        /// <summary>
        ///     Gets the value using the current <see cref="IReflectionManager" />.
        /// </summary>
        public static T GetValueEx<T>([NotNull] this MemberInfo member, object target)
        {
            return ServiceProvider.ReflectionManager.GetMemberGetter<T>(member).Invoke(target);
        }

        /// <summary>
        ///     Sets the value using the current <see cref="IReflectionManager" />.
        /// </summary>
        public static void SetValueEx<T>([NotNull] this MemberInfo member, object target, T value)
        {
            ServiceProvider.ReflectionManager.GetMemberSetter<T>(member).Invoke(target, value);
        }

        /// <summary>
        /// Tries to get data context.
        /// </summary>
        public static object GetDataContext(object item)
        {
            Should.NotBeNull(item, "item");
            var type = item.GetType();
            Func<object, object> func;
            lock (GetDataContextDelegateCache)
            {
                if (!GetDataContextDelegateCache.TryGetValue(type, out func))
                {
                    var property = type.GetPropertyEx("DataContext", MemberFlags.Public | MemberFlags.Instance);
                    if (property != null && property.CanRead)
                        func = ServiceProvider.ReflectionManager.GetMemberGetter<object>(property);
                    GetDataContextDelegateCache[type] = func;
                }
            }
            if (func == null)
                return null;
            return func(item);
        }

        /// <summary>
        /// Tries to set data context.
        /// </summary>
        public static bool SetDataContext(object item, object dataContext)
        {
            Should.NotBeNull(item, "item");
            var type = item.GetType();
            Action<object, object> setter;
            lock (SetDataContextDelegateCache)
            {
                if (!SetDataContextDelegateCache.TryGetValue(type, out setter))
                {
                    var property = type.GetPropertyEx("DataContext", MemberFlags.Public | MemberFlags.Instance);
                    if (property != null && property.CanWrite)
                        setter = ServiceProvider.ReflectionManager.GetMemberSetter<object>(property);
                    SetDataContextDelegateCache[type] = setter;
                }
            }
            if (setter == null)
                return false;
            setter(item, dataContext);
            return true;
        }

        internal static Attribute[] GetAttributes([NotNull] this MemberInfo member)
        {
            Should.NotBeNull(member, "member");
            Attribute[] attributes;
            lock (CachedAttributes)
            {
                if (!CachedAttributes.TryGetValue(member, out attributes))
                {
                    attributes = member.GetCustomAttributes(typeof(Attribute), true)
                        .OfType<Attribute>()
                        .ToArray();
                    CachedAttributes[member] = attributes;
                }
            }
            return attributes;
        }

        private static void UnsubscribePropertyChanged(object sender, PropertyChangedEventHandler handler)
        {
            var notifyPropertyChanged = sender as INotifyPropertyChanged;
            if (notifyPropertyChanged != null)
                notifyPropertyChanged.PropertyChanged -= handler;
        }

        private static void UnsubscribeErrorsChanged(object o, EventHandler<DataErrorsChangedEventArgs> eventHandler)
        {
            var notifyDataErrorInfo = o as INotifyDataErrorInfo;
            if (notifyDataErrorInfo != null)
                notifyDataErrorInfo.ErrorsChanged -= eventHandler;
        }

        private static PropertyChangedEventHandler CreateHandler(IWeakEventHandler<PropertyChangedEventArgs> weakEventHandler)
        {
            return weakEventHandler.Handle;
        }

        private static EventHandler<DataErrorsChangedEventArgs> CreateHandler(IWeakEventHandler<DataErrorsChangedEventArgs> weakEventHandler)
        {
            return weakEventHandler.Handle;
        }

#if PCL_WINRT
        [CanBeNull]
        public static PropertyInfo GetPropertyEx([NotNull] this Type type, string name, MemberFlags flags = MemberFlags.Public | MemberFlags.Static | MemberFlags.Instance)
        {
            Should.NotBeNull(type, "type");
            var properties = type.GetPropertiesEx(flags);
            for (int index = 0; index < properties.Length; index++)
            {
                var property = properties[index];
                if (property.Name == name)
                    return property;
            }
            return null;
        }

        [NotNull]
        public static PropertyInfo[] GetPropertiesEx([NotNull] this Type type, MemberFlags flags = MemberFlags.Public | MemberFlags.Static | MemberFlags.Instance)
        {
            Should.NotBeNull(type, "type");
            //BUG: GetRuntimeProperties returns only declared static properties instead of all
            var list = new List<PropertyInfo>();
            while (type != null && type != typeof(object))
            {
                var typeInfo = type.GetTypeInfo();
                foreach (var property in typeInfo.DeclaredProperties)
                {
                    if (FilterProperty(property, flags))
                        list.Add(property);
                }
                type = typeInfo.BaseType;
            }
            return list.ToArray();
        }

        [CanBeNull]
        public static FieldInfo GetFieldEx([NotNull] this Type type, string name, MemberFlags flags = MemberFlags.Public | MemberFlags.Static | MemberFlags.Instance)
        {
            Should.NotBeNull(type, "type");
            var fields = type.GetFieldsEx(flags);
            for (int index = 0; index < fields.Length; index++)
            {
                var field = fields[index];
                if (field.Name == name)
                    return field;
            }
            return null;
        }

        [NotNull]
        public static FieldInfo[] GetFieldsEx([NotNull] this Type type, MemberFlags flags = MemberFlags.Public | MemberFlags.Static | MemberFlags.Instance)
        {
            Should.NotBeNull(type, "type");
            var list = new List<FieldInfo>();
            while (type != null && type != typeof(object))
            {
                var typeInfo = type.GetTypeInfo();
                foreach (var field in typeInfo.DeclaredFields)
                {
                    if (FilterField(field, flags))
                        list.Add(field);
                }
                type = typeInfo.BaseType;
            }
            return list.ToArray();
            //BUG: GetRuntimeFields returns only declared fields
            //            return type.GetRuntimeFields().Where(info => FilterField(info, flags)).ToArray();
        }

        [CanBeNull]
        public static MethodInfo GetMethodEx([NotNull] this Type type, string name, Type[] types)
        {
            Should.NotBeNull(type, "type");
            return type.GetRuntimeMethod(name, types);
        }

        [CanBeNull]
        public static MethodInfo GetMethodEx([NotNull] this Type type, string name, MemberFlags flags = MemberFlags.Public | MemberFlags.Static | MemberFlags.Instance)
        {
            Should.NotBeNull(type, "type");
            MethodInfo result = null;
            var methods = type.GetMethodsEx(flags);
            for (int index = 0; index < methods.Length; index++)
            {
                var method = methods[index];
                if (method.Name == name)
                {
                    if (result != null)
                        throw new AmbiguousMatchException();
                    result = method;
                }
            }
            return result;
        }

        [NotNull]
        public static MethodInfo[] GetMethodsEx([NotNull] this Type type, MemberFlags flags = MemberFlags.Public | MemberFlags.Static | MemberFlags.Instance)
        {
            Should.NotBeNull(type, "type");
            var result = new List<MethodInfo>();
            foreach (var method in type.GetRuntimeMethods())
            {
                if (FilterMethod(method, flags))
                    result.Add(method);
            }
            return result.ToArray();
        }

        [CanBeNull]
        public static EventInfo GetEventEx(this Type sourceType, string eventName, MemberFlags flags = MemberFlags.Public | MemberFlags.Static | MemberFlags.Instance)
        {
            EventInfo @event = sourceType.GetRuntimeEvent(eventName);
            if (@event == null)
                return null;
            MethodInfo eventMethod = @event.AddMethod ?? @event.RemoveMethod;
            if (eventMethod == null || !FilterMethod(eventMethod, flags))
                return null;
            return @event;
        }

        [CanBeNull]
        internal static ConstructorInfo GetConstructor([NotNull] this Type type, Type[] types)
        {
            Should.NotBeNull(type, "type");
            foreach (ConstructorInfo constructor in type.GetTypeInfo().DeclaredConstructors)
            {
                if (constructor.IsStatic)
                    continue;
                ParameterInfo[] constParams = constructor.GetParameters();
                if (types.Length != constParams.Length) continue;
                bool find = true;
                for (int i = 0; i < constParams.Length; i++)
                {
                    if (constParams[i].ParameterType != types[i])
                    {
                        find = false;
                        break;
                    }
                }
                if (!find) continue;
                return constructor;
            }
            return null;
        }

        internal static bool IsDefined(this Type type, Type attributeType, bool inherit)
        {
            Should.NotBeNull(type, "type");
            return type.GetTypeInfo().IsDefined(attributeType, inherit);
        }

        internal static bool IsInstanceOfType(this Type type, object instance)
        {
            if (instance == null)
                return false;
            return type.IsAssignableFrom(instance.GetType());
        }

        internal static bool IsAssignableFrom([NotNull] this Type typeFrom, [NotNull] Type typeTo)
        {
            Should.NotBeNull(typeFrom, "typeFrom");
            Should.NotBeNull(typeTo, "typeTo");
            return typeFrom.GetTypeInfo().IsAssignableFrom(typeTo.GetTypeInfo());
        }

        internal static IEnumerable<Type> GetInterfaces([NotNull] this Type type)
        {
            Should.NotBeNull(type, "type");
            return type.GetTypeInfo().ImplementedInterfaces;
        }

        internal static Attribute[] GetAttributes([NotNull] this Type type)
        {
            return type.GetTypeInfo().GetAttributes();
        }

        internal static Type[] GetGenericArguments(this Type typeInfo)
        {
            return typeInfo.GenericTypeArguments;
        }

        internal static Assembly GetAssembly(this Type type)
        {
            return type.GetTypeInfo().Assembly;
        }

        internal static MethodInfo GetGetMethod(this PropertyInfo property, bool nonPublic)
        {
            var method = property.GetMethod;
            if (nonPublic)
                return method;
            return method.IsPublic ? method : null;
        }

        internal static MethodInfo GetSetMethod(this PropertyInfo property, bool nonPublic)
        {
            var method = property.SetMethod;
            if (nonPublic)
                return method;
            return method.IsPublic ? method : null;
        }

        private static bool FilterProperty(PropertyInfo property, MemberFlags flags)
        {
            if (property == null)
                return false;
            return FilterMethod(property.CanRead ? property.GetMethod : property.SetMethod, flags);
        }

        private static bool FilterField(FieldInfo field, MemberFlags flags)
        {
            if (field == null)
                return false;
            if (flags.HasMemberFlag(MemberFlags.Static) && !flags.HasMemberFlag(MemberFlags.Instance) && !field.IsStatic)
                return false;
            if (flags.HasMemberFlag(MemberFlags.NonPublic))
                return true;
            return field.IsPublic;
        }

        private static bool FilterMethod(MethodBase method, MemberFlags flags)
        {
            if (method == null)
                return false;
            if (flags.HasMemberFlag(MemberFlags.Static) && !flags.HasMemberFlag(MemberFlags.Instance) && !method.IsStatic)
                return false;
            if (flags.HasMemberFlag(MemberFlags.NonPublic))
                return true;
            return method.IsPublic;
        }
#else
        /// <summary>
        /// Gets an object that represents the method represented by the specified delegate.
        /// </summary>        
        /// <returns>
        /// An object that represents the method.
        /// </returns>
        /// <param name="del">The delegate to examine.</param>
        public static MethodInfo GetMethodInfo([NotNull] this Delegate del)
        {
            Should.NotBeNull(del, "del");
            return del.Method;
        }

        [CanBeNull]
        public static PropertyInfo GetPropertyEx([NotNull] this Type type, string name, MemberFlags flags = MemberFlags.Public | MemberFlags.Static | MemberFlags.Instance)
        {
            Should.NotBeNull(type, "type");
            return type.GetProperty(name, flags.ToBindingFlags());
        }

        [NotNull]
        public static PropertyInfo[] GetPropertiesEx([NotNull] this Type type, MemberFlags flags = MemberFlags.Public | MemberFlags.Static | MemberFlags.Instance)
        {
            Should.NotBeNull(type, "type");
            return type.GetProperties(flags.ToBindingFlags());
        }

        [CanBeNull]
        public static FieldInfo GetFieldEx([NotNull] this Type type, string name, MemberFlags flags = MemberFlags.Public | MemberFlags.Static | MemberFlags.Instance)
        {
            Should.NotBeNull(type, "type");
            return type.GetField(name, flags.ToBindingFlags());
        }

        [NotNull]
        public static FieldInfo[] GetFieldsEx([NotNull] this Type type, MemberFlags flags = MemberFlags.Public | MemberFlags.Static | MemberFlags.Instance)
        {
            Should.NotBeNull(type, "type");
            return type.GetFields(flags.ToBindingFlags());
        }

        [CanBeNull]
        public static MethodInfo GetMethodEx([NotNull] this Type type, string name, Type[] types)
        {
            Should.NotBeNull(type, "type");
            return type.GetMethod(name, types);
        }

        [CanBeNull]
        public static MethodInfo GetMethodEx([NotNull] this Type type, string name, MemberFlags flags = MemberFlags.Public | MemberFlags.Static | MemberFlags.Instance)
        {
            Should.NotBeNull(type, "type");
            return type.GetMethod(name, flags.ToBindingFlags());
        }

        [NotNull]
        public static MethodInfo[] GetMethodsEx([NotNull] this Type type, MemberFlags flags = MemberFlags.Public | MemberFlags.Static | MemberFlags.Instance)
        {
            Should.NotBeNull(type, "type");
            return type.GetMethods(flags.ToBindingFlags());
        }

        [CanBeNull]
        public static EventInfo GetEventEx(this Type sourceType, string eventName, MemberFlags flags = MemberFlags.Public | MemberFlags.Static | MemberFlags.Instance)
        {
            return sourceType.GetEvent(eventName, flags.ToBindingFlags());
        }

        internal static Assembly GetAssembly(this Type type)
        {
            return type.Assembly;
        }

        private static BindingFlags ToBindingFlags(this MemberFlags flags)
        {
            BindingFlags result = default(BindingFlags);
            if (flags.HasMemberFlag(MemberFlags.Instance))
                result |= BindingFlags.Instance;
            if (flags.HasMemberFlag(MemberFlags.Static))
                result |= BindingFlags.Static;
            if (flags.HasMemberFlag(MemberFlags.NonPublic))
                result |= BindingFlags.NonPublic;
            if (flags.HasMemberFlag(MemberFlags.Public))
                result |= BindingFlags.Public;
            return result | BindingFlags.FlattenHierarchy;
        }
#endif
        #endregion
    }
}