#region Copyright
// ****************************************************************************
// <copyright file="WinRTBootstrapperBase.cs">
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
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using JetBrains.Annotations;
using MugenMvvmToolkit.Infrastructure.Navigation;
using MugenMvvmToolkit.Infrastructure.Presenters;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Infrastructure
{
    public abstract class WinRTBootstrapperBase : BootstrapperBase
    {
        #region Fields

        /// <summary>
        /// Gets the name of binding assembly.
        /// </summary>
        protected const string BindingAssemblyName = "MugenMvvmToolkit.Binding.WinRT";
        private readonly Frame _rootFrame;
        private readonly bool _overrideAssemblies;
        private List<Assembly> _assemblies;
        private PlatformInfo _platform;

        #endregion

        #region Constructors

        static WinRTBootstrapperBase()
        {
            DynamicMultiViewModelPresenter.CanShowViewModelDefault = CanShowViewModelTabPresenter;
            DynamicViewModelNavigationPresenter.CanShowViewModelDefault = CanShowViewModelNavigationPresenter;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="WinRTBootstrapperBase" /> class.
        /// </summary>
        protected WinRTBootstrapperBase([NotNull] Frame rootFrame, bool overrideAssemblies)
        {
            Should.NotBeNull(rootFrame, "rootFrame");
            _rootFrame = rootFrame;
            _overrideAssemblies = overrideAssemblies;
        }

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
        ///     Starts the current bootstraper.
        /// </summary>
        protected override void OnInitialize()
        {
            base.OnInitialize();
            var service = CreateNavigationService(_rootFrame);
            if (service != null)
                IocContainer.BindToConstant(service);
        }

        /// <summary>
        ///     Gets the application assemblies.
        /// </summary>
        protected override ICollection<Assembly> GetAssemblies()
        {
            if (_assemblies != null)
                return _assemblies;
            var assemblies = new HashSet<Assembly> { GetType().GetAssembly(), typeof(WinRTBootstrapperBase).GetAssembly() };
            var application = Application.Current;
            if (application != null)
                assemblies.Add(application.GetType().GetAssembly());
            TryLoadAssembly(BindingAssemblyName, assemblies);
            return assemblies;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Starts the current bootstrapper.
        /// </summary>
        public virtual void Start()
        {
            Initialize();
            CreateMainViewModel(GetMainViewModelType()).ShowAsync((model, result) => model.Dispose(), context: InitializationContext);
        }

        /// <summary>
        ///     Initializes the current bootstraper asynchronously.
        /// </summary>
        public async Task InitializeAsync()
        {
            if (!_overrideAssemblies)
                _assemblies = await GetAssembliesAsync();
            Initialize();
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
        ///     Creates an instance of <see cref="INavigationService" />.
        /// </summary>
        [CanBeNull]
        protected virtual INavigationService CreateNavigationService(Frame frame)
        {
            return new FrameNavigationService(frame);
        }

        /// <summary>
        ///     Gets the type of main view model.
        /// </summary>
        [NotNull]
        protected abstract Type GetMainViewModelType();

        private static bool CanShowViewModelTabPresenter(IViewModel viewModel, IDataContext dataContext, IViewModelPresenter arg3)
        {
            var viewName = viewModel.GetViewName(dataContext);
            var container = viewModel.GetIocContainer(true);
            var mappingProvider = container.Get<IViewMappingProvider>();
            var mappingItem = mappingProvider.FindMappingForViewModel(viewModel.GetType(), viewName, false);
            return mappingItem == null || !typeof(Page).IsAssignableFrom(mappingItem.ViewType);
        }

        private static bool CanShowViewModelNavigationPresenter(IViewModel viewModel, IDataContext dataContext, IViewModelPresenter arg3)
        {
            var viewName = viewModel.GetViewName(dataContext);
            var container = viewModel.GetIocContainer(true);
            var mappingProvider = container.Get<IViewMappingProvider>();
            var mappingItem = mappingProvider.FindMappingForViewModel(viewModel.GetType(), viewName, false);
            return mappingItem != null && typeof(Page).IsAssignableFrom(mappingItem.ViewType);
        }

        private static async Task<List<Assembly>> GetAssembliesAsync()
        {
            var assemblies = new List<Assembly>();
            try
            {
                var files = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFilesAsync();

                foreach (var file in files)
                {
                    if ((file.FileType == ".dll") || (file.FileType == ".exe"))
                    {
                        var name = new AssemblyName { Name = Path.GetFileNameWithoutExtension(file.Name) };
                        Assembly asm = Assembly.Load(name);
                        if (asm.IsToolkitAssembly())
                            assemblies.Add(asm);
                    }
                }
            }
            catch (Exception e)
            {
                Tracer.Error(e.Flatten(true));
            }

            return assemblies;
        }

        #endregion
    }
}