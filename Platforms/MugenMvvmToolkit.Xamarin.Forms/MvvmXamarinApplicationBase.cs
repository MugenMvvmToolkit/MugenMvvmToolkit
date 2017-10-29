#region Copyright

// ****************************************************************************
// <copyright file="MvvmXamarinApplicationBase.cs">
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

using JetBrains.Annotations;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Xamarin.Forms.Infrastructure;
using MugenMvvmToolkit.Xamarin.Forms.Interfaces.Presenters;
using Xamarin.Forms;

namespace MugenMvvmToolkit.Xamarin.Forms
{
    public abstract class MvvmXamarinApplicationBase : Application
    {
        #region Fields

        private IRestorableViewModelPresenter _presenter;
        private bool _presenterActivated;

        #endregion

        #region Constructors

        protected MvvmXamarinApplicationBase(XamarinFormsBootstrapperBase.IPlatformService platformService, IDataContext context = null)
        {
            if (context == null)
                context = DataContext.Empty;
            var bootstrapper = XamarinFormsBootstrapperBase.Current;
            if (bootstrapper == null)
            {
                // ReSharper disable VirtualMemberCallInConstructor
                bootstrapper = CreateBootstrapper(platformService, context);
                if (!ShouldRestoreApplicationState(context))
                {
                    bootstrapper.InitializationContext = bootstrapper.InitializationContext.ToNonReadOnly();
                    bootstrapper.InitializationContext.AddOrUpdate(ViewModelConstants.StateNotNeeded, true);
                }
                // ReSharper restore VirtualMemberCallInConstructor
            }
            bootstrapper.Start();
        }

        #endregion

        #region Methods

        [NotNull]
        protected abstract XamarinFormsBootstrapperBase CreateBootstrapper(XamarinFormsBootstrapperBase.IPlatformService platformService, IDataContext context);

        protected override void OnStart()
        {
            base.OnStart();
            if (ToolkitServiceProvider.Application != null && ToolkitServiceProvider.Application.PlatformInfo.Platform != PlatformType.XamarinFormsUWP)
                ToolkitServiceProvider.Application.SetApplicationState(ApplicationState.Active, null);
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (ToolkitServiceProvider.Application != null && ToolkitServiceProvider.Application.PlatformInfo.Platform != PlatformType.XamarinFormsUWP)
                ToolkitServiceProvider.Application.SetApplicationState(ApplicationState.Active, null);
        }

        protected override void OnSleep()
        {
            base.OnSleep();
            if (ToolkitServiceProvider.Application != null && ToolkitServiceProvider.Application.PlatformInfo.Platform != PlatformType.XamarinFormsUWP)
                ToolkitServiceProvider.Application.SetApplicationState(ApplicationState.Background, null);
            if (ShouldSaveApplicationState())
                SaveState();
        }

        protected virtual bool ShouldSaveApplicationState()
        {
            return true;
        }

        protected virtual bool ShouldRestoreApplicationState(IDataContext context)
        {
            return true;
        }

        protected virtual void SaveState()
        {
            if (_presenter == null && !_presenterActivated)
            {
                _presenter = ToolkitServiceProvider.Get<IViewModelPresenter>() as IRestorableViewModelPresenter;
                _presenterActivated = true;
            }
            _presenter?.SaveState();
        }

        #endregion
    }
}