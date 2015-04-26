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
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.ViewModels;

namespace MugenMvvmToolkit.Infrastructure
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
        protected const string BindingAssemblyName = "MugenMvvmToolkit.Binding.Silverlight";
        private readonly Application _application;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="SilverlightBootstrapperBase" /> class.
        /// </summary>
        protected SilverlightBootstrapperBase([NotNull] Application application, bool autoStart = true)
            : base(PlatformExtensions.GetPlatformInfo())
        {
            Should.NotBeNull(application, "application");
            _application = application;
            AutoStart = autoStart;
            application.Startup += ApplicationOnStartup;
            application.Exit += ApplicationOnExit;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Indicates that bootstrapper should call the Start method when Application.Startup is raised.
        /// </summary>
        public bool AutoStart { get; set; }

        #endregion

        #region Overrides of BootstrapperBase

        /// <summary>
        ///     Gets the application assemblies.
        /// </summary>
        protected override ICollection<Assembly> GetAssemblies()
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

        #endregion

        #region Methods

        /// <summary>
        ///     Starts the current bootstrapper.
        /// </summary>
        public virtual void Start()
        {
            Initialize();
            var viewModelType = GetMainViewModelType();
            _application.RootVisual = (UIElement)ViewManager.GetOrCreateView(CreateMainViewModel(viewModelType), false, InitializationContext);
        }

        /// <summary>
        ///     Creates the main view model.
        /// </summary>
        [NotNull]
        protected virtual IViewModel CreateMainViewModel([NotNull] Type viewModelType)
        {
            return IocContainer
                .Get<IViewModelProvider>()
                .GetViewModel(viewModelType, InitializationContext);
        }

        /// <summary>
        ///     Gets the type of main view model.
        /// </summary>
        [NotNull]
        protected abstract Type GetMainViewModelType();

        private void ApplicationOnStartup(object sender, StartupEventArgs args)
        {
            var application = sender as Application;
            if (application != null)
                application.Startup -= ApplicationOnStartup;
            if (AutoStart)
                Start();
        }

        private void ApplicationOnExit(object sender, EventArgs eventArgs)
        {
            var application = sender as Application;
            if (application != null)
                application.Exit -= ApplicationOnExit;
            Stop();
        }

        #endregion
    }
}