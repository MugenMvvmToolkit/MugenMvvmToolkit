#region Copyright

// ****************************************************************************
// <copyright file="XamarinFormsBootstrapperBase.cs">
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
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;
using MugenMvvmToolkit.Xamarin.Forms.Infrastructure.Presenters;
using MugenMvvmToolkit.Xamarin.Forms.Interfaces.Presenters;
using MugenMvvmToolkit.Xamarin.Forms.Interfaces.Views;
using MugenMvvmToolkit.Xamarin.Forms.Models.Messages;
using Xamarin.Forms;

namespace MugenMvvmToolkit.Xamarin.Forms.Infrastructure
{
    public abstract class XamarinFormsBootstrapperBase : BootstrapperBase
    {
        #region Nested types

        public interface IPlatformService
        {
            Func<MemberInfo, Type, object, object> ValueConverter { get; }

            PlatformInfo GetPlatformInfo();

            ICollection<Assembly> GetAssemblies();

            void Initialize();
        }

        #endregion

        #region Fields

        private readonly IPlatformService _platformService;

        #endregion

        #region Constructors

        static XamarinFormsBootstrapperBase()
        {
            ApplicationSettings.MultiViewModelPresenterCanShowViewModel = CanShowViewModelTabPresenter;
            ApplicationSettings.NavigationPresenterCanShowViewModel = CanShowViewModelNavigationPresenter;
            ApplicationSettings.ViewManagerClearDataContext = true;
            var getContext = ToolkitExtensions.GetDataContext;
            ToolkitExtensions.GetDataContext = o =>
            {
                var bindableObject = o as BindableObject;
                if (bindableObject == null)
                    return getContext.Invoke(o);
                return bindableObject.BindingContext;
            };
            var setContext = ToolkitExtensions.SetDataContext;
            ToolkitExtensions.SetDataContext = (item, value) =>
            {
                var bindableObject = item as BindableObject;
                if (bindableObject == null)
                    setContext(item, value);
                else
                    bindableObject.BindingContext = value;
            };
        }

        protected XamarinFormsBootstrapperBase(bool isDesignMode, PlatformInfo platform) : base(isDesignMode)
        {
            Platform = platform ?? PlatformInfo.Unknown;
        }

        protected XamarinFormsBootstrapperBase(IPlatformService platformService, bool isDesignMode = false)
            : this(isDesignMode, platformService?.GetPlatformInfo())
        {
            _platformService = platformService;
            WrapToNavigationPage = true;
        }

        #endregion

        #region Properties

        [CanBeNull]
        public new static XamarinFormsBootstrapperBase Current => BootstrapperBase.Current as XamarinFormsBootstrapperBase;

        public bool WrapToNavigationPage { get; set; }

        public Func<IIocContainer, IDynamicViewModelPresenter> RootPresenterFactory { get; set; }

        public Func<Page, NavigationPage> NavigationPageFactory { get; set; }

        protected override PlatformInfo Platform { get; }

        #endregion

        #region Methods

        protected override void InitializeInternal()
        {
            base.InitializeInternal();
            if (_platformService != null)
            {
                _platformService.Initialize();
                XamarinFormsToolkitExtensions.ValueConverter = _platformService.ValueConverter;
            }
            if (!IsDesignMode)
            {
                var rootPresenter = GetRootPresenter();
                if (rootPresenter != null)
                    IocContainer.Get<IViewModelPresenter>().DynamicPresenters.Add(rootPresenter);
            }
        }

        protected override void UpdateAssemblies(HashSet<Assembly> assemblies)
        {
            base.UpdateAssemblies(assemblies);
            TryLoadAssemblyByType("AttachedMembers", "MugenMvvmToolkit.Xamarin.Forms.Binding", assemblies);
            if (IsDesignMode)
            {
                TryLoadAssemblyByType("PlatformBootstrapperService", "MugenMvvmToolkit.Xamarin.Forms.Android", assemblies);
                TryLoadAssemblyByType("PlatformBootstrapperService", "MugenMvvmToolkit.Xamarin.Forms.iOS", assemblies);
                TryLoadAssemblyByType("PlatformBootstrapperService", "MugenMvvmToolkit.Xamarin.Forms.UWP", assemblies);
                TryLoadAssemblyByType("PlatformBootstrapperService", "MugenMvvmToolkit.Xamarin.Forms.WinPhone", assemblies);
                TryLoadAssemblyByType("PlatformBootstrapperService", "MugenMvvmToolkit.Xamarin.Forms.WinRT", assemblies);
                TryLoadAssemblyByType("MugenMvvmToolkit.Xamarin.Forms.WinRT.PlatformBootstrapperService, MugenMvvmToolkit.Xamarin.Forms.WinRT.Phone", assemblies);
            }
            if (Application.Current != null)
                assemblies.Add(Application.Current.GetType().GetAssembly());
            if (_platformService != null)
            {
                assemblies.Add(_platformService.GetType().GetAssembly());
                assemblies.AddRange(_platformService.GetAssemblies().Where(x => !x.IsDynamic));
            }
        }

        public virtual void Start()
        {
            if (Current != null && !ReferenceEquals(Current, this))
            {
                Current.Start();
                return;
            }
            Initialize();
            ServiceProvider.EventAggregator.Publish(this, ApplicationStartingMessage.Instance);

            var context = new DataContext(MvvmApplication.Context);
            var viewModelPresenter = IocContainer.Get<IViewModelPresenter>();
            var presenter = viewModelPresenter as IRestorableViewModelPresenter;
            if (presenter == null || !presenter.TryRestore(context))
                MvvmApplication.Start();
        }

        protected virtual IDynamicViewModelPresenter GetRootPresenter()
        {
            if (RootPresenterFactory != null)
                return RootPresenterFactory(IocContainer);
            var presenter = IocContainer.Get<XamarinFormsRootDynamicViewModelPresenter>();
            presenter.WrapToNavigationPage = WrapToNavigationPage;
            presenter.NavigationPageFactory = NavigationPageFactory;
            return presenter;
        }

        private static bool CanShowViewModelTabPresenter(IViewModel viewModel, IDataContext dataContext,
            IViewModelPresenter arg3)
        {
            var viewName = viewModel.GetViewName(dataContext);
            var container = viewModel.GetIocContainer(true);
            var mappingProvider = container.Get<IViewMappingProvider>();
            var mappingItem = mappingProvider.FindMappingForViewModel(viewModel.GetType(), viewName, false);
            return mappingItem == null ||
                   typeof(ITabView).GetTypeInfo().IsAssignableFrom(mappingItem.ViewType.GetTypeInfo()) ||
                   !typeof(Page).GetTypeInfo().IsAssignableFrom(mappingItem.ViewType.GetTypeInfo());
        }

        private static bool CanShowViewModelNavigationPresenter(IViewModel viewModel, IDataContext dataContext,
            IViewModelPresenter arg3)
        {
            var viewName = viewModel.GetViewName(dataContext);
            var container = viewModel.GetIocContainer(true);
            var mappingProvider = container.Get<IViewMappingProvider>();
            var mappingItem = mappingProvider.FindMappingForViewModel(viewModel.GetType(), viewName, false);
            return mappingItem != null &&
                   typeof(Page).GetTypeInfo().IsAssignableFrom(mappingItem.ViewType.GetTypeInfo());
        }

        #endregion
    }
}