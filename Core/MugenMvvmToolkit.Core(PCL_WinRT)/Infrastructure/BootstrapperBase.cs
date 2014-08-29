#region Copyright
// ****************************************************************************
// <copyright file="BootstrapperBase.cs">
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
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Utils;

namespace MugenMvvmToolkit.Infrastructure
{
    /// <summary>
    ///     Represents the base class that is used to start MVVM application.
    /// </summary>
    public abstract class BootstrapperBase
    {
        #region Fields

        private const int InitializedState = 1;
        private IModuleContext _context;
        private IList<IModule> _loadedModules;
        private static int _state;
        private IList<Assembly> _assemblies;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="BootstrapperBase" /> class.
        /// </summary>
        protected BootstrapperBase()
        {
            ServiceProvider.DesignTimeManager = new DesignTimeManagerImpl(Platform);
        }

        #endregion

        #region Properties

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
        public IIocContainer IocContainer { get; protected set; }

        /// <summary>
        ///     Gets the current platform.
        /// </summary>
        [NotNull]
        public abstract PlatformInfo Platform { get; }

        #endregion

        #region Methods

        /// <summary>
        ///     Initializes the current bootstraper.
        /// </summary>
        public virtual void Initialize(bool throwIfInitialized = false)
        {
            if (Interlocked.Exchange(ref _state, InitializedState) == InitializedState)
            {
                if (throwIfInitialized)
                    throw ExceptionManager.ObjectInitialized("Bootstrapper", this);
                return;
            }
            IocContainer = CreateIocContainer();
            OnInitialize();
        }

        /// <summary>
        ///     Stops the current bootstraper.
        /// </summary>
        public virtual void Stop()
        {
            try
            {
                if (Interlocked.Exchange(ref _state, 0) != 0)
                    OnStop();
            }
            finally
            {
                var iocContainer = IocContainer;
                if (iocContainer != null)
                    iocContainer.Dispose();

            }
        }

        /// <summary>
        ///     Initializes the current bootstraper.
        /// </summary>
        protected virtual void OnInitialize()
        {
            LoadModules();
            IViewModelSettings settings = CreateViewModelSettings();
            if (!IocContainer.CanResolve<IViewModelSettings>())
                IocContainer.BindToConstant(settings);
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
            if (_loadedModules == null)
                return;
            for (int index = 0; index < _loadedModules.Count; index++)
                _loadedModules[index].Unload(_context);
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
            return MvvmUtils.GetModules(GetAssembliesInternal(), true);
        }

        /// <summary>
        ///     Gets the application assemblies.
        /// </summary>
        [NotNull]
        protected virtual ICollection<Assembly> GetAssemblies()
        {
            return new[] { GetType().GetAssembly(), typeof(BootstrapperBase).GetAssembly() };
        }

        /// <summary>
        ///     Creates an instance of <see cref="IModuleContext" />.
        /// </summary>
        /// <returns>An instance of <see cref="IModuleContext" />.</returns>
        [NotNull]
        protected virtual IModuleContext CreateModuleContext(IIocContainer iocContainer)
        {
            return new ModuleContext(Platform, LoadMode.Runtime, iocContainer, DataContext.Empty, GetAssembliesInternal());
        }

        /// <summary>
        ///     Creates an instance of <see cref="IIocContainer" />.
        /// </summary>
        /// <returns>An instance of <see cref="IIocContainer" />.</returns>
        [NotNull]
        protected abstract IIocContainer CreateIocContainer();

        /// <summary>
        /// Tries to add assembly by full name.
        /// </summary>
        protected static ICollection<Assembly> TryAddAssembly(string assemblyName, ICollection<Assembly> assemblies)
        {
            try
            {
#if PCL_WINRT
                var assembly = Assembly.Load(new AssemblyName(assemblyName));
#else
                var assembly = Assembly.Load(assemblyName);
#endif

                if (assembly != null)
                    assemblies.Add(assembly);
            }
            catch
            {
            }
            return assemblies;
        }

        private IList<Assembly> GetAssembliesInternal()
        {
            if (_assemblies == null)
            {
                var assemblies = GetAssemblies();
                assemblies.Add(GetType().GetAssembly());
                _assemblies = assemblies.ToArrayFast();
            }
            return _assemblies;
        }

        #endregion
    }
}