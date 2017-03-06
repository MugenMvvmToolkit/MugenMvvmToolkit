#region Copyright

// ****************************************************************************
// <copyright file="ObserverProviderTest.cs">
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
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Test.TestModels;
using Should;

namespace MugenMvvmToolkit.Test.Bindings.Infrastructure
{
    [TestClass]
    public class ObserverProviderTest : BindingTestBase
    {
        #region Methods

        [TestMethod]
        public void ProviderShouldReturnEmptyPathObserverEmptyPath()
        {
            var target = new BindingSourceModel();
            var observerProvider = Cretate();
            observerProvider.Observe(target, BindingPath.Empty, false, EmptyContext).ShouldBeType<EmptyPathObserver>();
        }

        [TestMethod]
        public void ProviderShouldReturnSinglePathObserverSinglePath()
        {
            var target = new BindingSourceModel();
            var observerProvider = Cretate();
            observerProvider.Observe(target, new BindingPath(GetMemberPath(target, model => model.NestedModel)), false, EmptyContext).ShouldBeType<SinglePathObserver>();
        }

        [TestMethod]
        public void ProviderShouldReturnMultiPathObserverMultiPath()
        {
            var target = new BindingSourceModel();
            var observerProvider = Cretate();
            observerProvider.Observe(target, new BindingPath(GetMemberPath(target, model => model.NestedModel.IntProperty)), false, EmptyContext).ShouldBeType<MultiPathObserver>();
        }

        protected virtual ObserverProvider Cretate()
        {
            return new ObserverProvider();
        }

        #endregion
    }
}
