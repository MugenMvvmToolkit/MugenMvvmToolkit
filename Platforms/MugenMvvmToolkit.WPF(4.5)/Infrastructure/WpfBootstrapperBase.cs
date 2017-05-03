#region Copyright

// ****************************************************************************
// <copyright file="WpfBootstrapperBase.cs">
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
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using JetBrains.Annotations;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.WPF.Infrastructure
{
    public abstract class WpfBootstrapperBase : BootstrapperBase, IDynamicViewModelPresenter
    {
        #region Fields

        private readonly PlatformInfo _platform;

        #endregion

        #region Constructors

        static WpfBootstrapperBase()
        {
            ReflectionExtensions.GetTypesDefault = assembly => assembly.GetTypes();
            ApplicationSettings.MultiViewModelPresenterCanShowViewModel = CanShowViewModelTabPresenter;
        }

        internal WpfBootstrapperBase(bool isDesignMode, PlatformInfo platform = null)
            : base(isDesignMode)
        {
            _platform = platform ?? WpfToolkitExtensions.GetPlatformInfo();
        }

        protected WpfBootstrapperBase([NotNull] Application application, bool autoStart = true, PlatformInfo platform = null)
            : this(false, platform)
        {
            Should.NotBeNull(application, nameof(application));
            application.Startup += ApplicationOnStartup;
            AutoStart = autoStart;
        }

        #endregion

        #region Properties

        public bool AutoStart { get; set; }

        public bool ShutdownOnMainViewModelClose { get; set; }

        #endregion

        #region Overrides of BootstrapperBase

        protected override PlatformInfo Platform => _platform;

        protected override void UpdateAssemblies(HashSet<Assembly> assemblies)
        {
            base.UpdateAssemblies(assemblies);
            try
            {
                assemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic));
            }
            catch
            {
                if (!IsDesignMode)
                    throw;
            }
            TryLoadAssemblyByType("AttachedMembers", "MugenMvvmToolkit.WPF.Binding", assemblies);
        }

        #endregion

        #region Implementation of IDynamicViewModelPresenter

        int IDynamicViewModelPresenter.Priority => int.MaxValue;

        IAsyncOperation IDynamicViewModelPresenter.TryShowAsync(IDataContext context, IViewModelPresenter parentPresenter)
        {
            parentPresenter.DynamicPresenters.Remove(this);
            var operation = parentPresenter.ShowAsync(context);
            if (ShutdownOnMainViewModelClose)
            {
                operation.ContinueWith(result =>
                {
                    Application application = Application.Current;
                    if (application != null)
                    {
                        Action action = application.Shutdown;
                        application.Dispatcher.BeginInvoke(action);
                    }
                });
            }
            return operation;
        }

        Task<bool> IDynamicViewModelPresenter.TryCloseAsync(IDataContext context, IViewModelPresenter parentPresenter)
        {
            return null;
        }

        #endregion

        #region Methods

        public virtual void Start()
        {
            Initialize();
            if (!MvvmApplication.Context.Contains(NavigationConstants.IsDialog))
                MvvmApplication.Context.Add(NavigationConstants.IsDialog, false);
            IocContainer.Get<IViewModelPresenter>().DynamicPresenters.Add(this);
            MvvmApplication.Start();
        }

        private void ApplicationOnStartup(object sender, StartupEventArgs args)
        {
            var application = sender as Application;
            if (application != null)
                application.Startup -= ApplicationOnStartup;
            if (AutoStart)
                Start();
        }

        private static bool CanShowViewModelTabPresenter(IViewModel viewModel, IDataContext dataContext, IViewModelPresenter arg3)
        {
            var viewName = viewModel.GetViewName(dataContext);
            var container = viewModel.GetIocContainer(true);
            var mappingProvider = container.Get<IViewMappingProvider>();
            var mappingItem = mappingProvider.FindMappingForViewModel(viewModel.GetType(), viewName, false);
            return mappingItem == null || !typeof(Page).IsAssignableFrom(mappingItem.ViewType);
        }

        #endregion
    }
}
