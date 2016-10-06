#region Copyright

// ****************************************************************************
// <copyright file="ServiceProvider.cs">
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
using System.Reflection;
using MugenMvvmToolkit.Infrastructure.Callbacks;
using MugenMvvmToolkit.Infrastructure.Validation;
using MugenMvvmToolkit.Interfaces.Validation;
using System.Threading;
using JetBrains.Annotations;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit
{
    public static class ServiceProvider
    {
        #region Fields

        private static readonly Dictionary<Type, ConstructorInfo> EntityConstructorInfos;

        private static IThreadManager _threadManager;
        private static ITracer _tracer;
        private static IAttachedValueProvider _attachedValueProvider;
        private static IOperationCallbackFactory _operationCallbackFactory;
        private static IReflectionManager _reflectionManager;
        private static IValidatorProvider _validatorProvider;
        private static IEventAggregator _eventAggregator;

        private static Func<object, IEventAggregator> _instanceEventAggregatorFactory;
        private static Func<object, WeakReference> _weakReferenceFactory;
        private static IViewModelProvider _viewModelProvider;
        private static Func<IViewModel, IViewModelSettings> _viewModelSettingsFactory;
        internal static SynchronizationContext UiSynchronizationContextField;
        internal static Func<IViewModel, IDataContext, IViewModelPresenter, bool> CanShowMultiViewModelDelegate;

        #endregion

        #region Constructors

        static ServiceProvider()
        {
            _weakReferenceFactory = CreateWeakReference;
            _instanceEventAggregatorFactory = GetInstanceEventAggregator;
            _viewModelSettingsFactory = CreateViewModelSettings;
            EntityConstructorInfos = new Dictionary<Type, ConstructorInfo>();
            DefaultEntityFactory = DefaultEntityFactoryMethod;
            ObjectToSubscriberConverter = ObjectToSubscriberConverterImpl;

            var current = MvvmApplication.Current;
            if (current != null && current.IsInitialized)
                Initialize(current);
            MvvmApplication.Initialized += MvvmApplicationOnInitialized;
        }

        #endregion

        #region Properties

        [CanBeNull]
        public static IBootstrapCodeBuilder BootstrapCodeBuilder { get; set; }

        [CanBeNull]
        public static Func<object, IDataContext, ISubscriber> ObjectToSubscriberConverter { get; set; }

        [CanBeNull]
        public static Func<Type, object> DefaultEntityFactory { get; set; }

        [CanBeNull]
        public static Func<Type, IEnumerable<Type>> EntityMetadataTypeProvider { get; set; }

        [NotNull]
        public static SynchronizationContext UiSynchronizationContext
        {
            get { return UiSynchronizationContextField ?? SynchronizationContext.Current; }
            set { UiSynchronizationContextField = value; }
        }

        [NotNull]
        public static Func<IViewModel, IViewModelSettings> ViewModelSettingsFactory
        {
            get { return _viewModelSettingsFactory; }
            set { _viewModelSettingsFactory = value ?? CreateViewModelSettings; }
        }

        [NotNull]
        public static Func<object, WeakReference> WeakReferenceFactory
        {
            get { return _weakReferenceFactory; }
            set { _weakReferenceFactory = value ?? CreateWeakReference; }
        }

        [NotNull]
        public static Func<object, IEventAggregator> InstanceEventAggregatorFactory
        {
            get { return _instanceEventAggregatorFactory; }
            set { _instanceEventAggregatorFactory = value ?? GetInstanceEventAggregator; }
        }

        [CanBeNull]
        public static IItemsSourceDecorator ItemsSourceDecorator { get; set; }

        [CanBeNull]
        public static IOperationCallbackStateManager OperationCallbackStateManager { get; set; }

        public static IIocContainer IocContainer
        {
            get
            {
                if (MvvmApplication.Current == null)
                    return null;
                return MvvmApplication.Current.IocContainer;
            }
        }

        [NotNull]
        public static IThreadManager ThreadManager
        {
            get
            {
                if (_threadManager == null)
                    _threadManager = new SynchronousThreadManager();
                return _threadManager;
            }
            set { _threadManager = value; }
        }

        [NotNull]
        public static IAttachedValueProvider AttachedValueProvider
        {
            get
            {
                if (_attachedValueProvider == null)
                    Interlocked.CompareExchange(ref _attachedValueProvider, new AttachedValueProviderDefault(), null);
                return _attachedValueProvider;
            }
            set { _attachedValueProvider = value; }
        }

        [NotNull]
        public static IReflectionManager ReflectionManager
        {
            get
            {
                if (_reflectionManager == null)
                    Interlocked.CompareExchange(ref _reflectionManager, new ExpressionReflectionManager(), null);
                return _reflectionManager;
            }
            set { _reflectionManager = value; }
        }

        [NotNull]
        public static ITracer Tracer
        {
            get
            {
                if (_tracer == null)
                    return MugenMvvmToolkit.Tracer.Instance;
                return _tracer;
            }
            set { _tracer = value; }
        }

        [NotNull]
        public static IOperationCallbackFactory OperationCallbackFactory
        {
            get
            {
                if (_operationCallbackFactory == null)
                    return DefaultOperationCallbackFactory.Instance;
                return _operationCallbackFactory;
            }
            set { _operationCallbackFactory = value; }
        }

        [NotNull]
        public static IValidatorProvider ValidatorProvider
        {
            get
            {
                if (_validatorProvider == null)
                    Interlocked.CompareExchange(ref _validatorProvider, new ValidatorProvider(), null);
                return _validatorProvider;
            }
            set { _validatorProvider = value; }
        }

        [NotNull]
        public static IViewModelProvider ViewModelProvider
        {
            get
            {
                if (_viewModelProvider == null)
                    Interlocked.CompareExchange(ref _viewModelProvider, new ViewModelProvider(IocContainer), null);
                return _viewModelProvider;
            }
            set { _viewModelProvider = value; }
        }

        [NotNull]
        public static IEventAggregator EventAggregator
        {
            get
            {
                if (_eventAggregator == null)
                    Interlocked.CompareExchange(ref _eventAggregator, new EventAggregator(), null);
                return _eventAggregator;
            }
            set { _eventAggregator = value; }
        }

        private static bool? _isDesignMode;

        public static bool IsDesignMode
        {
            get
            {
                if (_isDesignMode == null)
                    _isDesignMode = GetIsDesignMode();
                return _isDesignMode.Value;
            }
            set { _isDesignMode = value; }
        }

        #endregion

        #region Methods

        [Pure]
        public static bool TryGet(Type type, out object result, string name = null,
            params IIocParameter[] parameters)
        {
            var iocContainer = IocContainer;
            if (iocContainer == null)
            {
#if PCL_WINRT
                result = type.GetTypeInfo().IsValueType ? Activator.CreateInstance(type) : null;
#else
                result = type.IsValueType ? Activator.CreateInstance(type) : null;
#endif
                return false;
            }
            return iocContainer.TryGet(type, out result, name, parameters);
        }

        [Pure]
        public static bool TryGet<T>(out T result, string name = null,
            params IIocParameter[] parameters)
        {
            var iocContainer = IocContainer;
            if (iocContainer == null)
            {
                result = default(T);
                return false;
            }
            return iocContainer.TryGet(out result, name, parameters);
        }

        [Pure]
        public static T GetOrCreate<T>()
        {
            return (T)GetOrCreate(typeof(T));
        }

        [Pure]
        public static object GetOrCreate(Type type)
        {
            var iocContainer = IocContainer;
            if (iocContainer == null)
                return Activator.CreateInstance(type);
            return iocContainer.Get(type);
        }

        [Pure]
        public static T Get<T>(string name = null, params IIocParameter[] parameters)
        {
            return (T)Get(typeof(T), name, parameters);
        }

        [Pure]
        public static object Get(Type type, string name = null, params IIocParameter[] parameters)
        {
            var iocContainer = IocContainer;
            if (iocContainer == null)
                throw ExceptionManager.ObjectNotInitialized("MvvmApplication", null);
            return iocContainer.Get(type, name, parameters);
        }

        internal static IList<T> TryDecorate<T>(object owner, IList<T> itemsSource)
        {
            var decorator = ItemsSourceDecorator;
            if (decorator == null)
                return itemsSource;
            return decorator.Decorate(owner, itemsSource);
        }

        private static void MvvmApplicationOnInitialized(object sender, EventArgs eventArgs)
        {
            Initialize(MvvmApplication.Current);
        }

        private static void Initialize(IMvvmApplication application)
        {
            Should.NotBeNull(application, nameof(application));
            var iocContainer = application.IocContainer;
            TryInitialize(iocContainer, ref _tracer);
            TryInitialize(iocContainer, ref _reflectionManager);
            TryInitialize(iocContainer, ref _attachedValueProvider);
            TryInitialize(iocContainer, ref _threadManager);
            TryInitialize(iocContainer, ref _operationCallbackFactory);
            TryInitialize(iocContainer, ref _validatorProvider);
            TryInitialize(iocContainer, ref _viewModelProvider);
            TryInitialize(iocContainer, ref _eventAggregator);

            if (OperationCallbackStateManager == null)
            {
                IOperationCallbackStateManager stateManager = null;
                TryInitialize(iocContainer, ref stateManager);
                OperationCallbackStateManager = stateManager;
            }
            if (ItemsSourceDecorator == null)
            {
                IItemsSourceDecorator decorator = null;
                TryInitialize(iocContainer, ref decorator);
                ItemsSourceDecorator = decorator;
            }
        }

        private static void TryInitialize<TService>(IIocContainer iocContainer, ref TService service)
        {
            TService result;
            if (iocContainer.TryGet(out result))
                service = result;
        }

        private static IViewModelSettings CreateViewModelSettings(IViewModel vm)
        {
            return new DefaultViewModelSettings();
        }

        private static WeakReference CreateWeakReference(object o)
        {
            return new WeakReference(o, true);
        }

        private static object DefaultEntityFactoryMethod(Type type)
        {
            Should.NotBeNull(type, nameof(type));
            ConstructorInfo constructor;
            lock (EntityConstructorInfos)
            {
                if (!EntityConstructorInfos.TryGetValue(type, out constructor))
                {
                    constructor = type.GetConstructor(Empty.Array<Type>());
                    EntityConstructorInfos[type] = constructor;
                }
            }
            if (constructor == null)
            {
                MugenMvvmToolkit.Tracer.Warn("Cannot create default entity no default constructor exists for class {0}", type);
                return null;
            }
            return constructor.InvokeEx(Empty.Array<object>());
        }

        private static IEventAggregator GetInstanceEventAggregator(object o)
        {
            return new EventAggregator();
        }

        private static ISubscriber ObjectToSubscriberConverterImpl(object o, IDataContext dataContext)
        {
            if (o == null)
                return null;
            return o as ISubscriber ?? HandlerSubscriber.Get(o);
        }


        private static bool GetIsDesignMode()
        {
            //Silverlight
            var type = Type.GetType("System.ComponentModel.DesignerProperties, System.Windows, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e");
            if (type != null)
            {
                try
                {
                    var dme = type.GetPropertyEx("IsInDesignTool");
                    if (dme == null)
                        return false;
                    return (bool)dme.GetValue(null, null);
                }
                catch
                {
                    return false;
                }
            }

            //.NET
            type = Type.GetType("System.ComponentModel.DesignerProperties, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            if (type != null)
            {
                try
                {
                    var dmp = type.GetFieldEx("IsInDesignModeProperty")?.GetValue(null);
                    var dpd = Type.GetType("System.ComponentModel.DependencyPropertyDescriptor, WindowsBase, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
                    var typeFe = Type.GetType("System.Windows.FrameworkElement, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
                    if (dpd == null || typeFe == null)
                        return false;

                    var fromProperty = dpd.GetMethodsEx().FirstOrDefault(mi => mi.Name == "FromProperty" && mi.IsPublic && mi.IsStatic && mi.GetParameters().Length == 2);
                    var descriptor = fromProperty?.Invoke(null, new[] { dmp, typeFe });
                    if (descriptor == null)
                        return false;

                    var metaProp = dpd.GetPropertyEx("Metadata");
                    if (metaProp == null)
                        return false;

                    var metadata = metaProp.GetValue(descriptor, null);
                    var tPropMeta = Type.GetType("System.Windows.PropertyMetadata, WindowsBase, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");

                    if (metadata == null || tPropMeta == null)
                        return false;
                    var dvProp = tPropMeta.GetPropertyEx("DefaultValue");
                    if (dvProp == null)
                        return false;
                    var dv = (bool)dvProp.GetValue(metadata, null);
                    return dv;
                }
                catch
                {
                    return false;
                }
            }

            //WinRT
            type = Type.GetType("Windows.ApplicationModel.DesignMode, Windows, ContentType=WindowsRuntime");
            if (type != null)
            {
                try
                {
                    var dme = type.GetPropertyEx("DesignModeEnabled");
                    if (dme == null)
                        return false;
                    return (bool)dme.GetValue(null, null);
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        #endregion
    }
}
