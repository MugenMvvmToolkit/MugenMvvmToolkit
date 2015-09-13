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
    /// <summary>
    ///     Represents the base class that is used to start MVVM application.
    /// </summary>
    public abstract class SilverlightBootstrapperBase : BootstrapperBase
    {
        #region Fields

        /// <summary>
        /// Gets the name of binding assembly.
        /// </summary>
        protected const string BindingAssemblyName = "MugenMvvmToolkit.Silverlight.Binding";
        private readonly Application _application;
        private readonly PlatformInfo _platform;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="SilverlightBootstrapperBase" /> class.
        /// </summary>
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

        /// <summary>
        ///     Indicates that bootstrapper should call the Start method when Application.Startup is raised.
        /// </summary>
        public bool AutoStart { get; set; }

        /// <summary>
        ///     Gets the current application.
        /// </summary>
        protected Application Application
        {
            get { return _application; }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Initializes the current bootstrapper.
        /// </summary>
        protected override void InitializeInternal()
        {
            var application = CreateApplication();
            var iocContainer = CreateIocContainer();
            application.Initialize(_platform, iocContainer, GetAssemblies().ToArrayEx(), InitializationContext ?? DataContext.Empty);
        }

        /// <summary>
        ///     Starts the current bootstrapper.
        /// </summary>
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

        /// <summary>
        ///     Gets the application assemblies.
        /// </summary>
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