#region Copyright

// ****************************************************************************
// <copyright file="MvvmApplication.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.Messages;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit
{
    public abstract class MvvmApplication : IMvvmApplication
    {
        #region Fields

        private bool _isInitialized;
        private readonly Action<IModuleContext> _loadModulesDelegate;
        private readonly LoadMode? _mode;
        private PlatformInfo _platform;
        private IIocContainer _iocContainer;
        private readonly IDataContext _context;
        private ApplicationState _applicationState;

        #endregion

        #region Constructors

        protected MvvmApplication(Action<IModuleContext> loadModulesDelegate = null, LoadMode? mode = null)
        {
            if (ServiceProvider.UiSynchronizationContextField == null)
                ServiceProvider.UiSynchronizationContextField = SynchronizationContext.Current;
            _loadModulesDelegate = loadModulesDelegate;
            _mode = mode;
            _applicationState = ApplicationState.Active;
            _platform = PlatformInfo.Unknown;
            _context = new DataContext();
            ServiceProvider.Initialize(this);
        }

        #endregion

        #region Properties

        public virtual bool IsInitialized => _isInitialized;

        public ApplicationState ApplicationState => _applicationState;

        public virtual PlatformInfo PlatformInfo => _platform;

        public virtual LoadMode Mode => _mode.GetValueOrDefault(Context.GetData(InitializationConstants.IsDesignMode) ? LoadMode.Design : LoadMode.Runtime);

        public virtual IIocContainer IocContainer => _iocContainer;

        public virtual IDataContext Context => _context;

        public IList<IModule> Modules { get; set; }

        #endregion

        #region Methods

        protected virtual void StartInternal()
        {
            var context = new DataContext(Context);
            IocContainer
                .Get<IViewModelProvider>()
                .GetViewModel(GetStartViewModelType(), context)
                .ShowAsync((model, result) => model.Dispose(), context: context);
        }

        protected virtual void OnInitialize(IList<Assembly> assemblies)
        {
            LoadModules(assemblies);
        }

        protected virtual void LoadModules(IList<Assembly> assemblies)
        {
            if (_loadModulesDelegate != null)
            {
                _loadModulesDelegate(CreateModuleContext(assemblies));
                return;
            }
            var modules = GetModules(assemblies);
            if (modules != null && modules.Count != 0)
            {
                var context = CreateModuleContext(assemblies);
                for (int index = 0; index < modules.Count; index++)
                {
                    var module = modules[index];
                    if (module.Load(context))
                    {
                        ServiceProvider.BootstrapCodeBuilder?.Append(nameof(LoadModules), $"new {module.GetType().GetPrettyName()}().Load(context);");
                        module.TraceModule(true);
                    }
                }
            }
        }

        [NotNull]
        protected virtual IModuleContext CreateModuleContext(IList<Assembly> assemblies)
        {
            return new ModuleContext(PlatformInfo, Mode, IocContainer, new DataContext(Context), assemblies);
        }

        protected virtual IList<IModule> GetModules(IList<Assembly> assemblies)
        {
            return Modules ?? assemblies.GetModules(!Mode.IsDesignMode());
        }

        protected virtual void OnApplicationStateChanged(ApplicationState oldState, ApplicationState newState, IDataContext context)
        {
        }

        #endregion

        #region Implementation of interfaces

        public void Initialize(PlatformInfo platformInfo, IIocContainer iocContainer, IList<Assembly> assemblies, IDataContext context)
        {
            Should.NotBeNull(platformInfo, nameof(platformInfo));
            Should.NotBeNull(iocContainer, nameof(iocContainer));
            Should.NotBeNull(assemblies, nameof(assemblies));
            if (_isInitialized)
                return;
            _isInitialized = true;
            _platform = platformInfo;
            _iocContainer = iocContainer;
            if (context != null)
                Context.Merge(context);
            _iocContainer.BindToConstant<IMvvmApplication>(this);
            OnInitialize(assemblies);
            ServiceProvider.Initialize(this);
        }

        public void SetApplicationState(ApplicationState value, IDataContext context)
        {
            if (_applicationState == value)
                return;
            var oldState = _applicationState;
            _applicationState = value;
            if (IsInitialized)
            {
                switch (value)
                {
                    case ApplicationState.Active:
                        ServiceProvider.EventAggregator.Publish(this, new ForegroundNavigationMessage(context));
                        break;
                    case ApplicationState.Background:
                        ServiceProvider.EventAggregator.Publish(this, new BackgroundNavigationMessage(context));
                        break;
                }
            }
            OnApplicationStateChanged(oldState, value, context ?? DataContext.Empty);
        }

        public void Start()
        {
            StartInternal();
        }

        public abstract Type GetStartViewModelType();

        #endregion
    }
}
