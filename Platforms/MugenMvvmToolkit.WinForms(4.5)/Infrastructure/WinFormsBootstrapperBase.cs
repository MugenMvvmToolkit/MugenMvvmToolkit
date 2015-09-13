#region Copyright

// ****************************************************************************
// <copyright file="WinFormsBootstrapperBase.cs">
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
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Infrastructure.Presenters;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.WinForms.Infrastructure
{
    /// <summary>
    ///     Represents the base class that is used to start MVVM application.
    /// </summary>
    public abstract class WinFormsBootstrapperBase : BootstrapperBase
    {
        #region Fields

        private readonly PlatformInfo _platform;

        #endregion

        #region Constructors

        static WinFormsBootstrapperBase()
        {
            ReflectionExtensions.GetTypesDefault = assembly => assembly.GetTypes();
            DynamicViewModelNavigationPresenter.CanShowViewModelDefault = (model, context, arg3) => false;
            BindingServiceProvider.ValueConverter = BindingReflectionExtensions.Convert;
            ViewManager.ViewCleared += OnViewCleared;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="WinFormsBootstrapperBase" /> class.
        /// </summary>
        protected WinFormsBootstrapperBase(bool autoRunApplication = true, PlatformInfo platform = null)
        {
            _platform = platform ?? PlatformExtensions.GetPlatformInfo();
            AutoRunApplication = autoRunApplication;
            ShutdownOnMainViewModelClose = true;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Indicates that bootstrapper should call the Application.Run method after start.
        /// </summary>
        public bool AutoRunApplication { get; set; }

        /// <summary>
        ///     An application shuts down when either the main view model closes, or Application.Exit() is called.
        /// </summary>
        public bool ShutdownOnMainViewModelClose { get; set; }

        #endregion

        #region Overrides of BootstrapperBase

        /// <summary>
        ///     Initializes the current bootstrapper.
        /// </summary>
        protected override void InitializeInternal()
        {
            var application = CreateApplication();
            var iocContainer = CreateIocContainer();
            application.Initialize(_platform, iocContainer, GetAssemblies().ToArrayEx(), InitializationContext ?? DataContext.Empty);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Starts the current bootstrapper.
        /// </summary>
        public virtual void Start()
        {
            Initialize();
            var app = MvvmApplication.Current;
            var ctx = new DataContext(app.Context);
            if (!ctx.Contains(NavigationConstants.IsDialog))
                ctx.Add(NavigationConstants.IsDialog, false);
            app.IocContainer
                .Get<IViewModelProvider>()
                .GetViewModel(app.GetStartViewModelType(), ctx)
                .ShowAsync((model, result) =>
                {
                    model.Dispose();
                    if (ShutdownOnMainViewModelClose)
                        Application.Exit();
                }, context: ctx);
            if (AutoRunApplication)
                Application.Run();
        }

        /// <summary>
        ///     Gets the application assemblies.
        /// </summary>
        protected virtual ICollection<Assembly> GetAssemblies()
        {
            var assemblies = new HashSet<Assembly>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().SkipFrameworkAssemblies())
            {
                if (assemblies.Add(assembly))
                    assemblies.AddRange(
                        assembly.GetReferencedAssemblies().Select(Assembly.Load).SkipFrameworkAssemblies());
            }
            return assemblies;
        }

        private static void OnViewCleared(IViewManager viewManager, IViewModel viewModel, object arg3, IDataContext arg4)
        {
            var control = arg3 as Control;
            if (control != null)
                ClearBindingsRecursively(control.Controls);
            var disposable = arg3 as IDisposable;
            if (disposable != null)
                disposable.Dispose();
        }

        private static void ClearBindingsRecursively(Control.ControlCollection collection)
        {
            if (collection == null)
                return;
            foreach (var item in collection.OfType<Control>())
            {
                ClearBindingsRecursively(item.Controls);
                item.ClearBindings(true, true);
            }
        }

        #endregion
    }
}