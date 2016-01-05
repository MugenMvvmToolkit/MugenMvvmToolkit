using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Test.ViewModels
{
    [TestClass]
    public class MultiWorkspaceViewModelTest : MultiViewModelTest<MultiViewModel>
    {
        #region Overrides of MultiViewModelTest

        protected override MultiViewModel GetMultiViewModelInternal()
        {
            var vm = new MultiViewModel();
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
            return new MultiViewModel();
        }

        #endregion
    }
}
