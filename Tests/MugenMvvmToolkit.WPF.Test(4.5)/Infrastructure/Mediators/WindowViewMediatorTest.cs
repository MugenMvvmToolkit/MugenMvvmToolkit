#region Copyright

// ****************************************************************************
// <copyright file="WindowViewMediatorTest.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

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
            var mediator = new WindowViewMediator(threadManager, viewManager, wrapperManager, navigationDispatcher);
            mediator.Initialize(viewModel, null);
            return mediator;
        }

        #endregion
    }
}
