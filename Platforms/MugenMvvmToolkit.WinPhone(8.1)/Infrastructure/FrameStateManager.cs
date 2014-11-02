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
using System.Windows.Controls;
using System.Windows.Navigation;
using JetBrains.Annotations;
using Microsoft.Phone.Controls;
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
                _previousView = Empty.WeakReference;
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
                        PlatformExtensions.ApplicationStateManager.OnSaveState(phoneApplicationPage, phoneApplicationPage.State, args);
                }
                var page = args.Content as PhoneApplicationPage;
                if (page == null)
                    return;
                ServiceProvider.AttachedValueProvider.SetValue(args, ActionAfterRestoreStateKey, null);
                page.Dispatcher.BeginInvoke(() =>
                {
                    PlatformExtensions.ApplicationStateManager.OnLoadState(page, page.State, args);
                    var action = ServiceProvider.AttachedValueProvider.GetValue<Action<NavigationEventArgs>>(args,
                        ActionAfterRestoreStateKey, false);
                    if (action != null)
                        action(args);
                });
            }

            #endregion
        }

        #endregion

        #region Fields

        private const string StateObserverMember = "~@#stobs";
        private const string ActionAfterRestoreStateKey = "~AARS";

        private static readonly Func<Frame, object, StateObserver> CreateObserverDelegate;

        #endregion

        #region Constructors

        static FrameStateManager()
        {
            CreateObserverDelegate = CreateObserver;
        }

        #endregion

        #region Methods

        public static void RegisterFrame([NotNull] Frame frame)
        {
            Should.NotBeNull(frame, "frame");
            ServiceProvider.AttachedValueProvider.GetOrAdd(frame, StateObserverMember, CreateObserverDelegate, null);
        }

        public static void InvokeAfterRestoreState([NotNull] this NavigationEventArgs args, Action<NavigationEventArgs> action)
        {
            Should.NotBeNull(args, "args");
            var attachedValueProvider = ServiceProvider.AttachedValueProvider;
            if (attachedValueProvider.Contains(args, ActionAfterRestoreStateKey))
                attachedValueProvider.SetValue(args, ActionAfterRestoreStateKey, action);
            else
                action(args);
        }

        private static StateObserver CreateObserver(Frame frame, object state)
        {
            return new StateObserver(frame);
        }

        #endregion
    }
}