using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Test.TestViewModels;
using Should;

namespace MugenMvvmToolkit.Test.ViewModels
{
    [TestClass]
    public class WorkspaceViewModelTest : CloseableViewModelTest
    {
        #region Test methods

        [TestMethod]
        public void CanExecuteShouldReturnResultFromCanClose()
        {
            var viewModel = GetViewModel<WorkspaceViewModelMock>();
            viewModel.CanCloseValue = false;
            viewModel.CloseCommand.CanExecute(null).ShouldBeFalse();

            viewModel.CanCloseValue = true;
            viewModel.CloseCommand.CanExecute(null).ShouldBeTrue();
        }

        [TestMethod]
        public void OnClosedShouldBeCalledWhenVmClosed()
        {
            var viewModel = GetViewModel<WorkspaceViewModelMock>();
            viewModel.OnClosedInvoke.ShouldBeFalse();
            viewModel.CloseAsync(null);
            viewModel.OnClosedInvoke.ShouldBeTrue();
        }

        [TestMethod]
        public void OnClosingShouldBeCalledWhenVmClosing()
        {
            var viewModel = GetViewModel<WorkspaceViewModelMock>();
            viewModel.OnClosingCallback = o => Empty.FalseTask;
            viewModel.CloseAsync(null).Result.ShouldBeFalse();

            viewModel.OnClosingCallback = o => Empty.TrueTask;
            viewModel.CloseAsync(null).Result.ShouldBeTrue();
        }

        #endregion

        #region Overrides of CloseableViewModelTest

        protected override ICloseableViewModel GetCloseableViewModelInternal()
        {
            return new TestWorkspaceViewModel();
        }

        #endregion
    }
}
