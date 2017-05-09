#region Copyright

// ****************************************************************************
// <copyright file="BootstrapperBase.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Infrastructure
{
    public abstract class BootstrapperBase
    {
        #region Fields

        private const int InitializedState = 1;
        private static int _state;
        private static int _initializationThreadId;
        private static readonly ManualResetEvent InitializedEvent;
        private readonly Dictionary<string, IViewModel> _viewModelMapping;

        #endregion

        #region Constructors

        static BootstrapperBase()
        {
            InitializedEvent = new ManualResetEvent(false);
        }

        protected BootstrapperBase(bool isDesignMode)
        {
            ServiceProvider.IsDesignMode = isDesignMode;
            if (isDesignMode)
            {
                _viewModelMapping = new Dictionary<string, IViewModel>();
                var context = SynchronizationContext.Current;
                if (context == null)
                    Task.Factory.StartNew(StartFromDesign, this);
                else
                    context.Post(StartFromDesign, this);
            }
        }

        #endregion

        #region Properties

        public bool IsInitialized => _state == InitializedState;

        public static BootstrapperBase Current { get; protected set; }

        public IDataContext InitializationContext { get; set; }

        protected abstract PlatformInfo Platform { get; }

        protected IMvvmApplication MvvmApplication { get; set; }

        protected IIocContainer IocContainer { get; set; }

        protected bool IsDesignMode => ServiceProvider.IsDesignMode;

        #endregion

        #region Methods

        public void Initialize()
        {
#if NET4
            var managedThreadId = Thread.CurrentThread.ManagedThreadId;
#else
            var managedThreadId = Environment.CurrentManagedThreadId;
#endif
            if (Interlocked.Exchange(ref _state, InitializedState) == InitializedState)
            {
                if (_initializationThreadId != managedThreadId)
                    InitializedEvent.WaitOne();
                return;
            }
            _initializationThreadId = managedThreadId;
            Current = this;
            InitializeInternal();
            InitializedEvent.Set();
        }

        [NotNull]
        protected abstract IMvvmApplication CreateApplication();

        [NotNull]
        protected abstract IIocContainer CreateIocContainer();

        protected virtual void InitializeInternal()
        {
            MvvmApplication = CreateApplication();
            IocContainer = CreateIocContainer();
            if (IsDesignMode)
            {
                InitializationContext = InitializationContext.ToNonReadOnly();
                InitializationContext.AddOrUpdate(InitializationConstants.IsDesignMode, true);
            }
            MvvmApplication.Initialize(Platform, IocContainer, GetAssemblies(), InitializationContext ?? DataContext.Empty);
        }

        [NotNull]
        protected virtual IList<Assembly> GetAssemblies()
        {
            var assemblies = new HashSet<Assembly>
            {
                GetType().GetAssembly(),
                typeof(BootstrapperBase).GetAssembly()
            };
            TryLoadAssemblyByType("BindingServiceProvider", "MugenMvvmToolkit.Binding", assemblies);
            if (MvvmApplication != null)
            {
                assemblies.Add(MvvmApplication.GetType().GetAssembly());
                assemblies.Add(MvvmApplication.GetStartViewModelType().GetAssembly());
            }
            UpdateAssemblies(assemblies);
            return assemblies.ToArrayEx();
        }

        protected virtual void UpdateAssemblies(HashSet<Assembly> assemblies)
        {
        }

        internal T GetOrAddDesignViewModelInternal<T>(Func<IViewModelProvider, T> getViewModel, string property) where T : IViewModel
        {
            if (!IsDesignMode)
                return default(T);
            IViewModel value;
            if (!_viewModelMapping.TryGetValue(property, out value))
            {
                value = getViewModel(ServiceProvider.ViewModelProvider);
                _viewModelMapping[property] = value;
            }
            return (T)value;
        }

        protected static void TryLoadAssemblyByType(string typeName, string namespaceAssemblyName, ICollection<Assembly> assemblies)
        {
            TryLoadAssemblyByType(namespaceAssemblyName + "." + typeName + ", " + namespaceAssemblyName, assemblies);
        }

        protected static void TryLoadAssemblyByType(string fullTypeName, ICollection<Assembly> assemblies)
        {
            try
            {
                //faster than Assembly.Load
                var type = Type.GetType(fullTypeName, false);
                if (type != null)
                    assemblies.Add(type.GetAssembly());
            }
            catch
            {
            }
        }

        private static void StartFromDesign(object state)
        {
            ((BootstrapperBase)state).Initialize();
        }

        #endregion
    }
}