#region Copyright

// ****************************************************************************
// <copyright file="BootstrapperBase.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
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

using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Infrastructure
{
    /// <summary>
    ///     Represents the base class that is used to start MVVM application.
    /// </summary>
    public abstract class BootstrapperBase
    {
        #region Fields

        private const int StoppedState = 0;
        private const int InitializedState = 1;
        private static int _state;

        private IModuleContext _context;
        private IList<IModule> _loadedModules;
        private IList<Assembly> _assemblies;
        private IDataContext _initializationContext;
        private IIocContainer _iocContainer;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="BootstrapperBase" /> class.
        /// </summary>
        protected BootstrapperBase()
        {
            if (Current != null)
            {
                Tracer.Error("The application is already has a bootstrapper " + Current);
                return;
            }
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            ServiceProvider.DesignTimeManager = new DesignTimeManagerImpl(Platform);
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the current <see cref="BootstrapperBase" />.
        /// </summary>
        public static BootstrapperBase Current { get; protected set; }

        /// <summary>
        ///     Gets the current platform.
        /// </summary>
        [NotNull]
        public abstract PlatformInfo Platform { get; }

        /// <summary>
        ///     Gets the initialized state of the current bootstraper.
        /// </summary>
        public bool IsInitialized
        {
            get { return _state == InitializedState; }
        }

        /// <summary>
        ///     Gets the current <see cref="IIocContainer" />.
        /// </summary>
        [NotNull]
        public IIocContainer IocContainer
        {
            get { return _iocContainer; }
            protected set { _iocContainer = value; }
        }

        /// <summary>
        ///     Gets the initialization context.
        /// </summary>
        [NotNull]
        public IDataContext InitializationContext
        {
            get { return _initializationContext ?? DataContext.Empty; }
            set { _initializationContext = value; }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Initializes the current bootstraper.
        /// </summary>
        public virtual void Initialize()
        {
            if (Interlocked.Exchange(ref _state, InitializedState) == InitializedState)
                return;
            Current = this;
            IocContainer = CreateIocContainer();
            OnInitialize();
        }

        /// <summary>
        ///     Stops the current bootstraper.
        /// </summary>
        public virtual void Stop()
        {
            if (Interlocked.Exchange(ref _state, StoppedState) == StoppedState)
                return;
            if (Current != null && !ReferenceEquals(Current, this))
                Current.Stop();
            try
            {
                OnStop();
            }
            finally
            {
                var iocContainer = IocContainer;
                if (iocContainer != null)
                    iocContainer.Dispose();
                Current = null;
            }
        }

        /// <summary>
        ///     Initializes the current bootstraper.
        /// </summary>
        protected virtual void OnInitialize()
        {
            LoadModules();
            if (!IocContainer.CanResolve<IViewModelSettings>())
            {
                IViewModelSettings settings = CreateViewModelSettings();
                IocContainer.BindToConstant(settings);
            }
            ServiceProvider.Initialize(IocContainer, Platform);
        }

        /// <summary>
        ///     Stops the current bootstraper.
        /// </summary>
        protected virtual void OnStop()
        {
            UnloadModules();
        }

        /// <summary>
        ///     Loads application modules.
        /// </summary>
        protected virtual void LoadModules()
        {
            _loadedModules = GetModules();
            if (_loadedModules != null && _loadedModules.Count != 0)
            {
                _context = CreateModuleContext(IocContainer);
                _loadedModules = _context.LoadModules(_loadedModules);
            }
        }

        /// <summary>
        ///     Unloads application modules.
        /// </summary>
        protected virtual void UnloadModules()
        {
            if (_loadedModules != null)
            {
                for (int index = 0; index < _loadedModules.Count; index++)
                    _loadedModules[index].Unload(_context);
            }
        }

        /// <summary>
        ///     Creates an instance of <see cref="IViewModelSettings" />.
        /// </summary>
        /// <returns>An instance of <see cref="IViewModelSettings" />.</returns>
        [NotNull]
        protected virtual IViewModelSettings CreateViewModelSettings()
        {
            return new DefaultViewModelSettings();
        }

        /// <summary>
        ///     Gets the application modules.
        /// </summary>
        [NotNull]
        protected virtual IList<IModule> GetModules()
        {
            return GetAssembliesInternal().GetModules(true);
        }

        /// <summary>
        ///     Gets the application assemblies.
        /// </summary>
        [NotNull]
        protected virtual ICollection<Assembly> GetAssemblies()
        {
            return new List<Assembly> { GetType().GetAssembly(), typeof(BootstrapperBase).GetAssembly() };
        }

        /// <summary>
        ///     Creates an instance of <see cref="IModuleContext" />.
        /// </summary>
        /// <returns>An instance of <see cref="IModuleContext" />.</returns>
        [NotNull]
        protected virtual IModuleContext CreateModuleContext(IIocContainer iocContainer)
        {
            return new ModuleContext(Platform, LoadMode.Runtime, iocContainer, InitializationContext, GetAssembliesInternal());
        }

        /// <summary>
        ///     Creates an instance of <see cref="IIocContainer" />.
        /// </summary>
        /// <returns>An instance of <see cref="IIocContainer" />.</returns>
        [NotNull]
        protected abstract IIocContainer CreateIocContainer();

        /// <summary>
        ///     Tries to load assembly by full name.
        /// </summary>
        protected static Assembly TryLoadAssembly(string assemblyName, ICollection<Assembly> assemblies)
        {
            try
            {
#if PCL_WINRT
                var assembly = Assembly.Load(new AssemblyName(assemblyName));
#else
                var assembly = Assembly.Load(assemblyName);
#endif

                if (assembly != null && assemblies != null)
                    assemblies.Add(assembly);
                return assembly;
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
                return null;
            }
        }

        private IList<Assembly> GetAssembliesInternal()
        {
            if (_assemblies == null)
            {
                var assemblies = GetAssemblies();
                assemblies.Add(GetType().GetAssembly());
                _assemblies = assemblies.ToArrayEx();
            }
            return _assemblies;
        }

        #endregion
    }
}