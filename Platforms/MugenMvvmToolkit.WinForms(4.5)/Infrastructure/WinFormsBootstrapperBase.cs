#region Copyright
// ****************************************************************************
// <copyright file="WinFormsBootstrapperBase.cs">
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using JetBrains.Annotations;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure.Presenters;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Infrastructure
{
    /// <summary>
    ///     Represents the base class that is used to start MVVM application.
    /// </summary>
    public abstract class WinFormsBootstrapperBase : BootstrapperBase
    {
        #region Fields

        private readonly bool _autoRunApplication;
        private PlatformInfo _platform;

        #endregion

        #region Constructors

        static WinFormsBootstrapperBase()
        {
            ReflectionExtensions.GetTypesDefault = assembly => assembly.GetTypes();
            DynamicViewModelNavigationPresenter.CanShowViewModelDefault = (model, context, arg3) => false;
            ViewManager.DisposeView = true;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="WinFormsBootstrapperBase" /> class.
        /// </summary>
        protected WinFormsBootstrapperBase(bool autoRunApplication = true)
        {
            _autoRunApplication = autoRunApplication;
            ShutdownOnMainViewModelClose = autoRunApplication;
        }

        #endregion

        #region Properties

        /// <summary>
        /// An application shuts down when either the main view model closes, or Application.Exit() is called.
        /// </summary>
        public bool ShutdownOnMainViewModelClose { get; set; }

        #endregion

        #region Overrides of BootstrapperBase

        /// <summary>
        ///     Gets the current platform.
        /// </summary>
        public override PlatformInfo Platform
        {
            get
            {
                if (_platform == null)
                    _platform = PlatformExtensions.GetPlatformInfo();
                return _platform;
            }
        }

        /// <summary>
        ///     Gets the application assemblies.
        /// </summary>
        protected override ICollection<Assembly> GetAssemblies()
        {
            var assemblies = new HashSet<Assembly>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies().SkipFrameworkAssemblies())
            {
                if (assemblies.Add(assembly))
                    assemblies.AddRange(assembly.GetReferencedAssemblies().Select(Assembly.Load).SkipFrameworkAssemblies());
            }
            return assemblies;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Starts the current bootstrapper.
        /// </summary>
        public virtual void Start()
        {
            InitializationContext = InitializationContext.ToNonReadOnly();
            if (!InitializationContext.Contains(NavigationConstants.IsDialog))
                InitializationContext.Add(NavigationConstants.IsDialog, false);
            Initialize();
            var viewModelType = GetMainViewModelType();
            CreateMainViewModel(viewModelType)
                .ShowAsync((model, result) =>
                {
                    model.Dispose();
                    if (ShutdownOnMainViewModelClose)
                        Application.Exit();
                }, context: InitializationContext);
            if (_autoRunApplication)
                Application.Run();
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

        #endregion
    }
}