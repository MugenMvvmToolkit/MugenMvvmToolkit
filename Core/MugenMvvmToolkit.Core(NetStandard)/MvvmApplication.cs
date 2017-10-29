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
        private readonly LoadMode? _moduleLoadMode;
        private PlatformInfo _platform;
        private IIocContainer _iocContainer;
        private readonly IDataContext _context;
        private ApplicationState _applicationState;
        private readonly Action<IModuleContext> _loadModulesDelegate;

        #endregion

        #region Constructors

        protected MvvmApplication(LoadMode? moduleLoadMode = null, IList<IModule> modules = null, Action<IModuleContext> loadModulesDelegate = null)
        {
            if (ServiceProvider.UiSynchronizationContextField == null)
                ServiceProvider.UiSynchronizationContextField = SynchronizationContext.Current;
            _moduleLoadMode = moduleLoadMode;
            _applicationState = ApplicationState.Active;
            _platform = PlatformInfo.Unknown;
            _context = new DataContext();
            _loadModulesDelegate = loadModulesDelegate;
            if (!modules.IsNullOrEmpty())
                Modules = modules;
            ServiceProvider.Initialize(this);
        }

        #endregion

        #region Properties

        public bool IsInitialized => _isInitialized;

        public ApplicationState ApplicationState => _applicationState;

        public PlatformInfo PlatformInfo => _platform;

        public IIocContainer IocContainer => _iocContainer;

        public IDataContext Context => _context;

        public IList<IModule> Modules { get; set; }

        #endregion

        #region Methods

        protected abstract Type GetStartViewModelType();

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
                        ServiceProvider.BootstrapCodeBuilder?.Append(nameof(LoadModules), $"new {module.GetType().GetPrettyName()}().Load(context);", ApplicationSettings.CodeBuilderHighPriority);
                        module.TraceModule(true);
                    }
                }
            }
        }

        [NotNull]
        protected virtual IModuleContext CreateModuleContext(IList<Assembly> assemblies)
        {
            var mode = _moduleLoadMode.GetValueOrDefault(Context.GetData(InitializationConstants.IsDesignMode) ? LoadMode.Design : LoadMode.Runtime);
            return new ModuleContext(PlatformInfo, mode, IocContainer, new DataContext(Context), assemblies);
        }

        protected virtual IList<IModule> GetModules(IList<Assembly> assemblies)
        {
            return Modules ?? assemblies.GetModules(!ServiceProvider.IsDesignMode);
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

        #endregion
    }
}
