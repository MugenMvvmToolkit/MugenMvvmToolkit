#region Copyright

// ****************************************************************************
// <copyright file="UnitTestBase.cs">
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
    public abstract class UnitTestBase
    {
        #region Nested Types

        public sealed class DefaultUnitTestModule : InitializationModuleBase
        {
            #region Methods

            public override bool Load(IModuleContext context)
            {
                return context.IsSupported(LoadMode.UnitTest) && base.Load(context);
            }

            #endregion
        }

        protected class UnitTestApp : MvvmApplication
        {
            #region Fields

            private readonly IModule[] _modules;

            #endregion

            #region Constructors

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

        protected IIocContainer IocContainer => ServiceProvider.IocContainer;

        protected IViewModelProvider ViewModelProvider { get; set; }

        #endregion

        #region Methods

        protected void Initialize([NotNull] IIocContainer iocContainer, params IModule[] modules)
        {
            Initialize(iocContainer, PlatformInfo.UnitTest, modules);
        }

        protected void Initialize([NotNull] IIocContainer iocContainer, PlatformInfo platform, params IModule[] modules)
        {
            Initialize(new UnitTestApp(modules: modules), iocContainer, platform, typeof(UnitTestApp).GetAssembly(),
                GetType().GetAssembly());
        }

        protected void Initialize([NotNull] IMvvmApplication application, [NotNull] IIocContainer iocContainer,
            params Assembly[] assemblies)
        {
            Initialize(application, iocContainer, PlatformInfo.UnitTest, assemblies);
        }

        protected void Initialize([NotNull] IMvvmApplication application, [NotNull] IIocContainer iocContainer,
            PlatformInfo platform, params Assembly[] assemblies)
        {
            Should.NotBeNull(application, nameof(application));
            Should.NotBeNull(iocContainer, nameof(iocContainer));
            application.Initialize(platform ?? PlatformInfo.UnitTest, iocContainer, assemblies, DataContext.Empty);
            if (ViewModelProvider == null)
            {
                IViewModelProvider viewModelProvider;
                ViewModelProvider = iocContainer.TryGet(out viewModelProvider) ? viewModelProvider : new ViewModelProvider(iocContainer);
            }
        }

        protected internal IViewModel GetViewModel([NotNull] GetViewModelDelegate<IViewModel> getViewModel,
            IViewModel parentViewModel = null, ObservationMode? observationMode = null, params DataConstantValue[] parameters)
        {
            return ViewModelProvider.GetViewModel(getViewModel, parentViewModel, observationMode, parameters);
        }

        protected internal T GetViewModel<T>([NotNull] GetViewModelDelegate<T> getViewModelGeneric,
            IViewModel parentViewModel = null, ObservationMode? observationMode = null, params DataConstantValue[] parameters)
            where T : class, IViewModel
        {
            return ViewModelProvider.GetViewModel(getViewModelGeneric, parentViewModel, observationMode, parameters);
        }

        protected internal IViewModel GetViewModel([NotNull] Type viewModelType,
            IViewModel parentViewModel = null, ObservationMode? observationMode = null, params DataConstantValue[] parameters)
        {
            return ViewModelProvider.GetViewModel(viewModelType, parentViewModel, observationMode, parameters);
        }

        protected internal T GetViewModel<T>(IViewModel parentViewModel = null, ObservationMode? observationMode = null, params DataConstantValue[] parameters)
            where T : IViewModel
        {
            return ViewModelProvider.GetViewModel<T>(parentViewModel, observationMode, parameters);
        }

        #endregion
    }
}
