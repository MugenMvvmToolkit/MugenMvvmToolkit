#region Copyright

// ****************************************************************************
// <copyright file="UnitTestBase.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Modules;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit
{
    /// <summary>
    ///     Represents the class that is used for unit tests.
    /// </summary>
    public abstract class UnitTestBase
    {
        #region Nested Types

        /// <summary>
        ///     Represents the default unit test module.
        /// </summary>
        public sealed class DefaultUnitTestModule : InitializationModuleBase
        {
            #region Constructors

            /// <summary>
            ///     Initializes a new instance of the <see cref="DefaultUnitTestModule" /> class.
            /// </summary>
            public DefaultUnitTestModule(int priority = InitializationModulePriority)
                : base(LoadMode.UnitTest, priority)
            {
            }

            #endregion
        }

        /// <summary>
        ///     Represents the default implementation of unit test <see cref="IMvvmApplication"/>.
        /// </summary>
        protected class UnitTestApp : MvvmApplication
        {
            #region Fields

            private readonly IModule[] _modules;

            #endregion

            #region Constructors

            /// <summary>
            ///     Initializes a new instance of the <see cref="UnitTestApp" /> class.
            /// </summary>
            public UnitTestApp(LoadMode mode = LoadMode.UnitTest, params IModule[] modules)
                : base(mode)
            {
                _modules = modules;
            }

            #endregion

            #region Methods

            protected override IList<IModule> GetModules(IList<Assembly> assemblies)
            {
                if (_modules.IsNullOrEmpty())
                    return base.GetModules(assemblies);
                return _modules;
            }

            protected override IModuleContext CreateModuleContext(IList<Assembly> assemblies)
            {
                return new ModuleContext(PlatformInfo.UnitTest, LoadMode.UnitTest, IocContainer, null, assemblies);
            }

            public override Type GetStartViewModelType()
            {
                return typeof(IViewModel);
            }

            #endregion
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the current <see cref="IIocContainer" />.
        /// </summary>
        protected IIocContainer IocContainer
        {
            get { return MvvmApplication.Current.IocContainer; }
        }

        /// <summary>
        ///     Gets the <see cref="IViewModelProvider" />.
        /// </summary>
        protected IViewModelProvider ViewModelProvider { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///     Initializes the current unit-test.
        /// </summary>
        protected void Initialize([NotNull] IIocContainer iocContainer, params IModule[] modules)
        {
            Initialize(iocContainer, PlatformInfo.UnitTest, modules);
        }

        /// <summary>
        ///     Initializes the current unit-test.
        /// </summary>
        protected void Initialize([NotNull] IIocContainer iocContainer, PlatformInfo platform, params IModule[] modules)
        {
            Initialize(new UnitTestApp(modules: modules), iocContainer, platform, typeof(UnitTestApp).GetAssembly(),
                GetType().GetAssembly());
        }

        /// <summary>
        ///     Initializes the current unit-test.
        /// </summary>
        protected void Initialize([NotNull] IMvvmApplication application, [NotNull] IIocContainer iocContainer,
            params Assembly[] assemblies)
        {
            Initialize(application, iocContainer, PlatformInfo.UnitTest, assemblies);
        }

        /// <summary>
        ///     Initializes the current unit-test.
        /// </summary>
        protected void Initialize([NotNull] IMvvmApplication application, [NotNull] IIocContainer iocContainer,
            PlatformInfo platform, params Assembly[] assemblies)
        {
            Should.NotBeNull(application, "application");
            Should.NotBeNull(iocContainer, "iocContainer");
            ServiceProvider.DesignTimeManager = new DesignTimeManagerImpl(platform);
            application.Initialize(platform ?? PlatformInfo.UnitTest, iocContainer, assemblies, DataContext.Empty);
            if (ViewModelProvider == null)
            {
                IViewModelProvider viewModelProvider;
                ViewModelProvider = iocContainer.TryGet(out viewModelProvider)
                    ? viewModelProvider
                    : new ViewModelProvider(iocContainer);
            }
        }

        /// <summary>
        ///     Creates an instance of the specified view model.
        /// </summary>
        /// <param name="getViewModel">The specified delegate to create view model.</param>
        /// <param name="parentViewModel">The parent view model.</param>
        /// <param name="containerCreationMode">The value that is responsible to initialize the IocContainer.</param>
        /// <param name="observationMode">The value that is responsible for listen messages in created view model.</param>
        /// <param name="parameters">The specified parameters to get view-model.</param>
        /// <returns>
        ///     An instance of <see cref="IViewModel" />.
        /// </returns>
        protected internal IViewModel GetViewModel([NotNull] GetViewModelDelegate<IViewModel> getViewModel,
            IViewModel parentViewModel = null, ObservationMode? observationMode = null,
            IocContainerCreationMode? containerCreationMode = null, params DataConstantValue[] parameters)
        {
            return ViewModelProvider.GetViewModel(getViewModel, parentViewModel, observationMode, containerCreationMode,
                parameters);
        }

        /// <summary>
        ///     Creates an instance of the specified view model.
        /// </summary>
        /// <param name="getViewModelGeneric">The specified delegate to create view model.</param>
        /// <param name="parentViewModel">The parent view model.</param>
        /// <param name="containerCreationMode">The value that is responsible to initialize the IocContainer.</param>
        /// <param name="observationMode">The value that is responsible for listen messages in created view model.</param>
        /// <param name="parameters">The specified parameters to get view-model.</param>
        /// <returns>
        ///     An instance of <see cref="IViewModel" />.
        /// </returns>
        protected internal T GetViewModel<T>([NotNull] GetViewModelDelegate<T> getViewModelGeneric,
            IViewModel parentViewModel = null, ObservationMode? observationMode = null,
            IocContainerCreationMode? containerCreationMode = null, params DataConstantValue[] parameters)
            where T : class, IViewModel
        {
            return ViewModelProvider.GetViewModel(getViewModelGeneric, parentViewModel, observationMode,
                containerCreationMode,
                parameters);
        }

        /// <summary>
        ///     Creates an instance of the specified view model.
        /// </summary>
        /// <param name="viewModelType">The type of view model.</param>
        /// <param name="parentViewModel">The parent view model.</param>
        /// <param name="containerCreationMode">The value that is responsible to initialize the IocContainer.</param>
        /// <param name="observationMode">The value that is responsible for listen messages in created view model.</param>
        /// <param name="parameters">The specified parameters to get view-model.</param>
        /// <returns>
        ///     An instance of <see cref="IViewModel" />.
        /// </returns>
        protected internal IViewModel GetViewModel([NotNull] Type viewModelType,
            IViewModel parentViewModel = null, ObservationMode? observationMode = null,
            IocContainerCreationMode? containerCreationMode = null, params DataConstantValue[] parameters)
        {
            return ViewModelProvider.GetViewModel(viewModelType, parentViewModel, observationMode, containerCreationMode,
                parameters);
        }

        /// <summary>
        ///     Creates an instance of the specified view model.
        /// </summary>
        /// <typeparam name="T">The type of view model.</typeparam>
        /// <param name="parentViewModel">The parent view model.</param>
        /// <param name="containerCreationMode">The value that is responsible to initialize the IocContainer.</param>
        /// <param name="observationMode">The value that is responsible for listen messages in created view model.</param>
        /// <param name="parameters">The specified parameters to get view-model.</param>
        /// <returns>
        ///     An instance of <see cref="IViewModel" />.
        /// </returns>
        protected internal T GetViewModel<T>(IViewModel parentViewModel = null, ObservationMode? observationMode = null,
            IocContainerCreationMode? containerCreationMode = null, params DataConstantValue[] parameters)
            where T : IViewModel
        {
            return ViewModelProvider.GetViewModel<T>(parentViewModel, observationMode, containerCreationMode, parameters);
        }

        #endregion
    }
}