using System;
using System.Threading.Tasks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Test.TestViewModels
{
    public class WorkspaceViewModelMock : WorkspaceViewModel
    {
        #region Properties

        public bool CanCloseValue { get; set; }

        public Func<IDataContext, object, Task<bool>> OnClosingCallback { get; set; }

        public bool OnClosedInvoke { get; set; }

        #endregion

        #region Overrides of WorkspaceViewModel

        protected override bool CanClose(object param)
        {
            return CanCloseValue;
        }

        protected override Task<bool> OnClosing(IDataContext context, object parameter)
        {
            return OnClosingCallback == null ? Empty.TrueTask : OnClosingCallback(context, parameter);
        }

        protected override void OnClosed(IDataContext context, object parameter)
        {
            OnClosedInvoke = true;
            base.OnClosed(context, parameter);
        }

        #endregion
    }
}
