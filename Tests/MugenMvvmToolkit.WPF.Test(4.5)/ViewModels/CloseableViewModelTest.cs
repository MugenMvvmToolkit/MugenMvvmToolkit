using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Test.TestModels;
using MugenMvvmToolkit.ViewModels;
using Should;

namespace MugenMvvmToolkit.Test.ViewModels
{
    [TestClass]
    public class CloseableViewModelTest : TestBase
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
            bool isClosingInvoked = false;
            var closeableViewModel = GetCloseableViewModel();
            closeableViewModel.Closing += (model, args) =>
                                                 {
                                                     args.Parameter.ShouldEqual(param);
                                                     isClosingInvoked = true;
                                                 };
            closeableViewModel.Closed += (model, args) =>
                                                {
                                                    args.Parameter.ShouldEqual(param);
                                                    isInvoked = true;
                                                };
            closeableViewModel.CloseCommand.Execute(param);
            isInvoked.ShouldBeTrue();
            isClosingInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void CloseCommandShouldCloseVmImmediateWithParameter()
        {
            bool isInvoked = false;
            bool isClosingInvoked = false;
            var closeableViewModel = GetCloseableViewModel();
            closeableViewModel.Closing += (model, args) =>
            {
                isClosingInvoked = true;
            };
            closeableViewModel.Closed += (model, args) =>
            {
                isInvoked = true;
            };
            closeableViewModel.CloseCommand.Execute(CloseableViewModel.ImmediateCloseParameter);
            isClosingInvoked.ShouldBeFalse();
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void CloseShouldThrowExceptionIfClosedEventThrow()
        {
            var closeableViewModel = GetCloseableViewModel();
            closeableViewModel.Closed += (model, parameter) =>
            {
                throw new TestException();
            };
            ShouldThrow<TestException>(() => closeableViewModel.CloseAsync(null).Wait());
        }

        [TestMethod]
        public void CloseShouldRedirectExceptionToTask()
        {
            var exc = new Exception();

            var closeableViewModel = GetCloseableViewModel();
            closeableViewModel.Closed += (model, parameter) =>
            {
                throw exc;
            };
            var close = closeableViewModel.CloseAsync(null);
            close.Status.ShouldEqual(TaskStatus.Faulted);
            Exception exception = close.Exception;
            while (exception is AggregateException)
            {
                exception = exception.InnerException;
            }
            exception.ShouldEqual(exc);
        }


        [TestMethod]
        public void CloseShouldReturnTrueIfWasClosedSuccessfully()
        {
            var closeableViewModel = GetCloseableViewModel();
            closeableViewModel.CloseAsync(null).Result.ShouldBeTrue();
        }

        [TestMethod]
        public void CloseShouldReturnFalseIfWasClosedNotSuccessfully()
        {
            var closeableViewModel = GetCloseableViewModel();
            closeableViewModel.Closing += (model, args) => args.Cancel = true;
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

        #endregion
    }
}