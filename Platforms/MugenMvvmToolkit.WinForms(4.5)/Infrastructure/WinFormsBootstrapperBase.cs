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
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Infrastructure.Presenters;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.WinForms.Infrastructure
{
    /// <summary>
    ///     Represents the base class that is used to start MVVM application.
    /// </summary>
    public abstract class WinFormsBootstrapperBase : BootstrapperBase
    {
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
        protected WinFormsBootstrapperBase(bool autoRunApplication = true)
            : base(PlatformExtensions.GetPlatformInfo())
        {
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
            if (AutoRunApplication)
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