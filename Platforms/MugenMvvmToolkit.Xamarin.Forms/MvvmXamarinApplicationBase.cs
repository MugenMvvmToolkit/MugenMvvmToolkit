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
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Models.Messages;
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

        protected MvvmXamarinApplicationBase([NotNull]XamarinFormsBootstrapperBase.IPlatformService platformService)
        {
            Should.NotBeNull(platformService, nameof(platformService));
            // ReSharper disable once VirtualMemberCallInConstructor
            var bootstrapper = XamarinFormsBootstrapperBase.Current ?? CreateBootstrapper(platformService);
            bootstrapper.Start();
        }

        #endregion

        #region Methods

        protected override void OnResume()
        {
            base.OnResume();
            ServiceProvider.EventAggregator.Publish(this, new ForegroundNavigationMessage());
        }

        protected override void OnSleep()
        {
            base.OnSleep();
            ServiceProvider.EventAggregator.Publish(this, new BackgroundNavigationMessage());
            if (_presenter == null && !_presenterActivated)
            {
                _presenter = ServiceProvider.Get<IViewModelPresenter>() as IRestorableViewModelPresenter;
                _presenterActivated = true;
            }
            _presenter?.SaveState();
        }

        [NotNull]
        protected abstract XamarinFormsBootstrapperBase CreateBootstrapper([NotNull]XamarinFormsBootstrapperBase.IPlatformService platformService);

        #endregion
    }
}