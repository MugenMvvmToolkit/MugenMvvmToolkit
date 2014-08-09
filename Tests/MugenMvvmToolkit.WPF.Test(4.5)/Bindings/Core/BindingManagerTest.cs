using System.Linq;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Binding.Core;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Test.TestInfrastructure;
using MugenMvvmToolkit.Test.TestModels;
using Should;

namespace MugenMvvmToolkit.Test.Bindings.Core
{
    [TestClass]
    public class BindingManagerTest : BindingTestBase
    {
        #region Methods

        [TestMethod]
        public void ManagerShouldRegisterBinding()
        {
            const string path = "test";
            var target = new object();
            var bindingMock = new DataBindingMock();
            var manager = GetBindingManager();

            manager.Register(target, path, bindingMock);
            manager.IsRegistered(bindingMock).ShouldBeTrue();
            manager.GetBindings(target, path).Single().ShouldEqual(bindingMock);
            manager.GetBindings(target).Single().ShouldEqual(bindingMock);
        }

        [TestMethod]
        public void IsRegisteredShouldReturnFalseIfBindingIsNotRegistered()
        {
            var bindingMock = new DataBindingMock();
            var manager = GetBindingManager();
            manager.IsRegistered(bindingMock).ShouldBeFalse();
        }

        [TestMethod]
        public void IsRegisteredShouldReturnTrueIfBindingIsRegistered()
        {
            const string path = "test";
            var target = new object();
            var bindingMock = new DataBindingMock();
            var manager = GetBindingManager();

            manager.Register(target, path, bindingMock);
            manager.IsRegistered(bindingMock).ShouldBeTrue();
        }

        [TestMethod]
        public void ManagerShouldReturnEmptyBindingsForTargetIfItHasNotBindings()
        {
            var manager = GetBindingManager();
            manager.GetBindings(manager).ShouldBeEmpty();
        }

        [TestMethod]
        public void ManagerShouldReturnAllBindingsForTarget()
        {
            const string path = "test";
            var target = new object();
            var bindingMock1 = new DataBindingMock();
            var bindingMock2 = new DataBindingMock();

            var manager = GetBindingManager();
            manager.Register(target, path, bindingMock1);
            manager.GetBindings(target).Single().ShouldEqual(bindingMock1);

            manager.Register(target, path + 1, bindingMock2);
            var bindings = manager.GetBindings(target).ToList();
            bindings.Contains(bindingMock1).ShouldBeTrue();
            bindings.Contains(bindingMock2).ShouldBeTrue();
            bindings.Count.ShouldEqual(2);
        }

        [TestMethod]
        public void ManagerShouldReturnEmptyBindingsForTargetAndPathIfItHasNotBindings()
        {
            const string path = "test";
            var manager = GetBindingManager();
            manager.GetBindings(manager, path).ShouldBeEmpty();
        }

        [TestMethod]
        public void ManagerShouldReturnAllBindingsForTargetAndPath()
        {
            const string path = "test";
            var target = new object();
            var bindingMock1 = new DataBindingMock();

            var manager = GetBindingManager();
            manager.Register(target, path, bindingMock1);
            manager.GetBindings(target, path).Single().ShouldEqual(bindingMock1);
        }

        [TestMethod]
        public void ManagerShouldClearAllBindingsForTarget()
        {
            const string path1 = "test";
            const string path2 = "test2";
            var target = new object();
            var bindingFirst = new DataBindingMock();
            var bindingSecond = new DataBindingMock();
            var manager = GetBindingManager();

            manager.Register(target, path1, bindingFirst);
            manager.Register(target, path2, bindingSecond);

            manager.IsRegistered(bindingFirst).ShouldBeTrue();
            manager.IsRegistered(bindingSecond).ShouldBeTrue();

            manager.ClearBindings(target);

            manager.IsRegistered(bindingFirst).ShouldBeFalse();
            manager.IsRegistered(bindingSecond).ShouldBeFalse();
            manager.GetBindings(target).ShouldBeEmpty();
        }

        [TestMethod]
        public void ManagerShouldClearAllBindingsForTargetAndPath()
        {
            const string path1 = "test";
            const string path2 = "test2";
            var target = new object();
            var bindingFirst = new DataBindingMock();
            var bindingSecond = new DataBindingMock();
            var manager = GetBindingManager();

            manager.Register(target, path1, bindingFirst);
            manager.Register(target, path2, bindingSecond);

            manager.IsRegistered(bindingFirst).ShouldBeTrue();
            manager.IsRegistered(bindingSecond).ShouldBeTrue();

            manager.ClearBindings(target, path1);
            manager.IsRegistered(bindingFirst).ShouldBeFalse();
            manager.IsRegistered(bindingSecond).ShouldBeTrue();
            manager.GetBindings(target).Single().ShouldEqual(bindingSecond);

            manager.ClearBindings(target, path2);
            manager.IsRegistered(bindingFirst).ShouldBeFalse();
            manager.IsRegistered(bindingSecond).ShouldBeFalse();
            manager.GetBindings(target).ShouldBeEmpty();
        }

        [TestMethod]
        public void ManagerShouldClearBindingIfItIsRegistered()
        {
            const string path = "test";
            var target = new object();
            var bindingMock = new DataBindingMock
            {
                TargetAccessor = new BindingSourceAccessorMock
                {
                    Source = new BindingSourceMock
                    {
                        Path = BindingPath.Create(path),
                        GetSource = b => target
                    }
                }
            };
            var manager = GetBindingManager();

            manager.Register(target, path, bindingMock);
            manager.IsRegistered(bindingMock).ShouldBeTrue();

            bindingMock.Dispose();
            manager.IsRegistered(bindingMock).ShouldBeFalse();
            bindingMock.IsDisposed.ShouldBeTrue();
        }

        protected virtual IBindingManager GetBindingManager()
        {
            return new BindingManager();
        }

        #endregion
    }
}