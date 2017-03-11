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
using System.Threading.Tasks;
using System.Windows.Forms;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.WinForms.Infrastructure
{
    public abstract class WinFormsBootstrapperBase : BootstrapperBase, IDynamicViewModelPresenter
    {
        #region Fields

        private readonly PlatformInfo _platform;

        #endregion

        #region Constructors

        static WinFormsBootstrapperBase()
        {
            ReflectionExtensions.GetTypesDefault = assembly => assembly.GetTypes();
            ApplicationSettings.NavigationPresenterCanShowViewModel = (model, context, arg3) => false;
        }

        protected WinFormsBootstrapperBase(bool autoRunApplication = true, PlatformInfo platform = null)
        {
            _platform = platform ?? PlatformExtensions.GetPlatformInfo();
            AutoRunApplication = autoRunApplication;
            ShutdownOnMainViewModelClose = true;
        }

        #endregion

        #region Properties

        public bool AutoRunApplication { get; set; }

        public bool ShutdownOnMainViewModelClose { get; set; }

        #endregion

        #region Overrides of BootstrapperBase

        protected override PlatformInfo Platform => _platform;

        protected override IList<Assembly> GetAssemblies()
        {
            var assemblies = new HashSet<Assembly>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic))
            {
                if (assemblies.Add(assembly))
                    assemblies.AddRange(assembly.GetReferencedAssemblies().Select(Assembly.Load));
            }
            return assemblies.ToArrayEx();
        }

        #endregion

        #region Implementation of IDynamicViewModelPresenter

        int IDynamicViewModelPresenter.Priority => int.MaxValue;

        IAsyncOperation IDynamicViewModelPresenter.TryShowAsync(IDataContext context, IViewModelPresenter parentPresenter)
        {
            parentPresenter.DynamicPresenters.Remove(this);
            var operation = parentPresenter.ShowAsync(context);
            if (ShutdownOnMainViewModelClose)
                operation.ContinueWith(result => Application.Exit());
            if (AutoRunApplication)
                Application.Run();
            return operation;
        }

        Task<bool> IDynamicViewModelPresenter.TryCloseAsync(IDataContext context, IViewModelPresenter parentPresenter)
        {
            return null;
        }

        #endregion

        #region Methods

        public virtual void Start()
        {
            Initialize();
            var app = ServiceProvider.Application;
            if (!app.Context.Contains(NavigationConstants.IsDialog))
                app.Context.Add(NavigationConstants.IsDialog, false);
            app.IocContainer.Get<IViewModelPresenter>().DynamicPresenters.Add(this);
            app.Start();
        }

        #endregion
    }
}
