#region Copyright

// ****************************************************************************
// <copyright file="MultiWorkspaceViewModelTest.cs">
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
