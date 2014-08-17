#region Copyright
// ****************************************************************************
// <copyright file="FrameStateManager.cs">
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;
using JetBrains.Annotations;
using Microsoft.Phone.Controls;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Utils;
using NavigationMode = System.Windows.Navigation.NavigationMode;

namespace MugenMvvmToolkit.Infrastructure
{
    /// <summary>
    ///     Represents the frame state manager that allows to save state of page.
    /// </summary>
    public static class FrameStateManager
    {
        #region Nested types

        private sealed class StateObserver
        {
            #region Fields

            private readonly WeakReference _reference;
            private WeakReference _previousView;

            #endregion

            #region Constructors

            public StateObserver(Frame frame)
            {
                _reference = new WeakReference(frame);
                _previousView = MvvmUtils.EmptyWeakReference;
                frame.Navigating += FrameOnNavigating;
                frame.Navigated += FrameOnNavigated;
            }

            #endregion

            #region Methods

            private void FrameOnNavigating(object sender, NavigatingCancelEventArgs args)
            {
                var target = (Frame)_reference.Target;
                if (target == null)
                    return;
                var page = target.Content as PhoneApplicationPage;
                if (page != null)
                    _previousView = ServiceProvider.WeakReferenceFactory(page, true);
            }

            private void FrameOnNavigated(object sender, NavigationEventArgs args)
            {
                if (args.NavigationMode != NavigationMode.Back)
                {
                    var phoneApplicationPage = (PhoneApplicationPage)_previousView.Target;
                    if (phoneApplicationPage != null)
                        ApplicationStateManager.OnSaveState(phoneApplicationPage, phoneApplicationPage.State, args);
                }
                var page = args.Content as PhoneApplicationPage;
                if (page != null)
                {
                    //NOTE: to make sure that the callback is called after the navigation.
                    TaskCompletionSource<object> source = null;
                    var viewModel = page.DataContext as IViewModel;
                    if (viewModel != null)
                    {
                        source = new TaskCompletionSource<object>();
                        viewModel.Settings.Metadata.AddOrUpdate(RestoreStateConstant, source.Task);
                    }
                    page.Dispatcher.BeginInvoke(() =>
                       {
                           ApplicationStateManager.OnLoadState(page, page.State, args);
                           if (source != null)
                               source.SetResult(null);
                       });
                }
            }

            #endregion
        }

        #endregion

        #region Fields

        private const string StateObserverMember = "~@#stobs";
        private static IApplicationStateManager _applicationStateManager;

        internal readonly static DataConstant<Task> RestoreStateConstant = DataConstant.Create(() => RestoreStateConstant, true);

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the <see cref="IApplicationStateManager" />.
        /// </summary>
        [NotNull]
        public static IApplicationStateManager ApplicationStateManager
        {
            get
            {
                if (_applicationStateManager == null)
                    Interlocked.CompareExchange(ref _applicationStateManager,
                        new ApplicationStateManager(ServiceProvider.IocContainer.Get<ISerializer>()), null);
                return _applicationStateManager;
            }
            set { _applicationStateManager = value; }
        }

        #endregion

        #region Methods

        public static void RegisterFrame([NotNull] Frame frame)
        {
            Should.NotBeNull(frame, "frame");
            ServiceProvider.AttachedValueProvider.GetOrAdd(frame, StateObserverMember, CreateObserver, null);
        }

        private static StateObserver CreateObserver(Frame frame, object state)
        {
            return new StateObserver(frame);
        }

        #endregion
    }
}