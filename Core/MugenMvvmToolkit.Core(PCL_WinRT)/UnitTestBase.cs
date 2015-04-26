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
        #region Nested types

        /// <summary>
        /// Represents the default unit test module.
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

        #endregion

        #region Fields

        private IIocContainer _iocContainer;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the current <see cref="IIocContainer" />.
        /// </summary>
        protected IIocContainer IocContainer
        {
            get { return _iocContainer; }
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
            Should.NotBeNull(iocContainer, "iocContainer");
            ServiceProvider.DesignTimeManager = new DesignTimeManagerImpl(platform);
            if (modules != null)
                CreateModuleContext(iocContainer).LoadModules(modules);
            _iocContainer = iocContainer;
            if (ViewModelProvider == null)
                ViewModelProvider = IocContainer.CanResolve<IViewModelProvider>()
                    ? IocContainer.Get<IViewModelProvider>()
                    : new ViewModelProvider(IocContainer);
            ServiceProvider.Initialize(iocContainer, platform ?? PlatformInfo.UnitTest);
        }

        /// <summary>
        ///     Creates an instance of <see cref="IModuleContext" />.
        /// </summary>
        /// <returns>An instance of <see cref="IModuleContext" />.</returns>
        protected virtual IModuleContext CreateModuleContext(IIocContainer iocContainer)
        {
            return new ModuleContext(PlatformInfo.UnitTest, LoadMode.UnitTest, iocContainer, null,
                new[] { GetType().GetAssembly() });
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
            IocContainerCreationMode? containerCreationMode = null, params DataConstantValue[] parameters) where T : IViewModel
        {
            return ViewModelProvider.GetViewModel(getViewModelGeneric, parentViewModel, observationMode, containerCreationMode,
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