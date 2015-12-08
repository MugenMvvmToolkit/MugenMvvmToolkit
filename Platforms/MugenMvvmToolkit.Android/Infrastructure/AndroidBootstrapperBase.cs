#region Copyright

// ****************************************************************************
// <copyright file="AndroidBootstrapperBase.cs">
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
using System.Threading;
using Android.App;
using Android.Views;
using JetBrains.Annotations;
using MugenMvvmToolkit.Android.Attributes;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Infrastructure.Presenters;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Android.Infrastructure
{
    public abstract class AndroidBootstrapperBase : BootstrapperBase, IComparer<string>
    {
        #region Nested Types

        protected sealed class DefaultApp : MvvmApplication
        {
            #region Fields

            private readonly Type _startViewModelType;
            private readonly IViewModelSettings _viewModelSettings;

            #endregion

            #region Constructors

            public DefaultApp(Type startViewModelType, IViewModelSettings viewModelSettings = null, LoadMode mode = LoadMode.Runtime)
                : base(mode)
            {
                Should.NotBeNull(startViewModelType, "startViewModelType");
                Should.BeOfType<IViewModel>(startViewModelType, "startViewModelType");
                _startViewModelType = startViewModelType;
                _viewModelSettings = viewModelSettings;
            }

            #endregion

            #region Methods

            protected override IViewModelSettings CreateViewModelSettings()
            {
                return _viewModelSettings ?? base.CreateViewModelSettings();
            }

            public override Type GetStartViewModelType()
            {
                return _startViewModelType;
            }

            #endregion
        }

        #endregion

        #region Fields

        private const int EmptyState = 0;
        private const int InitializedStateGlobal = 1;
        private const int InitializedStateLocal = 2;
        private static int _appStateGlobal;
        private readonly PlatformInfo _platform;

        #endregion

        #region Constructors

        static AndroidBootstrapperBase()
        {
            LinkerInclude.Initialize();
            ViewManager.AlwaysCreateNewView = true;
            ReflectionExtensions.GetTypesDefault = assembly => assembly.GetTypes();
            ServiceProvider.WeakReferenceFactory = PlatformExtensions.CreateWeakReference;
            DynamicMultiViewModelPresenter.CanShowViewModelDefault = CanShowViewModelTabPresenter;
            DynamicViewModelNavigationPresenter.CanShowViewModelDefault = CanShowViewModelNavigationPresenter;
            BindingServiceProvider.ValueConverter = BindingReflectionExtensions.Convert;
        }

        protected AndroidBootstrapperBase(PlatformInfo platform = null)
        {
            _platform = platform ?? PlatformExtensions.GetPlatformInfo();
        }

        #endregion

        #region Properties

        public static IList<Assembly> ViewAssemblies { get; protected set; }

        #endregion

        #region Methods

        public static void EnsureInitialized()
        {
            if (Interlocked.CompareExchange(ref _appStateGlobal, InitializedStateGlobal, EmptyState) != EmptyState)
                return;
            var attributes = new List<BootstrapperAttribute>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().SkipFrameworkAssemblies())
            {
                attributes.AddRange(assembly
                    .GetCustomAttributes(typeof(BootstrapperAttribute), false)
                    .OfType<BootstrapperAttribute>());
            }
            var bootstrapperAttribute = attributes
                .OrderByDescending(attribute => attribute.Priority)
                .FirstOrDefault();
            if (bootstrapperAttribute == null)
                throw new InvalidOperationException(@"The BootstrapperAttribute was not found.
You must specify the type of application bootstrapper using BootstrapperAttribute, for example [assembly:Bootstrapper(typeof(MyBootstrapperType))]");
            var instance = (AndroidBootstrapperBase)Activator.CreateInstance(bootstrapperAttribute.BootstrapperType);
            instance.Initialize();
        }

        protected override void InitializeInternal()
        {
            if (Interlocked.Exchange(ref _appStateGlobal, InitializedStateLocal) == InitializedStateLocal)
                return;
            TypeCache<View>.Initialize(null);
            var application = CreateApplication();
            application.Initialize(_platform, CreateIocContainer(), GetAssemblies().ToArrayEx(), InitializationContext ?? DataContext.Empty);
        }

        public virtual void Start(IDataContext context = null)
        {
            Initialize();
            MvvmApplication.Current.Start(context);
        }

        [NotNull]
        protected virtual ICollection<Assembly> GetAssemblies()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            if (ViewAssemblies == null)
            {
                //NOTE order the assemblies to keep the support libraries at the end of array.
                ViewAssemblies = assemblies
                    .Where(assembly => !assembly.IsNetFrameworkAssembly())
                    .OrderBy(assembly => assembly.FullName, this)
                    .ToArray();
            }
            return assemblies;
        }

        private static bool CanShowViewModelTabPresenter(IViewModel viewModel, IDataContext dataContext,
            IViewModelPresenter arg3)
        {
            var viewName = viewModel.GetViewName(dataContext);
            var container = viewModel.GetIocContainer(true);
            var mappingProvider = container.Get<IViewMappingProvider>();
            var mappingItem = mappingProvider.FindMappingForViewModel(viewModel.GetType(), viewName, false);
            return mappingItem == null || !typeof(Activity).IsAssignableFrom(mappingItem.ViewType);
        }

        private static bool CanShowViewModelNavigationPresenter(IViewModel viewModel, IDataContext dataContext,
            IViewModelPresenter arg3)
        {
            var viewName = viewModel.GetViewName(dataContext);
            var container = viewModel.GetIocContainer(true);
            var mappingProvider = container.Get<IViewMappingProvider>();
            var mappingItem = mappingProvider.FindMappingForViewModel(viewModel.GetType(), viewName, false);
            return mappingItem != null && typeof(Activity).IsAssignableFrom(mappingItem.ViewType);
        }

        #endregion

        #region Implementation of interfaces

        int IComparer<string>.Compare(string x, string y)
        {
            if (string.Equals(x, y, StringComparison.Ordinal))
                return 0;
            var xSupport = x.IndexOf(".Android.Support.", StringComparison.Ordinal) >= 0;
            var ySupport = y.IndexOf(".Android.Support.", StringComparison.Ordinal) >= 0;
            if (xSupport == ySupport)
                return string.Compare(x, y, StringComparison.Ordinal);
            if (xSupport)
                return 1;
            return -1;
        }

        #endregion
    }
}
