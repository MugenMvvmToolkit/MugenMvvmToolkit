#region Copyright

// ****************************************************************************
// <copyright file="WinFormsBootstrapperBase.cs">
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
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.WinForms.Infrastructure.Mediators;

namespace MugenMvvmToolkit.WinForms.Infrastructure
{
    public abstract class WinFormsBootstrapperBase : BootstrapperBase
    {
        #region Constructors

        static WinFormsBootstrapperBase()
        {
            ReflectionExtensions.GetTypesDefault = assembly => assembly.GetTypes();
            ApplicationSettings.NavigationPresenterCanShowViewModel = (model, context, arg3) => false;
        }

        protected WinFormsBootstrapperBase(bool autoRunApplication = true, PlatformInfo platform = null, bool isDesignMode = false)
            : base(isDesignMode)
        {
            Platform = platform ?? WinFormsToolkitExtensions.GetPlatformInfo();
            AutoRunApplication = autoRunApplication;
            ShutdownOnMainViewModelClose = true;
        }

        #endregion

        #region Properties

        public bool AutoRunApplication { get; set; }

        public bool ShutdownOnMainViewModelClose { get; set; }

        public Func<IIocContainer, IDynamicViewModelPresenter> RootPresenterFactory { get; set; }

        protected override PlatformInfo Platform { get; }

        #endregion

        #region Methods

        public virtual void Start()
        {
            Initialize();
            if (!MvvmApplication.Context.Contains(NavigationConstants.IsDialog))
                MvvmApplication.Context.Add(NavigationConstants.IsDialog, false);
            var rootPresenter = GetRootPresenter();
            if (rootPresenter != null)
                IocContainer.Get<IViewModelPresenter>().DynamicPresenters.Add(rootPresenter);
            MvvmApplication.Start();
        }

        protected virtual IDynamicViewModelPresenter GetRootPresenter()
        {
            if (RootPresenterFactory != null)
                return RootPresenterFactory(IocContainer);
            return new WinFormsRootDynamicViewModelPresenter
            {
                ShutdownOnMainViewModelClose = ShutdownOnMainViewModelClose,
                AutoRunApplication = AutoRunApplication
            };
        }

        protected override void UpdateAssemblies(HashSet<Assembly> assemblies)
        {
            base.UpdateAssemblies(assemblies);
            assemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic));
        }

        #endregion
    }
}