using System;
using System.Threading.Tasks;
using MugenMvvmToolkit.Utils;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Test.TestViewModels
{
    public class WorkspaceViewModelMock : WorkspaceViewModel
    {
        #region Properties

        public bool CanCloseValue { get; set; }

        public Func<object, Task<bool>> OnClosingCallback { get; set; }

        public bool OnClosedInvoke { get; set; }

        #endregion

        #region Overrides of WorkspaceViewModel

        /// <summary>
        ///     Determines whether the specified command <c>CloseCommand</c> can be execute.
        /// </summary>
        /// <param name="param">The specified command parameter.</param>
        /// <returns>
        ///     If <c>true</c> - can execute, otherwise <c>false</c>.
        /// </returns>
        protected override bool CanClose(object param)
        {
            return CanCloseValue;
        }

        /// <summary>
        ///     Occurs when view model closing.
        /// </summary>
        /// <returns>
        ///     If <c>true</c> - close, otherwise <c>false</c>.
        /// </returns>
        protected override Task<bool> OnClosing(object parameter)
        {
            if (OnClosingCallback == null)
                return MvvmUtils.TrueTaskResult;
            return OnClosingCallback(parameter);
        }

        /// <summary>
        ///     Occurs when <c>CloseCommand</c> executed.
        /// </summary>
        protected override void OnClosed(object parameter)
        {
            OnClosedInvoke = true;
            base.OnClosed(parameter);
        }

        #endregion
    }
}