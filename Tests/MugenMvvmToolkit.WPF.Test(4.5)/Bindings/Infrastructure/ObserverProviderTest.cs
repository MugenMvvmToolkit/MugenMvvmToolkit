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
            observerProvider.Observe(target, BindingPath.Empty, false).ShouldBeType<EmptyPathObserver>();
        }

        [TestMethod]
        public void ProviderShouldReturnSinglePathObserverSinglePath()
        {
            var target = new BindingSourceModel();
            var observerProvider = Cretate();
            observerProvider.Observe(target, BindingPath.Create(GetMemberPath(target, model => model.NestedModel)), false).ShouldBeType<SinglePathObserver>();
        }

        [TestMethod]
        public void ProviderShouldReturnMultiPathObserverMultiPath()
        {
            var target = new BindingSourceModel();
            var observerProvider = Cretate();
            observerProvider.Observe(target, BindingPath.Create(GetMemberPath(target, model => model.NestedModel.IntProperty)), false).ShouldBeType<MultiPathObserver>();
        }

        protected virtual ObserverProvider Cretate()
        {
            return new ObserverProvider();
        }

        #endregion
    }
}
