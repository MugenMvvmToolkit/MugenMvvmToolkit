#region Copyright

// ****************************************************************************
// <copyright file="ReflectionExtensions.cs">
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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using JetBrains.Annotations;
using MugenMvvmToolkit.Attributes;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Validation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.ViewModels;

// ReSharper disable once CheckNamespace
namespace MugenMvvmToolkit
{
    public static class ReflectionExtensions
    {
        #region Nested types

        public interface IWeakEventHandler<in TArg>
        {
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
            private readonly Delegate _unsubscribeAction;

            #endregion

            #region Constructors

            public WeakEventHandler(TTarget target, Action<TTarget, object, TArg> invokeAction, Delegate unsubscribeAction)
            {
                Should.NotBeNull(target, nameof(target));
                Should.NotBeNull(invokeAction, nameof(invokeAction));
                _invokeAction = invokeAction;
                _unsubscribeAction = unsubscribeAction;
                _targetReference = ToolkitExtensions.GetWeakReference(target);
            }

            #endregion

            #region Methods

            public void Handle(object sender, TArg arg)
            {
                var target = (TTarget)_targetReference.Target;
                if (target == null)
                {
                    if (_unsubscribeAction != null)
                    {
                        var action = _unsubscribeAction as Action<object, TDelegate>;
                        if (action == null)
                            ((Action<object, IWeakEventHandler<TArg>>)_unsubscribeAction).Invoke(sender, this);
                        else
                            action.Invoke(sender, HandlerDelegate);
                    }
                }
                else
                    _invokeAction(target, sender, arg);
            }

            #endregion
        }

        #endregion

        #region Fields

        internal const string IndexerName = "Item[]";
        private const MemberFlags PropertyBindingFlag = MemberFlags.Instance | MemberFlags.NonPublic | MemberFlags.Public;

        private static readonly Action<object, PropertyChangedEventHandler> UnsubscribePropertyChangedDelegate;
        private static readonly Action<object, EventHandler<DataErrorsChangedEventArgs>> UnsubscribeErrorsChangedDelegate;
        private static readonly Action<object, NotifyCollectionChangedEventHandler> UnsubscribeCollectionChangedDelegate;
        private static readonly Func<IWeakEventHandler<NotifyCollectionChangedEventArgs>, NotifyCollectionChangedEventHandler> CreateCollectionChangedHandlerDelegate;
        private static readonly Func<IWeakEventHandler<PropertyChangedEventArgs>, PropertyChangedEventHandler> CreatePropertyChangedHandlerDelegate;
        private static readonly Func<IWeakEventHandler<DataErrorsChangedEventArgs>, EventHandler<DataErrorsChangedEventArgs>> CreateErrorsChangedHandlerDelegate;

        private static readonly Dictionary<MemberInfo, Attribute[]> CachedAttributes;
        private static readonly Dictionary<Type, Func<object, object>> GetDataContextDelegateCache;
        private static readonly Dictionary<Type, Action<object, object>> SetDataContextDelegateCache;
        private static readonly Dictionary<Delegate, MemberInfo> ExpressionToMemberInfoCache;

        private static readonly Func<Assembly, bool> IsToolkitAssemblyDelegate;
        private static readonly HashSet<string> KnownPublicKeys;
        private static readonly HashSet<string> KnownMSPublicKeys;
        private static readonly HashSet<string> KnownAssemblyName;

        private static readonly Dictionary<Type, string[]> CachedIgnoreAttributes;
        private static readonly Dictionary<Type, Dictionary<string, ICollection<string>>> CachedViewModelProperties;
        private static readonly IList<string> ExcludedProperties;
        private static readonly Dictionary<Type, Func<object, ICommand>[]> TypesToCommandsProperties;
        private static readonly Dictionary<Type, Action<object, IViewModel>> ViewToViewModelInterface;
        private static readonly Dictionary<Type, PropertyInfo> ViewModelToViewInterface;
        private static readonly Dictionary<Type, bool> HasClosureDictionary;

        #endregion

        #region Constructors

        static ReflectionExtensions()
        {
            CreatePropertyChangedHandlerDelegate = CreateHandler;
            CreateErrorsChangedHandlerDelegate = CreateHandler;
            CreateCollectionChangedHandlerDelegate = CreateHandler;
            UnsubscribeCollectionChangedDelegate = UnsubscribeCollectionChanged;
            UnsubscribePropertyChangedDelegate = UnsubscribePropertyChanged;
            UnsubscribeErrorsChangedDelegate = UnsubscribeErrorsChanged;
            HasClosureDictionary = new Dictionary<Type, bool>();
            CachedAttributes = new Dictionary<MemberInfo, Attribute[]>();
            GetDataContextDelegateCache = new Dictionary<Type, Func<object, object>>();
            SetDataContextDelegateCache = new Dictionary<Type, Action<object, object>>();
            ExpressionToMemberInfoCache = new Dictionary<Delegate, MemberInfo>(ReferenceEqualityComparer.Instance);
            //NOTE: 7cec85d7bea7798e, 31bf3856ad364e35, b03f5f7f11d50a3a, b77a5c561934e089 - NET FRAMEWORK
            //NOTE: 0738eb9f132ed756, 84e04ff9cfb79065 - MONO
            //NOTE: 5803cfa389c90ce7 - Telerik
            //NOTE: 17863af14b0044da - Autofac
            KnownMSPublicKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "7cec85d7bea7798e", "31bf3856ad364e35", "b03f5f7f11d50a3a",  "b77a5c561934e089"
            };
            KnownPublicKeys = new HashSet<string>(KnownMSPublicKeys, StringComparer.OrdinalIgnoreCase)
            {
                "0738eb9f132ed756", "84e04ff9cfb79065", "5803cfa389c90ce7", "17863af14b0044da"
            };
            KnownAssemblyName = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "FormsViewGroup",
                "Xamarin.Android.Support.v13",
                "Xamarin.Android.Support.v4",
                "Xamarin.Android.Support.v7.RecyclerView",
                "Xamarin.Android.Support.v7.AppCompat",
                "Xamarin.Android.Support.v7.CardView",
                "Xamarin.Android.Support.Design",
                "Xamarin.Forms.Core",
                "Xamarin.Forms.Platform",
                "Xamarin.Forms.Xaml",
                "Xamarin.Forms.Platform.Android",
                "Xamarin.Forms.Platform.iOS",
                "Xamarin.Forms.Platform.WP8"
            };
            IsToolkitAssemblyDelegate = IsToolkitAssembly;
            CachedViewModelProperties = new Dictionary<Type, Dictionary<string, ICollection<string>>>();
            CachedIgnoreAttributes = new Dictionary<Type, string[]>();
            ExcludedProperties = typeof(EditableViewModel<>)
               .GetPropertiesEx(PropertyBindingFlag)
               .Select(info => info.Name)
               .Concat(new[] { Empty.IndexerPropertyChangedArgs.PropertyName })
               .ToArray();
            TypesToCommandsProperties = new Dictionary<Type, Func<object, ICommand>[]>();
            ViewToViewModelInterface = new Dictionary<Type, Action<object, IViewModel>>();
            ViewModelToViewInterface = new Dictionary<Type, PropertyInfo>();
        }

        #endregion

        #region Properties

        [CanBeNull]
        public static Func<Assembly, Type[]> GetTypesDefault { get; set; }

        #endregion

        #region Methods

        public static IWeakEventHandler<TArg> CreateWeakEventHandler<TTarget, TArg>([NotNull] TTarget target, [NotNull]Action<TTarget, object, TArg> invokeAction,
            Action<object, IWeakEventHandler<TArg>> unsubscribeAction = null)
            where TTarget : class
        {
            return new WeakEventHandler<TTarget, TArg, object>(target, invokeAction, unsubscribeAction);
        }

        public static TResult CreateWeakDelegate<TTarget, TArg, TResult>([NotNull] TTarget target,
            [NotNull]Action<TTarget, object, TArg> invokeAction, [CanBeNull] Action<object, TResult> unsubscribeAction,
            [NotNull] Func<IWeakEventHandler<TArg>, TResult> createHandler)
            where TTarget : class
            where TResult : class
        {
            Should.NotBeNull(createHandler, nameof(createHandler));
            var weakEventHandler = new WeakEventHandler<TTarget, TArg, TResult>(target, invokeAction, unsubscribeAction);
            var handler = createHandler(weakEventHandler);
            if (unsubscribeAction == null)
                return handler;
            weakEventHandler.HandlerDelegate = handler;
            return handler;
        }

        public static PropertyChangedEventHandler MakeWeakPropertyChangedHandler<TTarget>([NotNull]TTarget target,
            [NotNull]Action<TTarget, object, PropertyChangedEventArgs> invokeAction)
            where TTarget : class
        {
            return CreateWeakDelegate(target, invokeAction, UnsubscribePropertyChangedDelegate, CreatePropertyChangedHandlerDelegate);
        }

        public static EventHandler<DataErrorsChangedEventArgs> MakeWeakErrorsChangedHandler<TTarget>([NotNull]TTarget target,
            [NotNull]Action<TTarget, object, DataErrorsChangedEventArgs> invokeAction)
            where TTarget : class
        {
            return CreateWeakDelegate(target, invokeAction, UnsubscribeErrorsChangedDelegate, CreateErrorsChangedHandlerDelegate);
        }

        public static NotifyCollectionChangedEventHandler MakeWeakCollectionChangedHandler<TTarget>(TTarget target, Action<TTarget, object,
            NotifyCollectionChangedEventArgs> invokeAction) where TTarget : class
        {
            return CreateWeakDelegate(target, invokeAction, UnsubscribeCollectionChangedDelegate, CreateCollectionChangedHandlerDelegate);
        }

        public static IList<Type> SafeGetTypes(this Assembly assembly, bool throwOnError)
        {
            try
            {
                var func = GetTypesDefault;
                if (func != null)
                    return func(assembly);
#if NET_STANDARD
                return assembly.DefinedTypes.Select(info => info.AsType()).ToList();
#else
                return assembly.GetTypes();
#endif
            }
            catch (ReflectionTypeLoadException e) when (!throwOnError)
            {
                Tracer.Error("SafeGetTypes {0} - error {1}", assembly.FullName, e.Flatten(true));
            }
            return Empty.Array<Type>();
        }

        public static object InvokeEx([NotNull] this ConstructorInfo constructor)
        {
            return constructor.InvokeEx(Empty.Array<object>());
        }

        public static object InvokeEx([NotNull] this ConstructorInfo constructor, params object[] parameters)
        {
            return ServiceProvider.ReflectionManager.GetActivatorDelegate(constructor).Invoke(parameters);
        }

        public static object InvokeEx([NotNull] this MethodInfo method, object target)
        {
            return method.InvokeEx(target, Empty.Array<object>());
        }

        public static object InvokeEx([NotNull] this MethodInfo method, object target, params object[] parameters)
        {
            return ServiceProvider.ReflectionManager.GetMethodDelegate(method).Invoke(target, parameters);
        }

        public static T GetValueEx<T>([NotNull] this MemberInfo member, object target)
        {
            return ServiceProvider.ReflectionManager.GetMemberGetter<T>(member).Invoke(target);
        }

        public static void SetValueEx<T>([NotNull] this MemberInfo member, object target, T value)
        {
            ServiceProvider.ReflectionManager.GetMemberSetter<T>(member).Invoke(target, value);
        }

        public static object GetDefaultValue(this Type type)
        {
#if NET_STANDARD
            if (type.GetTypeInfo().IsValueType)
#else
            if (type.IsValueType)
#endif
                return Activator.CreateInstance(type);
            return null;
        }

        internal static object GetDataContext(object item)
        {
            Should.NotBeNull(item, nameof(item));
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
            return func?.Invoke(item);
        }

        internal static bool SetDataContext(object item, object dataContext)
        {
            Should.NotBeNull(item, nameof(item));
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

        public static IList<IModule> GetModules([NotNull] this IEnumerable<Assembly> assemblies, bool throwOnError)
        {
            Should.NotBeNull(assemblies, nameof(assemblies));
            var modulesToLoad = new List<Type>();
            foreach (var assembly in SkipFrameworkAssemblies(assemblies))
            {
                foreach (var type in assembly.SafeGetTypes(throwOnError))
                {
                    if (typeof(IModule).IsAssignableFrom(type) && type.IsPublicNonAbstractClass() && !modulesToLoad.Contains(type))
                        modulesToLoad.Add(type);
                }
            }
            var modules = new List<IModule>();
            for (int index = 0; index < modulesToLoad.Count; index++)
            {
                Type moduleType = modulesToLoad[index];
                var constructor = moduleType.GetConstructor(Empty.Array<Type>());
#if NET_STANDARD
                if (constructor == null || !constructor.IsPublic || modulesToLoad.Any(type => type != moduleType && type.GetTypeInfo().IsSubclassOf(moduleType)))
#else
                if (constructor == null || !constructor.IsPublic || modulesToLoad.Any(type => type != moduleType && type.IsSubclassOf(moduleType)))
#endif
                {
                    modulesToLoad.Remove(moduleType);
                    index--;
                    continue;
                }
                modules.Add((IModule)constructor.Invoke(Empty.Array<object>()));
            }
            modules.Sort((m1, m2) => m2.Priority.CompareTo(m1.Priority));
            return modules;
        }

        public static IEnumerable<Assembly> SkipFrameworkAssemblies(this IEnumerable<Assembly> assemblies)
        {
            return assemblies.Where(IsToolkitAssemblyDelegate);
        }

        public static bool IsNetFrameworkAssembly(this Assembly assembly)
        {
            if (assembly.IsDynamic)
                return false;
            return assembly.HasKnownPublicKey(true);
        }

        public static bool IsToolkitAssembly(this Assembly assembly)
        {
            if (assembly.IsDynamic)
                return false;
            return !assembly.HasKnownPublicKey(false);
        }

        public static bool IsPublic(this Type type)
        {
#if NET_STANDARD      
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsPublic || typeInfo.IsNestedPublic;
#else
            return type.IsPublic || type.IsNestedPublic;
#endif
        }

        public static bool IsPublicNonAbstractClass(this Type type)
        {
#if NET_STANDARD
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsClass && !typeInfo.IsAbstract && (typeInfo.IsPublic || typeInfo.IsNestedPublic);
#else
            return type.IsClass && !type.IsAbstract && (type.IsPublic || type.IsNestedPublic);
#endif
        }

        public static bool IsAnonymousClass(this Type type)
        {
#if NET_STANDARD
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsDefined(typeof(CompilerGeneratedAttribute), false) && typeInfo.IsClass;
#else
            return type.IsDefined(typeof(CompilerGeneratedAttribute), false) && type.IsClass;
#endif
        }

        internal static AssemblyName GetAssemblyName(this Assembly assembly)
        {
            Should.NotBeNull(assembly, nameof(assembly));
            return new AssemblyName(assembly.FullName);
        }

        internal static Attribute[] GetAttributes([NotNull] this MemberInfo member)
        {
            Should.NotBeNull(member, nameof(member));
            Attribute[] attributes;
            lock (CachedAttributes)
            {
                if (!CachedAttributes.TryGetValue(member, out attributes))
                {
                    attributes = member.GetCustomAttributes(typeof(Attribute), true)
                        .Cast<Attribute>()
                        .ToArray();
                    CachedAttributes[member] = attributes;
                }
            }
            return attributes;
        }

        [Pure]
        public static MemberInfo GetMemberInfo([NotNull] this Func<LambdaExpression> getExpression)
        {
            Should.NotBeNull(getExpression, nameof(getExpression));
            if (getExpression.HasClosure())
            {
                LambdaExpression expression = getExpression();
                expression.TraceClosureWarn();
                return expression.GetMemberInfo();
            }
            lock (ExpressionToMemberInfoCache)
            {
                MemberInfo info;
                if (!ExpressionToMemberInfoCache.TryGetValue(getExpression, out info))
                {
                    info = getExpression().GetMemberInfo();
                    ExpressionToMemberInfoCache[getExpression] = info;
                }
                return info;
            }
        }

        public static MemberInfo GetMemberInfo([NotNull] this LambdaExpression expression)
        {
            Should.NotBeNull(expression, nameof(expression));
            // Get the last element of the include path
            var memberExpression = (expression.Body as UnaryExpression)?.Operand as MemberExpression;
            if (memberExpression != null)
                return memberExpression.Member;

            var expressionBody = expression.Body as MemberExpression;
            if (expressionBody == null)
                throw new NotSupportedException("Expression " + expression + " not supported");
            // ReSharper disable once PossibleNullReferenceException
            return expressionBody.Member;
        }

        internal static HashSet<string> GetIgnoreProperties(this Type type)
        {
            string[] result;
            lock (CachedIgnoreAttributes)
            {
                if (!CachedIgnoreAttributes.TryGetValue(type, out result))
                {
                    var list = new List<string>(ExcludedProperties);
                    foreach (PropertyInfo propertyInfo in type.GetPropertiesEx(PropertyBindingFlag))
                    {
                        if (propertyInfo.IsDefined(typeof(IgnorePropertyAttribute), true))
                            list.Add(propertyInfo.Name);
                    }
                    result = list.ToArrayEx();
                    CachedIgnoreAttributes[type] = result;
                }
            }
            return new HashSet<string>(result);
        }

        internal static Dictionary<string, ICollection<string>> GetViewModelToModelProperties(this Type type)
        {
            Dictionary<string, ICollection<string>> result;
            lock (CachedViewModelProperties)
            {
                if (!CachedViewModelProperties.TryGetValue(type, out result))
                {
                    result = new Dictionary<string, ICollection<string>>();
                    foreach (PropertyInfo propertyInfo in type.GetPropertiesEx(PropertyBindingFlag))
                    {
                        IEnumerable<ModelPropertyAttribute> attributes = propertyInfo
                            .GetAttributes()
                            .OfType<ModelPropertyAttribute>();
                        foreach (ModelPropertyAttribute modelPropertyAttribute in attributes)
                        {
                            ICollection<string> list;
                            if (!result.TryGetValue(modelPropertyAttribute.Property, out list))
                            {
                                list = new HashSet<string>();
                                result[modelPropertyAttribute.Property] = list;

                                //to keep default property mapping.
                                ICollection<string> vmPropertyMap;
                                if (!result.TryGetValue(propertyInfo.Name, out vmPropertyMap))
                                {
                                    vmPropertyMap = new HashSet<string>();
                                    result[propertyInfo.Name] = vmPropertyMap;
                                }
                                vmPropertyMap.Add(propertyInfo.Name);
                            }
                            list.Add(propertyInfo.Name);
                        }
                    }
                    CachedViewModelProperties[type] = result;
                }
            }
            return new Dictionary<string, ICollection<string>>(result);
        }

        internal static void DisposeCommands(IViewModel item)
        {
            Should.NotBeNull(item, nameof(item));
            Func<object, ICommand>[] list;
            lock (TypesToCommandsProperties)
            {
                Type type = item.GetType();
                if (!TypesToCommandsProperties.TryGetValue(type, out list))
                {
                    List<Func<object, ICommand>> items = null;
                    foreach (var p in type.GetPropertiesEx(PropertyBindingFlag))
                    {
                        if (typeof(ICommand).IsAssignableFrom(p.PropertyType) && p.CanRead &&
                            p.GetIndexParameters().Length == 0)
                        {
                            var func = ServiceProvider.ReflectionManager.GetMemberGetter<ICommand>(p);
                            if (items == null)
                                items = new List<Func<object, ICommand>>();
                            items.Add(func);
                        }
                    }
                    list = items == null ? Empty.Array<Func<object, ICommand>>() : items.ToArray();
                    TypesToCommandsProperties[type] = list;
                }
            }
            if (list.Length == 0) return;
            for (int index = 0; index < list.Length; index++)
            {
                try
                {
                    (list[index].Invoke(item) as IDisposable)?.Dispose();
                }
                catch (Exception)
                {
                    //To avoid method access exception.
                }
            }
        }

        internal static Action<object, IViewModel> GetViewModelPropertySetter(Type viewType)
        {
            lock (ViewToViewModelInterface)
            {
                Action<object, IViewModel> result;
                if (!ViewToViewModelInterface.TryGetValue(viewType, out result))
                {
#if NET_STANDARD
                    foreach (Type @interface in viewType.GetInterfaces().Where(type => type.GetTypeInfo().IsGenericType))
#else
                    foreach (Type @interface in viewType.GetInterfaces().Where(type => type.IsGenericType))
#endif

                    {
                        if (@interface.GetGenericTypeDefinition() != typeof(IViewModelAwareView<>)) continue;
                        if (result != null)
                            throw ExceptionManager.DuplicateInterface("view", "IViewModelAwareView<>", viewType);
                        result = @interface.GetPropertyEx(nameof(IViewModelAwareView<IViewModel>.ViewModel), MemberFlags.Public | MemberFlags.Instance).SetValue;
                    }
                    ViewToViewModelInterface[viewType] = result;
                }
                return result;
            }
        }

        internal static PropertyInfo GetViewProperty(Type viewModelType)
        {
            lock (ViewModelToViewInterface)
            {
                PropertyInfo result;
                if (!ViewModelToViewInterface.TryGetValue(viewModelType, out result))
                {
#if NET_STANDARD
                    foreach (Type @interface in viewModelType.GetInterfaces().Where(type => type.GetTypeInfo().IsGenericType))
#else
                    foreach (Type @interface in viewModelType.GetInterfaces().Where(type => type.IsGenericType))
#endif
                    {
                        if (@interface.GetGenericTypeDefinition() != typeof(IViewAwareViewModel<>)) continue;
                        if (result != null)
                            throw ExceptionManager.DuplicateInterface("view model", "IViewAwareViewModel<>", viewModelType);
                        result = @interface.GetPropertyEx(nameof(IViewAwareViewModel<object>.View), MemberFlags.Public | MemberFlags.Instance);
                    }
                    ViewModelToViewInterface[viewModelType] = result;
                }
                return result;
            }
        }

        internal static void SetValue<TValue>(this PropertyInfo property, object target, TValue value)
        {
            property.SetValue(target, value, Empty.Array<object>());
        }

        internal static void SetValue<TValue>(this FieldInfo field, object target, TValue value)
        {
            field.SetValue(target, value);
        }

        internal static void TraceClosureWarn(this Expression expression)
        {
            if (Debugger.IsAttached)
                Tracer.Warn("The expression '{0}' has closure, it can lead to poor performance", expression);
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

        private static void UnsubscribeCollectionChanged(object o, NotifyCollectionChangedEventHandler handler)
        {
            var notifyCollectionChanged = o as INotifyCollectionChanged;
            if (notifyCollectionChanged != null)
                notifyCollectionChanged.CollectionChanged -= handler;
        }

        private static PropertyChangedEventHandler CreateHandler(IWeakEventHandler<PropertyChangedEventArgs> weakEventHandler)
        {
            return weakEventHandler.Handle;
        }

        private static EventHandler<DataErrorsChangedEventArgs> CreateHandler(IWeakEventHandler<DataErrorsChangedEventArgs> weakEventHandler)
        {
            return weakEventHandler.Handle;
        }

        private static NotifyCollectionChangedEventHandler CreateHandler(IWeakEventHandler<NotifyCollectionChangedEventArgs> weakEventHandler)
        {
            return weakEventHandler.Handle;
        }

        private static bool HasKnownPublicKey(this Assembly assembly, bool msCheck)
        {
            var assemblyName = assembly.GetAssemblyName();
            var bytes = assemblyName.GetPublicKeyToken();
            if (bytes == null || bytes.Length == 0)
                return !msCheck && KnownAssemblyName.Contains(assemblyName.Name);
            var builder = new StringBuilder(16);
            for (int i = 0; i < bytes.Length; i++)
                builder.Append(bytes[i].ToString("x2"));
            if (msCheck)
                return KnownMSPublicKeys.Contains(builder.ToString());
            return KnownPublicKeys.Contains(builder.ToString());
        }

        [CanBeNull]
        public static MethodInfo GetMethodEx([NotNull] this Type type, string name, Type[] types, MemberFlags flags = MemberFlags.Public | MemberFlags.Static | MemberFlags.Instance)
        {
            Should.NotBeNull(type, nameof(type));
            foreach (var method in type.GetMethodsEx(flags))
            {
                if (method.Name != name)
                    continue;
                var parameters = method.GetParameters();
                if (parameters.Length != types.Length)
                    continue;
                bool equal = true;
                for (int j = 0; j < parameters.Length; j++)
                {
                    if (!parameters[j].ParameterType.Equals(types[j]))
                    {
                        equal = false;
                        break;
                    }
                }
                if (equal)
                    return method;
            }
            return null;
        }

        internal static bool HasClosure(this Delegate del)
        {
            if (del.Target == null)
                return false;
            var type = del.Target.GetType();
            lock (HasClosureDictionary)
            {
                bool value;
                if (!HasClosureDictionary.TryGetValue(type, out value))
                {
                    value = type.GetPropertiesEx(MemberFlags.Public | MemberFlags.NonPublic | MemberFlags.Instance).Count != 0 ||
                        type.GetFieldsEx(MemberFlags.Public | MemberFlags.NonPublic | MemberFlags.Instance).Count != 0;
                    HasClosureDictionary[type] = value;
                }
                return value;
            }
        }

        public static string GetPrettyName(this Type t)
        {
            return GetPrettyNameRecursively(t).Replace("+", ".");
        }

        private static string GetPrettyNameRecursively(Type t)
        {
#if NET_STANDARD
            if (!t.GetTypeInfo().IsGenericType)
                return t.FullName;
#else
            if (!t.IsGenericType)
                return t.FullName;
#endif
            StringBuilder sb = new StringBuilder();
            sb.Append(t.Namespace);
            sb.Append(".");
            sb.Append(t.Name.Substring(0, t.Name.LastIndexOf('`')));
            sb.Append(t.GetGenericArguments().Aggregate("<", (aggregate, type) => aggregate + (aggregate == "<" ? "" : ",") + GetPrettyName(type)));
            sb.Append(">");
            return sb.ToString();
        }

        internal static object Convert(object value, Type type)
        {
            if (value == null)
                return type.GetDefaultValue();
            if (type.IsInstanceOfType(value))
                return value;
            if (type == typeof(Guid))
                return Guid.Parse(value.ToString());
#if NET_STANDARD
            if (type.GetTypeInfo().IsEnum)
#else 
            if (type.IsEnum)
#endif
            {
                var s = value as string;
                if (s == null)
                    return Enum.ToObject(type, value);
                return Enum.Parse(type, s, false);
            }

#if NET_STANDARD
            return System.Convert.ChangeType(value, type, CultureInfo.CurrentCulture);
#else
            if (value is IConvertible)
                return System.Convert.ChangeType(value, type, CultureInfo.CurrentCulture);
            return value;
#endif
        }

#if NET_STANDARD
        [CanBeNull]
        public static PropertyInfo GetPropertyEx([NotNull] this Type type, string name, MemberFlags flags = MemberFlags.Public | MemberFlags.Static | MemberFlags.Instance)
        {
            Should.NotBeNull(type, nameof(type));
            foreach (var property in type.GetRuntimeProperties())
            {
                if (property.Name == name && FilterProperty(property, flags))
                    return property;
            }
            while (type != null)
            {
                var typeInfo = type.GetTypeInfo();
                foreach (var property in typeInfo.DeclaredProperties)
                {
                    if (property.Name == name && FilterProperty(property, flags))
                        return property;
                }
                type = type == typeof(object) ? null : typeInfo.BaseType;
            }
            return null;
        }

        [NotNull]
        public static ICollection<PropertyInfo> GetPropertiesEx([NotNull] this Type type, MemberFlags flags = MemberFlags.Public | MemberFlags.Static | MemberFlags.Instance)
        {
            Should.NotBeNull(type, nameof(type));
            var list = new List<PropertyInfo>();
            while (type != null)
            {
                var typeInfo = type.GetTypeInfo();
                foreach (var property in typeInfo.DeclaredProperties)
                {
                    if (FilterProperty(property, flags))
                        list.Add(property);
                }
                type = type == typeof(object) ? null : typeInfo.BaseType;
            }
            return list;
        }

        [CanBeNull]
        public static FieldInfo GetFieldEx([NotNull] this Type type, string name, MemberFlags flags = MemberFlags.Public | MemberFlags.Static | MemberFlags.Instance)
        {
            Should.NotBeNull(type, nameof(type));
            foreach (var field in type.GetRuntimeFields())
            {
                if (field.Name == name && FilterField(field, flags))
                    return field;
            }
            while (type != null)
            {
                var typeInfo = type.GetTypeInfo();
                foreach (var field in typeInfo.DeclaredFields)
                {
                    if (field.Name == name && FilterField(field, flags))
                        return field;
                }
                type = type == typeof(object) ? null : typeInfo.BaseType;
            }
            return null;
        }

        [NotNull]
        public static ICollection<FieldInfo> GetFieldsEx([NotNull] this Type type, MemberFlags flags = MemberFlags.Public | MemberFlags.Static | MemberFlags.Instance)
        {
            Should.NotBeNull(type, nameof(type));
            var list = new List<FieldInfo>();
            while (type != null)
            {
                var typeInfo = type.GetTypeInfo();
                foreach (var field in typeInfo.DeclaredFields)
                {
                    if (FilterField(field, flags))
                        list.Add(field);
                }
                type = type == typeof(object) ? null : typeInfo.BaseType;
            }
            return list;
        }

        [CanBeNull]
        public static MethodInfo GetMethodEx([NotNull] this Type type, string name, MemberFlags flags = MemberFlags.Public | MemberFlags.Static | MemberFlags.Instance)
        {
            Should.NotBeNull(type, nameof(type));
            MethodInfo result = null;
            foreach (var method in type.GetMethodsEx(flags))
            {
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
        public static ICollection<MethodInfo> GetMethodsEx([NotNull] this Type type, MemberFlags flags = MemberFlags.Public | MemberFlags.Static | MemberFlags.Instance)
        {
            Should.NotBeNull(type, nameof(type));
            var list = new List<MethodInfo>();
            while (type != null)
            {
                var typeInfo = type.GetTypeInfo();
                foreach (var method in typeInfo.DeclaredMethods)
                {
                    if (FilterMethod(method, flags))
                        list.Add(method);
                }
                type = type == typeof(object) ? null : typeInfo.BaseType;
            }
            return list;
        }

        [CanBeNull]
        public static EventInfo GetEventEx(this Type type, string name, MemberFlags flags = MemberFlags.Public | MemberFlags.Static | MemberFlags.Instance)
        {
            Should.NotBeNull(type, nameof(type));
            foreach (var @event in type.GetRuntimeEvents())
            {
                if (@event.Name == name && FilterMethod(@event.AddMethod ?? @event.RemoveMethod, flags))
                    return @event;
            }
            while (type != null)
            {
                var typeInfo = type.GetTypeInfo();
                foreach (var @event in typeInfo.DeclaredEvents)
                {
                    if (@event.Name == name && FilterMethod(@event.AddMethod ?? @event.RemoveMethod, flags))
                        return @event;
                }
                type = type == typeof(object) ? null : typeInfo.BaseType;
            }
            return null;
        }

        [CanBeNull]
        public static ConstructorInfo GetConstructor([NotNull] this Type type, Type[] types)
        {
            Should.NotBeNull(type, nameof(type));
            foreach (ConstructorInfo constructor in type.GetTypeInfo().DeclaredConstructors)
            {
                if (constructor.IsStatic)
                    continue;
                ParameterInfo[] constParams = constructor.GetParameters();
                if (types.Length != constParams.Length)
                    continue;
                bool equal = true;
                for (int i = 0; i < constParams.Length; i++)
                {
                    if (constParams[i].ParameterType != types[i])
                    {
                        equal = false;
                        break;
                    }
                }
                if (equal)
                    return constructor;
            }
            return null;
        }

        internal static bool IsDefined(this Type type, Type attributeType, bool inherit)
        {
            Should.NotBeNull(type, nameof(type));
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
            Should.NotBeNull(typeFrom, nameof(typeFrom));
            Should.NotBeNull(typeTo, nameof(typeTo));
            return typeFrom.GetTypeInfo().IsAssignableFrom(typeTo.GetTypeInfo());
        }

        internal static IEnumerable<Type> GetInterfaces([NotNull] this Type type)
        {
            Should.NotBeNull(type, nameof(type));
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
            if (property.CanRead && FilterMethod(property.GetMethod, flags))
                return true;
            return property.CanWrite && FilterMethod(property.SetMethod, flags);
        }

        private static bool FilterField(FieldInfo field, MemberFlags flags)
        {
            if (field == null)
                return false;
            return ((flags.HasMemberFlag(MemberFlags.Static) && field.IsStatic) ||
                    flags.HasMemberFlag(MemberFlags.Instance) && !field.IsStatic) &&
                   ((flags.HasMemberFlag(MemberFlags.NonPublic) && !field.IsPublic) ||
                    flags.HasMemberFlag(MemberFlags.Public) && field.IsPublic);
        }

        private static bool FilterMethod(MethodBase method, MemberFlags flags)
        {
            if (method == null)
                return false;
            return ((flags.HasMemberFlag(MemberFlags.Static) && method.IsStatic) ||
                    flags.HasMemberFlag(MemberFlags.Instance) && !method.IsStatic) &&
                   ((flags.HasMemberFlag(MemberFlags.NonPublic) && !method.IsPublic) ||
                    flags.HasMemberFlag(MemberFlags.Public) && method.IsPublic);
        }
#else
        public static MethodInfo GetMethodInfo([NotNull] this Delegate del)
        {
            Should.NotBeNull(del, nameof(del));
            return del.Method;
        }

        [CanBeNull]
        public static PropertyInfo GetPropertyEx([NotNull] this Type type, string name, MemberFlags flags = MemberFlags.Public | MemberFlags.Static | MemberFlags.Instance)
        {
            Should.NotBeNull(type, nameof(type));
            var bindingFlags = flags.ToBindingFlags(false);
            var property = type.GetProperty(name, bindingFlags | BindingFlags.FlattenHierarchy);
            if (property != null || !flags.HasMemberFlag(MemberFlags.NonPublic))
                return property;
            bindingFlags |= BindingFlags.DeclaredOnly;
            while (type != null)
            {
                property = type.GetProperty(name, bindingFlags);
                if (property != null)
                    return property;
                type = type == typeof(object) ? null : type.BaseType;
            }
            return null;
        }

        [NotNull]
        public static ICollection<PropertyInfo> GetPropertiesEx([NotNull] this Type type, MemberFlags flags = MemberFlags.Public | MemberFlags.Static | MemberFlags.Instance)
        {
            Should.NotBeNull(type, nameof(type));
            var bindingFlags = flags.ToBindingFlags(false);
            if (flags.HasMemberFlag(MemberFlags.NonPublic))
            {
                bindingFlags |= BindingFlags.DeclaredOnly;
                var list = new List<PropertyInfo>();
                while (type != null)
                {
                    list.AddRange(type.GetProperties(bindingFlags));
                    type = type == typeof(object) ? null : type.BaseType;
                }
                return list;
            }
            return type.GetProperties(bindingFlags | BindingFlags.FlattenHierarchy);
        }

        [CanBeNull]
        public static FieldInfo GetFieldEx([NotNull] this Type type, string name, MemberFlags flags = MemberFlags.Public | MemberFlags.Static | MemberFlags.Instance)
        {
            Should.NotBeNull(type, nameof(type));
            var bindingFlags = flags.ToBindingFlags(false);
            var field = type.GetField(name, bindingFlags | BindingFlags.FlattenHierarchy);
            if (field != null || !flags.HasMemberFlag(MemberFlags.NonPublic))
                return field;
            bindingFlags |= BindingFlags.DeclaredOnly;
            while (type != null)
            {
                field = type.GetField(name, bindingFlags);
                if (field != null)
                    return field;
                type = type == typeof(object) ? null : type.BaseType;
            }
            return null;
        }

        [NotNull]
        public static ICollection<FieldInfo> GetFieldsEx([NotNull] this Type type, MemberFlags flags = MemberFlags.Public | MemberFlags.Static | MemberFlags.Instance)
        {
            Should.NotBeNull(type, nameof(type));
            var bindingFlags = flags.ToBindingFlags(false);
            if (flags.HasMemberFlag(MemberFlags.NonPublic))
            {
                bindingFlags |= BindingFlags.DeclaredOnly;
                var list = new List<FieldInfo>();
                while (type != null)
                {
                    list.AddRange(type.GetFields(bindingFlags));
                    type = type == typeof(object) ? null : type.BaseType;
                }
                return list;
            }
            return type.GetFields(bindingFlags | BindingFlags.FlattenHierarchy);
        }

        [CanBeNull]
        public static MethodInfo GetMethodEx([NotNull] this Type type, string name, MemberFlags flags = MemberFlags.Public | MemberFlags.Static | MemberFlags.Instance)
        {
            Should.NotBeNull(type, nameof(type));
            var bindingFlags = flags.ToBindingFlags(false);
            var method = type.GetMethod(name, bindingFlags | BindingFlags.FlattenHierarchy);
            if (method != null || !flags.HasMemberFlag(MemberFlags.NonPublic))
                return method;
            bindingFlags |= BindingFlags.DeclaredOnly;
            while (type != null)
            {
                method = type.GetMethod(name, bindingFlags);
                if (method != null)
                    return method;
                type = type == typeof(object) ? null : type.BaseType;
            }
            return null;
        }

        [NotNull]
        public static ICollection<MethodInfo> GetMethodsEx([NotNull] this Type type, MemberFlags flags = MemberFlags.Public | MemberFlags.Static | MemberFlags.Instance)
        {
            Should.NotBeNull(type, nameof(type));
            var bindingFlags = flags.ToBindingFlags(false);
            if (flags.HasMemberFlag(MemberFlags.NonPublic))
            {
                bindingFlags |= BindingFlags.DeclaredOnly;
                var list = new List<MethodInfo>();
                while (type != null)
                {
                    list.AddRange(type.GetMethods(bindingFlags));
                    type = type == typeof(object) ? null : type.BaseType;
                }
                return list;
            }
            return type.GetMethods(bindingFlags | BindingFlags.FlattenHierarchy);
        }

        [CanBeNull]
        public static EventInfo GetEventEx([NotNull] this Type type, string name, MemberFlags flags = MemberFlags.Public | MemberFlags.Static | MemberFlags.Instance)
        {
            Should.NotBeNull(type, nameof(type));
            var bindingFlags = flags.ToBindingFlags(false);
            var @event = type.GetEvent(name, bindingFlags | BindingFlags.FlattenHierarchy);
            if (@event != null || !flags.HasMemberFlag(MemberFlags.NonPublic))
                return @event;
            bindingFlags |= BindingFlags.DeclaredOnly;
            while (type != null)
            {
                @event = type.GetEvent(name, bindingFlags);
                if (@event != null)
                    return @event;
                type = type == typeof(object) ? null : type.BaseType;
            }
            return null;
        }

        internal static Assembly GetAssembly(this Type type)
        {
            return type.Assembly;
        }

        private static BindingFlags ToBindingFlags(this MemberFlags flags, bool flatten)
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
            if (flatten)
                result |= BindingFlags.FlattenHierarchy;
            return result;
        }
#endif
        #endregion
    }
}