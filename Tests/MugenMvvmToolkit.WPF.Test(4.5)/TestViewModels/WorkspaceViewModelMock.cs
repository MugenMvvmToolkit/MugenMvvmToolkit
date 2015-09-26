using System;
using System.Threading.Tasks;
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

        protected override bool CanClose(object param)
        {
            return CanCloseValue;
        }

        protected override Task<bool> OnClosing(object parameter)
        {
            if (OnClosingCallback == null)
                return Empty.TrueTask;
            return OnClosingCallback(parameter);
        }

        protected override void OnClosed(object parameter)
        {
            OnClosedInvoke = true;
            base.OnClosed(parameter);
        }

        #endregion
    }
}
