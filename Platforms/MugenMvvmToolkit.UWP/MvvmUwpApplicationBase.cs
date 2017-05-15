#region Copyright

// ****************************************************************************
// <copyright file="MvvmUwpApplicationBase.cs">
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

using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using MugenMvvmToolkit.UWP.Infrastructure;

namespace MugenMvvmToolkit.UWP
{
    public abstract class MvvmUwpApplicationBase : Application
    {
        #region Constructors

        /// <summary>
        ///     Initializes the singleton application object.  This is the first line of authored code
        ///     executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        protected MvvmUwpApplicationBase()
        {
            Suspending += OnSuspending;
        }

        #endregion

        #region Methods

        protected abstract UwpBootstrapperBase CreateBootstrapper();

        /// <summary>
        ///     Invoked when the application is launched normally by the end user.  Other entry points
        ///     will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            var rootContent = Window.Current.Content;
            UwpBootstrapperBase bootstrapper = null;
            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootContent == null)
            {
                bootstrapper = CreateBootstrapper();
                bootstrapper.Initialize();

                if (ShouldRestoreApplicationState())
                    await RestoreStateAsync(e);
            }

            bootstrapper?.Start();
            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        ///     Invoked when application execution is being suspended.  Application state is saved
        ///     without knowing whether the application will be terminated or resumed with the contents
        ///     of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        protected virtual async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            if (ShouldSaveApplicationState())
            {
                var deferral = e.SuspendingOperation.GetDeferral();
                await SaveStateAsync(e);
                deferral.Complete();
            }
        }

        protected virtual async Task RestoreStateAsync(LaunchActivatedEventArgs args)
        {
            if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
            {
                // Restore the saved session state only when appropriate
                try
                {
                    await SuspensionManager.RestoreAsync();
                }
                catch (SuspensionManagerException ex)
                {
                    // Something went wrong restoring state.
                    // Assume there is no state and continue
                    Tracer.Error(ex.Flatten(true));
                }
            }
        }

        protected virtual Task SaveStateAsync(SuspendingEventArgs args)
        {
            return SuspensionManager.SaveAsync();
        }

        protected virtual bool ShouldSaveApplicationState()
        {
            return true;
        }

        protected virtual bool ShouldRestoreApplicationState()
        {
            return true;
        }

        #endregion
    }
}