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

using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MugenMvvmToolkit.Models.Messages;
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
            Resuming += OnResuming;
        }

        #endregion

        #region Methods

        protected abstract UwpBootstrapperBase CreateBootstrapper(Frame frame);

        /// <summary>
        ///     Invoked when the application is launched normally by the end user.  Other entry points
        ///     will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            var rootFrame = Window.Current.Content as Frame;
            UwpBootstrapperBase bootstrapper = null;
            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                bootstrapper = CreateBootstrapper(rootFrame);
                await bootstrapper.InitializeAsync();

                //Associate the frame with a SuspensionManager key                                
                SuspensionManager.RegisterFrame(rootFrame, "AppFrame");
                rootFrame.NavigationFailed += OnNavigationFailed;

                if (ShouldRestoreApplicationState())
                    await RestoreStateAsync(e);

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
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
            ServiceProvider.EventAggregator.Publish(this, new BackgroundNavigationMessage());
            if (ShouldSaveApplicationState())
            {
                var deferral = e.SuspendingOperation.GetDeferral();
                await SaveStateAsync(e);
                deferral.Complete();
            }
        }

        /// <summary>
        ///     Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        protected virtual void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        protected virtual void OnResuming(object sender, object e)
        {
            ServiceProvider.EventAggregator.Publish(this, new ForegroundNavigationMessage());
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