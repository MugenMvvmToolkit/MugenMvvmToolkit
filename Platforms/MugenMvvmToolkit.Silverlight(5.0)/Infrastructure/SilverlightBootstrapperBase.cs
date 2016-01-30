#region Copyright

// ****************************************************************************
// <copyright file="SilverlightBootstrapperBase.cs">
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
using System.Reflection;
using System.Windows;
using System.Windows.Resources;
using JetBrains.Annotations;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Infrastructure.Callbacks;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Silverlight.Infrastructure
{
    public abstract class SilverlightBootstrapperBase : BootstrapperBase, IDynamicViewModelPresenter
    {
        #region Fields

        protected const string BindingAssemblyName = "MugenMvvmToolkit.Silverlight.Binding";
        private readonly Application _application;
        private readonly PlatformInfo _platform;

        #endregion

        #region Constructors

        protected SilverlightBootstrapperBase([NotNull] Application application, bool autoStart = true, PlatformInfo platform = null)
        {
            Should.NotBeNull(application, nameof(application));
            _application = application;
            _platform = platform ?? PlatformExtensions.GetPlatformInfo();
            AutoStart = autoStart;
            application.Startup += ApplicationOnStartup;
        }

        #endregion

        #region Properties

        public bool AutoStart { get; set; }

        protected Application Application => _application;

        #endregion

        #region Implementation of IDynamicViewModelPresenter

        int IDynamicViewModelPresenter.Priority => int.MaxValue;

        INavigationOperation IDynamicViewModelPresenter.TryShowAsync(IViewModel viewModel, IDataContext context, IViewModelPresenter parentPresenter)
        {
            parentPresenter.DynamicPresenters.Remove(this);
            _application.RootVisual = (UIElement)ViewManager.GetOrCreateView(viewModel, null, context);
            return new NavigationOperation();
        }

        #endregion

        #region Methods

        protected override void InitializeInternal()
        {
            var application = CreateApplication();
            var iocContainer = CreateIocContainer();
            application.Initialize(_platform, iocContainer, GetAssemblies().ToArrayEx(), InitializationContext ?? DataContext.Empty);
        }

        public virtual void Start(IDataContext context = null)
        {
            Initialize();
            var app = MvvmApplication.Current;
            app.IocContainer.Get<IViewModelPresenter>().DynamicPresenters.Add(this);
            app.Start(context);
        }

        protected virtual ICollection<Assembly> GetAssemblies()
        {
            var assemblies = new HashSet<Assembly>();
            foreach (AssemblyPart part in Deployment.Current.Parts)
            {
                StreamResourceInfo info = Application.GetResourceStream(new Uri(part.Source, UriKind.Relative));
                assemblies.Add(part.Load(info.Stream));
            }
            return assemblies;
        }

        private void ApplicationOnStartup(object sender, StartupEventArgs args)
        {
            var application = sender as Application;
            if (application != null)
                application.Startup -= ApplicationOnStartup;
            if (AutoStart)
                Start();
        }

        #endregion
    }
}
