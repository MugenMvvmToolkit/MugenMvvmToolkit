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
using System.Collections.Specialized;
using System.ComponentModel;
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
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Validation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.ViewModels;

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
                        _unsubscribeAction.Invoke(sender, HandlerDelegate);
                }
                else
                    _invokeAction(target, sender, arg);
            }

            #endregion
        }

        #endregion

        #region Fields

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
            CachedAttributes = new Dictionary<MemberInfo, Attribute[]>();
            GetDataContextDelegateCache = new Dictionary<Type, Func<object, object>>();
            SetDataContextDelegateCache = new Dictionary<Type, Action<object, object>>();
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
                "Xamarin.Forms.Core",
                "Xamarin.Forms.Platform.Android",
                "Xamarin.Forms.Xaml",
                "Xamarin.Forms.Platform.iOS",
                "Xamarin.Forms.Platform.WP8",
            };
            IsToolkitAssemblyDelegate = IsToolkitAssembly;

            CachedViewModelProperties = new Dictionary<Type, Dictionary<string, ICollection<string>>>();
            CachedIgnoreAttributes = new Dictionary<Type, string[]>();
            ExcludedProperties = typeof(EditableViewModel<>)
                .GetPropertiesEx(PropertyBindingFlag)
                .ToArrayEx(info => info.Name);
            TypesToCommandsProperties = new Dictionary<Type, Func<object, ICommand>[]>();
            ViewToViewModelInterface = new Dictionary<Type, Action<object, IViewModel>>();
            ViewModelToViewInterface = new Dictionary<Type, PropertyInfo>();
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
        ///     Returns a weak-reference version of a delegate.
        /// </summary>
        public static NotifyCollectionChangedEventHandler MakeWeakCollectionChangedHandler<TTarget>(TTarget target, Action<TTarget, object,
            NotifyCollectionChangedEventArgs> invokeAction) where TTarget : class
        {
            return CreateWeakDelegate(target, invokeAction, UnsubscribeCollectionChangedDelegate, CreateCollectionChangedHandlerDelegate);
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
            return Empty.Array<Type>();
        }

        /// <summary>
        ///     Invokes the constructor using the current <see cref="IReflectionManager" />.
        /// </summary>
        public static object InvokeEx([NotNull] this ConstructorInfo constructor)
        {
            return constructor.InvokeEx(Empty.Array<object>());
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
            return method.InvokeEx(target, Empty.Array<object>());
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
        internal static object GetDataContext(object item)
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
        internal static bool SetDataContext(object item, object dataContext)
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

        /// <summary>
        ///     Gets the modules.
        /// </summary>
        public static IList<IModule> GetModules([NotNull] this IEnumerable<Assembly> assemblies, bool throwOnError)
        {
            Should.NotBeNull(assemblies, "assemblies");
            var modulesToLoad = new List<Type>();
            foreach (var assembly in SkipFrameworkAssemblies(assemblies.Distinct()))
            {
                foreach (var type in assembly.SafeGetTypes(throwOnError))
                {
                    if (typeof(IModule).IsAssignableFrom(type) && type.IsPublicNonAbstractClass())
                        modulesToLoad.Add(type);
                }
            }
            var modules = new List<IModule>();
            for (int index = 0; index < modulesToLoad.Count; index++)
            {
                Type moduleType = modulesToLoad[index];
                var constructor = moduleType.GetConstructor(Empty.Array<Type>());
#if PCL_WINRT
                if (constructor == null || modulesToLoad.Any(type => type != moduleType && type.GetTypeInfo().IsSubclassOf(moduleType)))
#else
                if (constructor == null || modulesToLoad.Any(type => type != moduleType && type.IsSubclassOf(moduleType)))
#endif
                {
                    modulesToLoad.Remove(moduleType);
                    index--;
                    continue;
                }
                var module = (IModule)constructor.InvokeEx(Empty.Array<object>());
                modules.Add(module);
            }
            modules.Sort((module, module1) => module1.Priority.CompareTo(module.Priority));
            return modules;
        }

        /// <summary>
        /// Filters assemblies.
        /// </summary>
        public static IEnumerable<Assembly> SkipFrameworkAssemblies(this IEnumerable<Assembly> assemblies)
        {
            return assemblies.Where(IsToolkitAssemblyDelegate);
        }

        /// <summary>
        ///     Checks whether the current assembly is Microsoft assembly.
        /// </summary>
        public static bool IsMicrosoftAssembly(this Assembly assembly)
        {
#if !PCL_Silverlight
            if (assembly.IsDynamic)
                return false;
#endif
            return assembly.HasKnownPublicKey(true);
        }

        /// <summary>
        ///     Checks whether the current assembly is toolkit assembly.
        /// </summary>
        public static bool IsToolkitAssembly(this Assembly assembly)
        {
#if !PCL_Silverlight
            if (assembly.IsDynamic)
                return false;
#endif
            return !assembly.HasKnownPublicKey(false);
        }

        /// <summary>
        ///     Gets the design time assemblies.
        /// </summary>
        public static IList<Assembly> GetDesignAssemblies()
        {
            return DesignTimeInitializer.GetAssemblies(true);
        }

        /// <summary>
        ///     Checks whether the current type is public non-abstract class.
        /// </summary>
        public static bool IsPublicNonAbstractClass(this Type type)
        {
#if PCL_WINRT
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsClass && !typeInfo.IsAbstract && typeInfo.IsPublic;
#else
            return type.IsClass && !type.IsAbstract && type.IsPublic;
#endif
        }

        /// <summary>
        ///     Checks whether the current type is anonymous class.
        /// </summary>
        public static bool IsAnonymousClass(this Type type)
        {
#if PCL_WINRT
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsDefined(typeof(CompilerGeneratedAttribute), false) && typeInfo.IsClass;
#else
            return type.IsDefined(typeof(CompilerGeneratedAttribute), false) && type.IsClass;
#endif
        }

        internal static AssemblyName GetAssemblyName(this Assembly assembly)
        {
            Should.NotBeNull(assembly, "assembly");
            return new AssemblyName(assembly.FullName);
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

        internal static MemberInfo GetMemberInfo([NotNull] this LambdaExpression expression)
        {
            Should.NotBeNull(expression, "expression");
            // Get the last element of the include path
            var unaryExpression = expression.Body as UnaryExpression;
            if (unaryExpression != null)
            {
                var memberExpression = unaryExpression.Operand as MemberExpression;
                if (memberExpression != null)
                    return memberExpression.Member;
            }
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
            Should.NotBeNull(item, "item");
            Func<object, ICommand>[] list;
            lock (TypesToCommandsProperties)
            {
                Type type = item.GetType();
                if (!TypesToCommandsProperties.TryGetValue(type, out list))
                {
                    list = type
                        .GetPropertiesEx(PropertyBindingFlag)
                        .Where(c => typeof(ICommand).IsAssignableFrom(c.PropertyType) && c.CanRead &&
                                    c.GetIndexParameters().Length == 0)
                        .Select(ServiceProvider.ReflectionManager.GetMemberGetter<ICommand>)
                        .ToArray();
                    TypesToCommandsProperties[type] = list;
                }
            }
            if (list.Length == 0) return;
            for (int index = 0; index < list.Length; index++)
            {
                try
                {
                    var disposable = list[index].Invoke(item) as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                }
                catch (MemberAccessException)
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
#if PCL_WINRT
                    foreach (Type @interface in viewType.GetInterfaces().Where(type => type.GetTypeInfo().IsGenericType))
#else
                    foreach (Type @interface in viewType.GetInterfaces().Where(type => type.IsGenericType))
#endif

                    {
                        if (@interface.GetGenericTypeDefinition() != typeof(IViewModelAwareView<>)) continue;
                        if (result != null)
                            throw ExceptionManager.DuplicateInterface("view", "IViewModelAwareView<>", viewType);
                        result = ServiceProvider.ReflectionManager.GetMemberSetter<IViewModel>(@interface.GetPropertyEx("ViewModel", MemberFlags.Public | MemberFlags.Instance));
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
#if PCL_WINRT
                    foreach (Type @interface in viewModelType.GetInterfaces().Where(type => type.GetTypeInfo().IsGenericType))
#else
                    foreach (Type @interface in viewModelType.GetInterfaces().Where(type => type.IsGenericType))
#endif
                    {
                        if (@interface.GetGenericTypeDefinition() != typeof(IViewAwareViewModel<>)) continue;
                        if (result != null)
                            throw ExceptionManager.DuplicateInterface("view model", "IViewAwareViewModel<>", viewModelType);
                        result = @interface.GetPropertyEx("View", MemberFlags.Public | MemberFlags.Instance);
                    }
                    ViewModelToViewInterface[viewModelType] = result;
                }
                return result;
            }
        }

        /// <summary>
        ///     This method is used to reduce closure allocation in generatde methods.
        /// </summary>
        internal static void SetValue<TValue>(this PropertyInfo property, object target, TValue value)
        {
            property.SetValue(target, value, Empty.Array<object>());
        }

        /// <summary>
        ///     This method is used to reduce closure allocation in generatde methods.
        /// </summary>
        internal static void SetValue<TValue>(this FieldInfo field, object target, TValue value)
        {
            field.SetValue(target, value);
        }

        /// <summary>
        ///     This method is used to reduce closure allocation in generatde methods.
        /// </summary>
        internal static object AsFunc(this Action<object[]> action, object target, object[] args)
        {
            action(args);
            return null;
        }

        /// <summary>
        ///     This method is used to reduce closure allocation in generatde methods.
        /// </summary>
        internal static object AsFunc(this Action<object, object[]> action, object target, object[] args)
        {
            action(target, args);
            return null;
        }

        /// <summary>
        ///     This method is used to reduce closure allocation in generatde methods.
        /// </summary>
        internal static object AsFunc(this Func<object[], object> func, object target, object[] args)
        {
            return func(args);
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

        private static NotifyCollectionChangedEventHandler CreateHandler(ReflectionExtensions.IWeakEventHandler<NotifyCollectionChangedEventArgs> weakEventHandler)
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
            return list.ToArray();
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
                if (!find)
                    continue;
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
        /// <summary>
        ///     Gets an object that represents the method represented by the specified delegate.
        /// </summary>
        /// <returns>
        ///     An object that represents the method.
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
            var property = type.GetProperty(name, flags.ToBindingFlags(true));
            if (property != null || !flags.HasMemberFlag(MemberFlags.NonPublic))
                return property;

            var properties = type.GetPropertiesEx(flags);
            for (int index = 0; index < properties.Length; index++)
            {
                property = properties[index];
                if (property.Name == name)
                    return property;
            }
            return null;
        }

        [NotNull]
        public static PropertyInfo[] GetPropertiesEx([NotNull] this Type type, MemberFlags flags = MemberFlags.Public | MemberFlags.Static | MemberFlags.Instance)
        {
            Should.NotBeNull(type, "type");
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
                return list.ToArray();
            }
            return type.GetProperties(bindingFlags | BindingFlags.FlattenHierarchy);
        }

        [CanBeNull]
        public static FieldInfo GetFieldEx([NotNull] this Type type, string name, MemberFlags flags = MemberFlags.Public | MemberFlags.Static | MemberFlags.Instance)
        {
            Should.NotBeNull(type, "type");
            var field = type.GetField(name, flags.ToBindingFlags(true));
            if (field != null || !flags.HasMemberFlag(MemberFlags.NonPublic))
                return field;

            var fields = type.GetFieldsEx(flags);
            for (int index = 0; index < fields.Length; index++)
            {
                field = fields[index];
                if (field.Name == name)
                    return field;
            }
            return null;
        }

        [NotNull]
        public static FieldInfo[] GetFieldsEx([NotNull] this Type type, MemberFlags flags = MemberFlags.Public | MemberFlags.Static | MemberFlags.Instance)
        {
            Should.NotBeNull(type, "type");
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
                return list.ToArray();
            }
            return type.GetFields(bindingFlags | BindingFlags.FlattenHierarchy);
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
            return type.GetMethod(name, flags.ToBindingFlags(true));
        }

        [NotNull]
        public static MethodInfo[] GetMethodsEx([NotNull] this Type type, MemberFlags flags = MemberFlags.Public | MemberFlags.Static | MemberFlags.Instance)
        {
            Should.NotBeNull(type, "type");
            return type.GetMethods(flags.ToBindingFlags(true));
        }

        [CanBeNull]
        public static EventInfo GetEventEx([NotNull] this Type type, string name, MemberFlags flags = MemberFlags.Public | MemberFlags.Static | MemberFlags.Instance)
        {
            Should.NotBeNull(type, "type");
            return type.GetEvent(name, flags.ToBindingFlags(true));
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