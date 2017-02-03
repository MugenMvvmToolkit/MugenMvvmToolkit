using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Infrastructure.Mediators;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Silverlight.Infrastructure.Mediators;
using MugenMvvmToolkit.Silverlight.Interfaces.Views;
using MugenMvvmToolkit.WPF.Infrastructure.Mediators;
using MugenMvvmToolkit.WPF.Interfaces.Views;

namespace MugenMvvmToolkit.Test.Infrastructure.Mediators
{
    [TestClass]
    public class WindowViewMediatorTest : WindowViewMediatorBaseTest<IWindowView>
    {
        #region Overrides of WindowViewMediatorBaseTest<IWindowView>

        protected override WindowViewMediatorBase<IWindowView> Create(IViewModel viewModel, IThreadManager threadManager, IViewManager viewManager,
            IWrapperManager wrapperManager, IOperationCallbackManager callbackManager, INavigationDispatcher navigationDispatcher)
        {
            return new WindowViewMediator(viewModel, threadManager, viewManager, wrapperManager, callbackManager, navigationDispatcher);
        }

        #endregion
    }
}
