using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.ViewModels;
using Should;

namespace MugenMvvmToolkit.Test.ViewModels
{
    [TestClass]
    public class CloseableViewModelTest : ViewModelBaseTest
    {
        #region Nested types

        private sealed class CloseableViewModelImpl : CloseableViewModel
        {

        }

        #endregion

        #region Test methods

        [TestMethod]
        public void CloseCommandShouldCloseVm()
        {
            var param = new object();
            bool isInvoked = false;
            var closeableViewModel = GetCloseableViewModel();
            ViewModelPresenter.CloseAsync += (model, context) =>
            {
                isInvoked = true;
                model.ShouldEqual(closeableViewModel);
                return Empty.TrueTask;
            };
            closeableViewModel.CloseCommand.Execute(param);
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void CloseShouldReturnTrueIfWasClosedSuccessfully()
        {
            ViewModelPresenter.CloseAsync += (model, context) => Empty.TrueTask;
            var closeableViewModel = GetCloseableViewModel();
            closeableViewModel.CloseAsync(null).Result.ShouldBeTrue();
        }

        [TestMethod]
        public void CloseShouldReturnFalseIfWasClosedNotSuccessfully()
        {
            ViewModelPresenter.CloseAsync += (model, context) => Empty.FalseTask;
            var closeableViewModel = GetCloseableViewModel();
            closeableViewModel.CloseAsync(null).Result.ShouldBeFalse();
        }

        #endregion

        #region Methods

        private ICloseableViewModel GetCloseableViewModel()
        {
            var viewModel = GetCloseableViewModelInternal();
            InitializeViewModel(viewModel, IocContainer);
            return viewModel;
        }

        protected virtual ICloseableViewModel GetCloseableViewModelInternal()
        {
            return new CloseableViewModelImpl();
        }

        protected override void OnInit()
        {
            base.OnInit();
            GetViewModelBaseDelegate = () => (ViewModelBase)GetCloseableViewModelInternal();
        }

        #endregion
    }
}
