#region Copyright

// ****************************************************************************
// <copyright file="BindingContextManagerTest.cs">
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

using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Infrastructure;
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
            bindingContext.Value.ShouldBeNull();
            context.DataContext = o;
            bindingContext.Value.ShouldEqual(o);
        }

        [TestMethod]
        public void ManagerShouldUseExplicitDataContextSet()
        {
            var o = new object();
            var context = new ExplicitDataContext();
            var manager = CreateContextManager();
            var bindingContext = manager.GetBindingContext(context);
            bindingContext.Value.ShouldBeNull();
            bindingContext.Value = o;
            bindingContext.Value.ShouldEqual(o);
            context.DataContext.ShouldEqual(o);
        }

        [TestMethod]
        public void ManagerShouldRaiseEventWhenDataContextChanged()
        {
            bool isInvoked = false;
            var context = new ExplicitDataContext();
            var manager = CreateContextManager();
            var bindingContext = manager.GetBindingContext(context);
            bindingContext.ValueChanged += (sender, args) => isInvoked = true;
            isInvoked.ShouldBeFalse();
            bindingContext.Value = context;
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void ManagerShouldUseWeakReferenceForTarget()
        {
            var o = new object();
            var context = new ExplicitDataContext { DataContext = o };
            var manager = CreateContextManager();
            var bindingContext = manager.GetBindingContext(context);
            bindingContext.Value.ShouldEqual(o);

            context = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            bindingContext.Value.ShouldBeNull();
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
            bindingContext.ValueChanged += (sender, args) => isInvoked = true;
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

            var memberMock = new BindingMemberInfoMock
            {
                TryObserveMember = (o, listener) =>
                {
                    o.ShouldEqual(context);
                    isObserveParentInvoked = true;
                    return null;
                }
            };
            var managerMock = new VisualTreeManagerMock
            {
                FindParent = o =>
                {
                    o.ShouldEqual(context);
                    isFindParentInvoked = true;
                    return null;
                },
                GetParentMember = type =>
                {
                    type.ShouldEqual(context.GetType());
                    return memberMock;
                }
            };

            var manager = CreateContextManager(managerMock);
            var bindingContext = manager.GetBindingContext(context);
            isFindParentInvoked.ShouldBeTrue();
            isObserveParentInvoked.ShouldBeTrue();
            bindingContext.Value.IsUnsetValue().ShouldBeTrue();
        }

        [TestMethod]
        public void ManagerShouldUpdateContextWhenParentChanged()
        {
            bool isFindParentInvoked = false;
            var context = new object();
            IEventListener eventListener = null;
            var memberMock = new BindingMemberInfoMock
            {
                TryObserveMember = (o, listener) =>
                {
                    eventListener = listener;
                    return null;
                }
            };
            var managerMock = new VisualTreeManagerMock
            {
                FindParent = o =>
                {
                    o.ShouldEqual(context);
                    isFindParentInvoked = true;
                    return null;
                },
                GetParentMember = type =>
                {
                    type.ShouldEqual(context.GetType());
                    return memberMock;
                }
            };

            var manager = CreateContextManager(managerMock);
            var bindingContext = manager.GetBindingContext(context);
            isFindParentInvoked.ShouldBeTrue();
            bindingContext.Value.IsUnsetValue().ShouldBeTrue();

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
            IEventListener eventListener = null;
            var memberMock = new BindingMemberInfoMock
            {
                TryObserveMember = (o, listener) =>
                {
                    eventListener = listener;
                    return null;
                }
            };
            var managerMock = new VisualTreeManagerMock
            {
                FindParent = o =>
                {
                    o.ShouldEqual(context);
                    isFindParentInvoked = true;
                    return null;
                },
                GetParentMember = type =>
                {
                    type.ShouldEqual(context.GetType());
                    return memberMock;
                }
            };

            var manager = CreateContextManager(managerMock);
            var bindingContext = manager.GetBindingContext(context);
            isFindParentInvoked.ShouldBeTrue();
            bindingContext.Value.IsUnsetValue().ShouldBeTrue();

            isFindParentInvoked = false;
            bindingContext.Value = context;
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
                FindParent = o => null,
                GetParentMember = type => null
            };

            var manager = CreateContextManager(managerMock);
            var bindingContext = manager.GetBindingContext(context);
            bindingContext.ValueChanged += (sender, args) => contextChanged = true;
            bindingContext.Value = context;
            contextChanged.ShouldBeTrue();
        }

        [TestMethod]
        public void ManagerShouldRaiseEventWhenParentChanged()
        {
            bool contextChanged = false;
            var context = new object();
            IEventListener eventListener = null;
            var item = new object();
            var memberMock = new BindingMemberInfoMock
            {
                TryObserveMember = (o, listener) =>
                {
                    eventListener = listener;
                    return null;
                }
            };
            var managerMock = new VisualTreeManagerMock
            {
                FindParent = o =>
                {
                    if (o == item)
                        return null;
                    return item;
                },
                GetParentMember = type =>
                {
                    type.ShouldEqual(context.GetType());
                    return memberMock;
                }
            };

            var manager = CreateContextManager(managerMock);
            manager.GetBindingContext(item).Value = item;
            var bindingContext = manager.GetBindingContext(context);
            bindingContext.ValueChanged += (sender, args) => contextChanged = true;
            eventListener.ShouldNotBeNull();
            eventListener.Handle(this, EventArgs.Empty);
            contextChanged.ShouldBeTrue();
        }

        protected virtual IBindingContextManager CreateContextManager(IVisualTreeManager treeManager = null,
            IObserverProvider observerProvider = null)
        {
            BindingServiceProvider.BindingProvider = new BindingProvider();
            if (treeManager != null)
                BindingServiceProvider.VisualTreeManager = treeManager;
            if (observerProvider != null)
                BindingServiceProvider.ObserverProvider = observerProvider;
            return new BindingContextManager();
        }

        #endregion
    }
}
