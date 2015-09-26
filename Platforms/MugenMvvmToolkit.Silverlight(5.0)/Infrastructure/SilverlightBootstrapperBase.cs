#region Copyright

// ****************************************************************************
// <copyright file="SilverlightBootstrapperBase.cs">
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
using System.Windows;
using System.Windows.Resources;
using JetBrains.Annotations;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Silverlight.Infrastructure
{
    public abstract class SilverlightBootstrapperBase : BootstrapperBase
    {
        #region Fields

        protected const string BindingAssemblyName = "MugenMvvmToolkit.Silverlight.Binding";
        private readonly Application _application;
        private readonly PlatformInfo _platform;

        #endregion

        #region Constructors

        protected SilverlightBootstrapperBase([NotNull] Application application, bool autoStart = true, PlatformInfo platform = null)
        {
            Should.NotBeNull(application, "application");
            _application = application;
            _platform = platform ?? PlatformExtensions.GetPlatformInfo();
            AutoStart = autoStart;
            application.Startup += ApplicationOnStartup;
        }

        #endregion

        #region Properties

        public bool AutoStart { get; set; }

        protected Application Application
        {
            get { return _application; }
        }

        #endregion

        #region Methods

        protected override void InitializeInternal()
        {
            var application = CreateApplication();
            var iocContainer = CreateIocContainer();
            application.Initialize(_platform, iocContainer, GetAssemblies().ToArrayEx(), InitializationContext ?? DataContext.Empty);
        }

        public virtual void Start()
        {
            Initialize();
            var app = MvvmApplication.Current;
            var ctx = new DataContext(app.Context);
            var viewModelType = app.GetStartViewModelType();
            var viewModel = app.IocContainer
               .Get<IViewModelProvider>()
               .GetViewModel(viewModelType, ctx);
            _application.RootVisual = (UIElement)ViewManager.GetOrCreateView(viewModel, false, ctx);
        }

        protected virtual ICollection<Assembly> GetAssemblies()
        {
            var listAssembly = new List<Assembly>();
            foreach (AssemblyPart part in Deployment.Current.Parts)
            {
                StreamResourceInfo info = Application.GetResourceStream(new Uri(part.Source, UriKind.Relative));
                Assembly assembly = part.Load(info.Stream);
                if (assembly.IsToolkitAssembly())
                    listAssembly.Add(assembly);
            }
            return listAssembly;
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
