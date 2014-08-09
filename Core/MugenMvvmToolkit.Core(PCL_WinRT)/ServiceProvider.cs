#region Copyright
// ****************************************************************************
// <copyright file="ServiceProvider.cs">
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
using System.Reflection;
using MugenMvvmToolkit.Infrastructure.Callbacks;
using MugenMvvmToolkit.Infrastructure.Validation;
using MugenMvvmToolkit.Interfaces.Validation;
using System.Threading;
using JetBrains.Annotations;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Collections;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
#if NET4
using AttachedValueProviderDefault = MugenMvvmToolkit.Infrastructure.AttachedValueProvider;    
#endif
using MugenMvvmToolkit.Utils;

namespace MugenMvvmToolkit
{
    public static class ServiceProvider
    {
        #region Fields

        private static readonly Dictionary<Type, ConstructorInfo> EntityConstructorInfos;

        private static IIocContainer _iocContainer;
        private static IThreadManager _threadManager;
        private static IReflectionManager _reflectionManager;
        private static ITracer _tracer;
        private static IAttachedValueProvider _attachedValueProvider;
        private static IValidatorProvider _validatorProvider;
        private static IOperationCallbackFactory _operationCallbackFactory;

        private static Func<ITrackingCollection, IStateTransitionManager> _trackingCollectionStateTransitionManagerFactory;
        private static Func<ITrackingCollection, IEqualityComparer<object>> _trackingCollectionEqualityComparerFactory;
        private static Func<object, IEventAggregator> _instanceEventAggregatorFactory;
        private static Func<object, bool, WeakReference> _weakReferenceFactory;        

        #endregion

        #region Constructors

        static ServiceProvider()
        {
            EntityConstructorInfos = new Dictionary<Type, ConstructorInfo>();
            DefaultEntityFactory = DefaultEntityFactoryMethod;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the factory that creates an empty instance of editable entity.
        /// </summary>
        [CanBeNull]
        public static Func<Type, object> DefaultEntityFactory { get; set; }

        /// <summary>
        ///     Gets or sets the metadata type provider.
        /// </summary>
        [CanBeNull]
        public static Func<Type, IEnumerable<Type>> EntityMetadataTypeProvider { get; set; }

        /// <summary>
        ///     Gets or sets the factory that creates a instance of <see cref="WeakReference"/>.
        /// </summary>
        [NotNull]
        public static Func<object, bool, WeakReference> WeakReferenceFactory
        {
            get
            {
                if (_weakReferenceFactory == null)
                    return CreateWeakReference;
                return _weakReferenceFactory;
            }
            set { _weakReferenceFactory = value; }
        }

        /// <summary>
        ///     Gets or sets the factory that creates an instance of <see cref="IEventAggregator" /> for the specified item.
        /// </summary>
        [NotNull]
        public static Func<object, IEventAggregator> InstanceEventAggregatorFactory
        {
            get
            {
                if (_instanceEventAggregatorFactory == null)
                    return GetInstanceEventAggregator;
                return _instanceEventAggregatorFactory;
            }
            set { _instanceEventAggregatorFactory = value; }
        }

        /// <summary>
        ///     Gets or sets the factory that creates an instance of <see cref="IEqualityComparer{T}" />
        /// </summary>
        [NotNull]
        public static Func<ITrackingCollection, IEqualityComparer<object>> TrackingCollectionEqualityComparerFactory
        {
            get
            {
                if (_trackingCollectionEqualityComparerFactory == null)
                    _trackingCollectionEqualityComparerFactory = collection => ReferenceEqualityComparer.Instance;
                return _trackingCollectionEqualityComparerFactory;
            }
            set { _trackingCollectionEqualityComparerFactory = value; }
        }

        /// <summary>
        ///     Gets or sets the factory that creates an instance of <see cref="IStateTransitionManager" />
        /// </summary>
        [NotNull]
        public static Func<ITrackingCollection, IStateTransitionManager> TrackingCollectionStateTransitionManagerFactory
        {
            get
            {
                if (_trackingCollectionStateTransitionManagerFactory == null)
                    _trackingCollectionStateTransitionManagerFactory = collection => StateTransitionManager.Instance;
                return _trackingCollectionStateTransitionManagerFactory;
            }
            set { _trackingCollectionStateTransitionManagerFactory = value; }
        }

        /// <summary>
        ///     Gets or sets the root <see cref="IIocContainer" />.
        /// </summary>
        public static IIocContainer IocContainer
        {
            get { return _iocContainer; }
        }

        /// <summary>
        ///     Gets or sets the default <see cref="IThreadManager" />.
        /// </summary>
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

        /// <summary>
        ///     Gets or sets the attached value provider.
        /// </summary>
        [NotNull]
        public static IAttachedValueProvider AttachedValueProvider
        {
            get
            {
#if PCL_Silverlight
                if (_attachedValueProvider == null)
                    throw ExceptionManager.ObjectNotInitialized("AttachedValueProvider", null, "");
#else
                if (_attachedValueProvider == null)
                    Interlocked.CompareExchange(ref _attachedValueProvider, new AttachedValueProviderDefault(), null);
#endif
                return _attachedValueProvider;
            }
            set { _attachedValueProvider = value; }
        }

        /// <summary>
        ///     Gets the flag that indicates that the attached value provider is initialized.
        /// </summary>
        public static bool HasAttachedValueProvider
        {
            get
            {
#if PCL
                return _attachedValueProvider != null;
#else
                return true;
#endif
            }
        }

        /// <summary>
        ///     Gets or sets the default <see cref="IReflectionManager" />.
        /// </summary>
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

        /// <summary>
        ///     Gets or sets the default tracer
        /// </summary>
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

        /// <summary>
        ///     Gets or sets the default <see cref="IOperationCallbackFactory" />.
        /// </summary>
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

        /// <summary>
        ///     Gets or sets the default <see cref="IValidatorProvider" />.
        /// </summary>
        [NotNull]
        public static IValidatorProvider ValidatorProvider
        {
            get
            {
                if (_validatorProvider == null)
                    _validatorProvider = new ValidatorProvider(true, _iocContainer);
                return _validatorProvider;
            }
            set { _validatorProvider = value; }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Sets the <see cref="IocContainer" />.
        /// </summary>
        public static void Initialize(IIocContainer iocContainer, PlatformInfo platform)
        {
            Should.NotBeNull(iocContainer, "iocContainer");
            ApplicationSettings.Platform = platform;
            _iocContainer = iocContainer;
            TryInitialize(iocContainer, ref _tracer);
            TryInitialize(iocContainer, ref _reflectionManager);
            TryInitialize(iocContainer, ref _attachedValueProvider);
            TryInitialize(iocContainer, ref _threadManager);
            TryInitialize(iocContainer, ref _operationCallbackFactory);
            TryInitialize(iocContainer, ref _validatorProvider);
            if (iocContainer.CanResolve<IViewModelSettings>())
                ApplicationSettings.ViewModelSettings = iocContainer.Get<IViewModelSettings>();
        }

        private static void TryInitialize<TService>(IIocContainer iocContainer, ref TService service)
        {
            TService result;
            if (iocContainer.TryGet(out result))
                service = result;
        }

        private static WeakReference CreateWeakReference(object o, bool b)
        {
            return new WeakReference(o, b);
        }

        private static object DefaultEntityFactoryMethod(Type type)
        {
            Should.NotBeNull(type, "type");
            ConstructorInfo constructor;
            lock (EntityConstructorInfos)
            {
                if (!EntityConstructorInfos.TryGetValue(type, out constructor))
                {
                    constructor = type.GetConstructor(EmptyValue<Type>.ArrayInstance);
                    EntityConstructorInfos[type] = constructor;
                }
            }
            if (constructor == null)
            {
                MugenMvvmToolkit.Tracer.Warn("Cannot create default entity no default constructor exists for class {0}", type);
                return null;
            }
            return constructor.InvokeEx(EmptyValue<object>.ArrayInstance);
        }

        private static IEventAggregator GetInstanceEventAggregator(object o)
        {
            return new EventAggregator();
        }

        #endregion
    }
}