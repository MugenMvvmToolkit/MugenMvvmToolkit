using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Core;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Test.TestInfrastructure;
using MugenMvvmToolkit.Test.TestModels;
using Should;

namespace MugenMvvmToolkit.Test.Bindings.Core
{
    [TestClass]
    public class BindingContextManagerTest : BindingTestBase
    {
        #region Methods

        [TestMethod]
        public void ManagerShouldUseOneContextForOneObject()
        {
            var context = new ExplicitDataContext();
            var manager = CreateContextManager();
            var bindingContext = manager.GetBindingContext(context);
            manager.GetBindingContext(context).ShouldEqual(bindingContext);
        }

        [TestMethod]
        public void ManagerShouldUseExplicitDataContextGet()
        {
            var o = new object();
            var context = new ExplicitDataContext();
            var manager = CreateContextManager();
            var bindingContext = manager.GetBindingContext(context);
            bindingContext.DataContext.ShouldBeNull();
            context.DataContext = o;
            bindingContext.DataContext.ShouldEqual(o);
        }

        [TestMethod]
        public void ManagerShouldUseExplicitDataContextSet()
        {
            var o = new object();
            var context = new ExplicitDataContext();
            var manager = CreateContextManager();
            var bindingContext = manager.GetBindingContext(context);
            bindingContext.DataContext.ShouldBeNull();
            bindingContext.DataContext = o;
            bindingContext.DataContext.ShouldEqual(o);
            context.DataContext.ShouldEqual(o);
        }

        [TestMethod]
        public void ManagerShouldRaiseEventWhenDataContextChanged()
        {
            bool isInvoked = false;
            var context = new ExplicitDataContext();
            var manager = CreateContextManager();
            var bindingContext = manager.GetBindingContext(context);
            bindingContext.DataContextChanged += (sender, args) => isInvoked = true;
            isInvoked.ShouldBeFalse();
            bindingContext.DataContext = context;
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void ManagerShouldUseWeakReferenceForTarget()
        {
            var o = new object();
            var context = new ExplicitDataContext { DataContext = o };
            var manager = CreateContextManager();
            var bindingContext = manager.GetBindingContext(context);
            bindingContext.DataContext.ShouldEqual(o);

            context = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            bindingContext.DataContext.ShouldBeNull();
        }

        [TestMethod]
        public void ManagerShouldObserveExplicitDataContext()
        {
            bool isInvoked = false;
            var o = new object();
            var context = new ExplicitDataContext { DataContext = o };
            var providerMock = new ObserverProviderMock();
            var manager = CreateContextManager(observerProvider: providerMock);
            providerMock.Observe = (o1, path, arg3) =>
            {
                o1.ShouldEqual(context);
                path.Path.ShouldEqual(AttachedMemberConstants.DataContext);
                arg3.ShouldBeTrue();
                isInvoked = true;
                return new ObserverMock();
            };
            manager.GetBindingContext(context);
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void ManagerShouldRaiseEventWhenObserverValueChanged()
        {
            bool isInvoked = false;
            var o = new object();
            var context = new ExplicitDataContext { DataContext = o };
            var providerMock = new ObserverProviderMock();
            var observerMock = new ObserverMock();
            var manager = CreateContextManager(observerProvider: providerMock);
            providerMock.Observe = (o1, path, arg3) => observerMock;
            var bindingContext = manager.GetBindingContext(context);
            bindingContext.DataContextChanged += (sender, args) => isInvoked = true;
            isInvoked.ShouldBeFalse();
            observerMock.RaiseValueChanged();
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void ManagerShouldUseParentBindingContextIfItHasNotExplicit()
        {
            bool isFindParentInvoked = false;
            bool isObserveParentInvoked = false;
            var context = new object();
            var managerMock = new VisualTreeManagerMock
            {
                FindParent = o =>
                {
                    o.ShouldEqual(context);
                    isFindParentInvoked = true;
                    return null;
                }
            };
            var providerMock = new ObserverProviderMock
            {
                ObserveParent = (o, listener) =>
                {
                    o.ShouldEqual(context);
                    isObserveParentInvoked = true;
                    return null;
                }
            };

            var manager = CreateContextManager(managerMock, providerMock);
            var bindingContext = manager.GetBindingContext(context);
            isFindParentInvoked.ShouldBeTrue();
            isObserveParentInvoked.ShouldBeTrue();
            bindingContext.DataContext.ShouldBeNull();
        }

        [TestMethod]
        public void ManagerShouldUpdateContextWhenParentChanged()
        {
            bool isFindParentInvoked = false;
            var context = new object();
            var managerMock = new VisualTreeManagerMock
            {
                FindParent = o =>
                {
                    o.ShouldEqual(context);
                    isFindParentInvoked = true;
                    return null;
                }
            };
            IEventListener eventListener = null;
            var providerMock = new ObserverProviderMock
            {
                ObserveParent = (o, listener) =>
                {
                    eventListener = listener;
                    return null;
                },
            };

            var manager = CreateContextManager(managerMock, providerMock);
            var bindingContext = manager.GetBindingContext(context);
            isFindParentInvoked.ShouldBeTrue();
            bindingContext.DataContext.ShouldBeNull();

            isFindParentInvoked = false;
            eventListener.ShouldNotBeNull();
            eventListener.Handle(this, EventArgs.Empty);
            isFindParentInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void ManagerShouldNotUpdateContextWhenParentChangedIfHasValue()
        {
            bool isFindParentInvoked = false;
            var context = new object();
            var managerMock = new VisualTreeManagerMock
            {
                FindParent = o =>
                {
                    o.ShouldEqual(context);
                    isFindParentInvoked = true;
                    return null;
                }
            };
            IEventListener eventListener = null;
            var providerMock = new ObserverProviderMock
            {
                ObserveParent = (o, listener) =>
                {
                    eventListener = listener;
                    return null;
                },
            };

            var manager = CreateContextManager(managerMock, providerMock);
            var bindingContext = manager.GetBindingContext(context);
            isFindParentInvoked.ShouldBeTrue();
            bindingContext.DataContext.ShouldBeNull();

            isFindParentInvoked = false;
            bindingContext.DataContext = context;
            eventListener.ShouldNotBeNull();
            eventListener.Handle(this, EventArgs.Empty);
            isFindParentInvoked.ShouldBeFalse();
        }

        [TestMethod]
        public void ManagerShouldRaiseEventWhenDataContextChangedNotExplicit()
        {
            bool contextChanged = false;
            var context = new object();
            var managerMock = new VisualTreeManagerMock
            {
                FindParent = o => null
            };
            var providerMock = new ObserverProviderMock
            {
                ObserveParent = (o, listener) => null
            };

            var manager = CreateContextManager(managerMock, providerMock);
            var bindingContext = manager.GetBindingContext(context);
            bindingContext.DataContextChanged += (sender, args) => contextChanged = true;
            bindingContext.DataContext = context;
            contextChanged.ShouldBeTrue();
        }

        [TestMethod]
        public void ManagerShouldRaiseEventWhenParentChanged()
        {
            bool contextChanged = false;
            var context = new object();
            var managerMock = new VisualTreeManagerMock
            {
                FindParent = o => null
            };
            IEventListener eventListener = null;
            var providerMock = new ObserverProviderMock
            {
                ObserveParent = (o, listener) =>
                {
                    eventListener = listener;
                    return null;
                },
            };

            var manager = CreateContextManager(managerMock, providerMock);
            var bindingContext = manager.GetBindingContext(context);
            bindingContext.DataContextChanged += (sender, args) => contextChanged = true;
            eventListener.ShouldNotBeNull();
            eventListener.Handle(this, EventArgs.Empty);
            contextChanged.ShouldBeTrue();
        }

        protected virtual IBindingContextManager CreateContextManager(IVisualTreeManager treeManager = null,
            IObserverProvider observerProvider = null)
        {
            BindingProvider.Instance = new BindingProvider();
            if (treeManager != null)
                BindingProvider.Instance.VisualTreeManager = treeManager;
            if (observerProvider != null)
                BindingProvider.Instance.ObserverProvider = observerProvider;
            return new BindingContextManager();
        }

        #endregion
    }
}