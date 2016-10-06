using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Test.ViewModels
{
    [TestClass]
    public class MultiWorkspaceViewModelTest : MultiViewModelTest<MultiViewModel<IViewModel>>
    {
        #region Overrides of MultiViewModelTest

        protected override MultiViewModel<IViewModel> GetMultiViewModelInternal()
        {
            var vm = new MultiViewModel<IViewModel>();
            return vm;
        }

        #endregion
    }

    [TestClass]
    public class MultiWorkspaceViewModelCloseableTest : CloseableViewModelTest
    {
        #region Overrides of CloseableViewModelTest

        protected override ICloseableViewModel GetCloseableViewModelInternal()
        {
            return new MultiViewModel<IViewModel>();
        }

        #endregion
    }
}
