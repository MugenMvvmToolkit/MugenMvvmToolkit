#region Copyright

// ****************************************************************************
// <copyright file="WorkspaceViewModelTest.cs">
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

        #endregion

        #region Overrides of CloseableViewModelTest

        protected override ICloseableViewModel GetCloseableViewModelInternal()
        {
            return new TestWorkspaceViewModel();
        }

        #endregion
    }
}
