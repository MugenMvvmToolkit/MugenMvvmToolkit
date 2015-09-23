#region Copyright

// ****************************************************************************
// <copyright file="MvvmApplication.cs">
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

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using JetBrains.Annotations;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit
{
    /// <summary>
    ///     Represents the base class that is used to start MVVM application.
    /// </summary>
    public abstract class MvvmApplication : IMvvmApplication
    {
        #region Fields

        private const int InitializedState = 1;
        private int _state;

        private readonly LoadMode _mode;
        private PlatformInfo _platform;
        private IViewModelSettings _viewModelSettings;
        private IIocContainer _iocContainer;
        private readonly IDataContext _context;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="MvvmApplication" /> class.
        /// </summary>
        protected MvvmApplication(LoadMode mode = LoadMode.Runtime)
        {
            ServiceProvider.DesignTimeManager = DesignTimeManagerImpl.Instance;
            Current = this;
            _mode = mode;
            _platform = PlatformInfo.Unknown;
            _context = new DataContext();
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the current <see cref="IMvvmApplication" />.
        /// </summary>
        public static IMvvmApplication Current { get; protected set; }

        /// <summary>
        ///     Gets a value indicating whether this instance is initialized.
        /// </summary>
        public virtual bool IsInitialized
        {
            get { return _state == InitializedState; }
        }

        /// <summary>
        ///     Gets the current platform.
        /// </summary>
        public virtual PlatformInfo Platform
        {
            get { return _platform; }
        }

        /// <summary>
        ///     Gets or sets the load mode of current <see cref="IMvvmApplication" />.
        /// </summary>
        public virtual LoadMode Mode
        {
            get { return _mode; }
        }

        /// <summary>
        ///     Gets the current <see cref="IIocContainer" />.
        /// </summary>
        public virtual IIocContainer IocContainer
        {
            get { return _iocContainer; }
        }

        /// <summary>
        ///     Gets the current application context.
        /// </summary>
        public virtual IDataContext Context
        {
            get { return _context; }
        }

        /// <summary>
        ///     Gets the default view model settings.
        /// </summary>
        public virtual IViewModelSettings ViewModelSettings
        {
            get { return _viewModelSettings; }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Occurs when this <see cref="IMvvmApplication" /> is initialized.
        /// </summary>
        public static event EventHandler Initialized;

        /// <summary>
        ///     Initializes the current bootstrapper.
        /// </summary>
        protected virtual void OnInitialize(IList<Assembly> assemblies)
        {
            LoadModules(assemblies);
            if (!IocContainer.TryGet(out _viewModelSettings))
            {
                _viewModelSettings = CreateViewModelSettings();
                IocContainer.BindToConstant(_viewModelSettings);
            }
        }

        /// <summary>
        ///     Loads application modules.
        /// </summary>
        protected virtual void LoadModules(IList<Assembly> assemblies)
        {
            var loadedModules = GetModules(assemblies);
            if (loadedModules != null && loadedModules.Count != 0)
            {
                var context = CreateModuleContext(assemblies);
                context.LoadModules(loadedModules);
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
        ///     Creates an instance of <see cref="IModuleContext" />.
        /// </summary>
        /// <returns>An instance of <see cref="IModuleContext" />.</returns>
        [NotNull]
        protected virtual IModuleContext CreateModuleContext(IList<Assembly> assemblies)
        {
            return new ModuleContext(Platform, Mode, IocContainer, Context, assemblies);
        }

        /// <summary>
        ///     Gets the application modules.
        /// </summary>
        protected virtual IList<IModule> GetModules(IList<Assembly> assemblies)
        {
            if (Mode == LoadMode.Design)
                return Empty.Array<IModule>();
            return assemblies.GetModules(true);
        }

        /// <summary>
        ///     Raises the <see cref="Initialized" /> event.
        /// </summary>
        protected static void RaiseInitialized(IMvvmApplication sender)
        {
            var handler = Initialized;
            if (handler != null) handler(sender, EventArgs.Empty);
        }

        #endregion

        #region Implementation of interfaces

        /// <summary>
        ///     Initializes the current application.
        /// </summary>
        public void Initialize(PlatformInfo platform, IIocContainer iocContainer, IList<Assembly> assemblies,
            IDataContext context)
        {
            Should.NotBeNull(platform, "platform");
            Should.NotBeNull(iocContainer, "iocContainer");
            Should.NotBeNull(assemblies, "assemblies");
            if (Interlocked.Exchange(ref _state, InitializedState) == InitializedState)
                return;
            Current = this;
            _platform = platform;
            _iocContainer = iocContainer;
            if (context != null)
                Context.Merge(context);
            OnInitialize(assemblies);
            RaiseInitialized(this);
        }

        /// <summary>
        ///     Gets the type of start view model.
        /// </summary>
        public abstract Type GetStartViewModelType();

        #endregion
    }
}