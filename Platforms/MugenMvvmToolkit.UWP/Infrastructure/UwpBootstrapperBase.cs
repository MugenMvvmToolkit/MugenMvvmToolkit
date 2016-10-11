#region Copyright

// ****************************************************************************
// <copyright file="WinRTBootstrapperBase.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Infrastructure.Presenters;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;
using MugenMvvmToolkit.UWP.Infrastructure.Navigation;
using MugenMvvmToolkit.UWP.Interfaces.Navigation;

namespace MugenMvvmToolkit.UWP.Infrastructure
{
    public abstract class UwpBootstrapperBase : BootstrapperBase
    {
        #region Fields

        protected internal const string BindingAssemblyName = "MugenMvvmToolkit.UWP.Binding";
        private readonly Frame _rootFrame;
        private readonly bool _overrideAssemblies;
        private readonly PlatformInfo _platform;
        private HashSet<Assembly> _assemblies;

        #endregion

        #region Constructors

        static UwpBootstrapperBase()
        {
            SetDefaultPlatformValues();
        }

        protected UwpBootstrapperBase([CanBeNull] Frame rootFrame, bool overrideAssemblies, PlatformInfo platform = null)
        {
            _rootFrame = rootFrame;
            _overrideAssemblies = overrideAssemblies;
            _platform = platform ?? PlatformExtensions.GetPlatformInfo();
        }

        #endregion

        #region Properties

        protected Frame RootFrame => _rootFrame;

        #endregion

        #region Overrides of BootstrapperBase

        protected override void InitializeInternal()
        {
            var application = CreateApplication();
            var iocContainer = CreateIocContainer();
            var service = CreateNavigationService(_rootFrame);
            if (service != null)
                iocContainer.BindToConstant(service);
            application.Initialize(_platform, iocContainer, GetAssemblies().ToArrayEx(), InitializationContext ?? DataContext.Empty);
        }

        #endregion

        #region Methods

        public virtual void Start(IDataContext context = null)
        {
            Initialize();
            MvvmApplication.Current.Start(context);
        }

        public async Task InitializeAsync()
        {
            if (!_overrideAssemblies)
                _assemblies = await GetAssembliesAsync();
            Initialize();
        }

        protected virtual ICollection<Assembly> GetAssemblies()
        {
            if (_assemblies != null)
                return _assemblies;
            var assemblies = new HashSet<Assembly> { GetType().GetAssembly(), typeof(UwpBootstrapperBase).GetAssembly() };
            var application = Application.Current;
            if (application != null)
                assemblies.Add(application.GetType().GetAssembly());
            TryLoadAssembly(BindingAssemblyName, assemblies);
            return assemblies;
        }

        [CanBeNull]
        protected virtual INavigationService CreateNavigationService(Frame frame)
        {
            if (frame == null)
                return null;
            return new FrameNavigationService(frame, true);
        }

        internal static void SetDefaultPlatformValues()
        {
            DynamicMultiViewModelPresenter<IViewModel>.CanShowViewModelDefault = CanShowViewModelTabPresenter;
            DynamicViewModelNavigationPresenter.CanShowViewModelDefault = CanShowViewModelNavigationPresenter;
        }

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

        private static async Task<HashSet<Assembly>> GetAssembliesAsync()
        {
            var assemblies = new HashSet<Assembly>();
            var files = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFilesAsync();
            foreach (var file in files)
            {
                try
                {
                    if ((file.FileType == ".dll") || (file.FileType == ".exe"))
                    {
                        var name = new AssemblyName { Name = Path.GetFileNameWithoutExtension(file.Name) };
                        assemblies.Add(Assembly.Load(name));
                    }

                }
                catch
                {
                    ;
                }
            }
            return assemblies;
        }

        #endregion
    }
}
