using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Collections;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Test.TestViewModels;
using MugenMvvmToolkit.ViewModels;
using Should;

namespace MugenMvvmToolkit.Test.ViewModels
{
    [TestClass]
    public class MultiWorkspaceViewModelTest : MultiViewModelTest<MultiViewModel>
    {
        #region Overrides of MultiViewModelTest

        protected override MultiViewModel GetMultiViewModelInternal()
        {
            var vm = new MultiViewModel();
            ((SynchronizedNotifiableCollection<IViewModel>)vm.ItemsSource).ExecutionMode = ExecutionMode.None;
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
