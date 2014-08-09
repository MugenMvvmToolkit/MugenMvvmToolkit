using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Infrastructure.Mediators;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Interfaces.Views;

namespace MugenMvvmToolkit.Test.Infrastructure.Mediators
{
    [TestClass]
    public class WindowViewMediatorTest : WindowViewMediatorBaseTest<IWindowView>
    {
        #region Overrides of WindowViewMediatorBaseTest<IWindowView>

        protected override WindowViewMediatorBase<IWindowView> Create(IViewModel viewModel, IThreadManager threadManager,
            IViewManager viewManager, IOperationCallbackManager callbackManager)
        {
            return new WindowViewMediator(viewModel, threadManager, viewManager, callbackManager);
        }

        #endregion
    }
}