#region Copyright

// ****************************************************************************
// <copyright file="VisualStateManager.cs">
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

using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
#if WPF
using System.Windows;

namespace MugenMvvmToolkit.WPF.Infrastructure
#elif SILVERLIGHT
using System.Windows.Controls;

namespace MugenMvvmToolkit.Silverlight.Infrastructure
#elif WINDOWSCOMMON || NETFX_CORE
using Windows.UI.Xaml.Controls;

namespace MugenMvvmToolkit.WinRT.Infrastructure
#elif WINDOWS_PHONE
using System.Windows.Controls;

namespace MugenMvvmToolkit.WinPhone.Infrastructure
#endif

{
    /// <summary>
    ///     Manages states and the logic for transitioning between states for controls.
    /// </summary>
    public sealed class VisualStateManager : IVisualStateManager
    {
        #region Fields

        private readonly IThreadManager _threadManager;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="VisualStateManager" /> class.
        /// </summary>
        public VisualStateManager([NotNull] IThreadManager threadManager)
        {
            Should.NotBeNull(threadManager, "threadManager");
            _threadManager = threadManager;
        }

        #endregion

        #region Implementation of IVisualStateManager

        /// <summary>
        ///     Transitions the control between two states.
        /// </summary>
        /// <returns>
        ///     true if the control successfully transitioned to the new state; otherwise, false.
        /// </returns>
        /// <param name="view">The view to transition between states. </param>
        /// <param name="stateName">The state to transition to.</param>
        /// <param name="useTransitions">true to use a VisualTransition to transition between states; otherwise, false.</param>
        /// <param name="context">The specified context.</param>
        public Task<bool> GoToStateAsync(object view, string stateName, bool useTransitions, IDataContext context)
        {
            Should.NotBeNull(view, "view");
#if WPF
            var control = ToolkitExtensions.GetUnderlyingView<object>(view) as FrameworkElement;
#else
            var control = ToolkitExtensions.GetUnderlyingView<object>(view) as Control;
#endif
            if (control == null)
                return Empty.FalseTask;
            var tcs = new TaskCompletionSource<bool>();
            _threadManager.InvokeOnUiThreadAsync(() =>
            {
#if WPF
                var result = System.Windows.VisualStateManager.GoToState(control, stateName, useTransitions);
#elif !NETFX_CORE && !WINDOWSCOMMON
                var result = System.Windows.VisualStateManager.GoToState(control, stateName, useTransitions);
#else
                var result = Windows.UI.Xaml.VisualStateManager.GoToState(control, stateName, useTransitions);
#endif
                tcs.SetResult(result);
            });
            return tcs.Task;
        }

        #endregion
    }
}
