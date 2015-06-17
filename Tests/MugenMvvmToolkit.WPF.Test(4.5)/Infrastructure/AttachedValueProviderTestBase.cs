using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Silverlight.Infrastructure;
using MugenMvvmToolkit.Test.Collections;
using MugenMvvmToolkit.WinRT.Infrastructure;
using MugenMvvmToolkit.WPF.Infrastructure;
using Should;

namespace MugenMvvmToolkit.Test.Infrastructure
{
    [TestClass]
    public class AttachedValueProviderTestBase : TestBase
    {
        #region Fields

        private const string FirstPath = "first";
        private const string SecondPath = "second";
        private IAttachedValueProvider _provider;

        #endregion

        #region Methods

        [TestMethod]
        public void AddOrUpdateValueTest()
        {
            bool isInvoked = false;
            var target = new object();
            var value1 = new object();
            var value2 = new object();
            var stateObj = new object();
            UpdateValueDelegate<object, object, object, object> del = (item, newValue, oldValue, state) =>
            {
                item.ShouldEqual(target);
                newValue.ShouldEqual(value2);
                oldValue.ShouldEqual(value1);
                state.ShouldEqual(stateObj);
                isInvoked = true;
                return newValue;
            };
            _provider.AddOrUpdate(target, FirstPath, value1, del, stateObj);
            isInvoked.ShouldBeFalse();
            _provider.AddOrUpdate(target, FirstPath, value2, del, stateObj);
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void AddOrUpdateDelegateTest()
        {
            bool isInvoked = false;
            var target = new object();
            var value1 = new object();
            var value2 = new object();
            var stateObj = new object();
            UpdateValueDelegate<object, Func<object, object, object>, object, object> del = (item, newValue, oldValue, state) =>
            {
                item.ShouldEqual(target);
                oldValue.ShouldEqual(value1);
                state.ShouldEqual(stateObj);
                isInvoked = true;
                return newValue(item, state);
            };
            _provider.AddOrUpdate(target, FirstPath, (o, o1) =>
            {
                o.ShouldEqual(target);
                o1.ShouldEqual(stateObj);
                return value1;
            }, del, stateObj);
            isInvoked.ShouldBeFalse();
            _provider.AddOrUpdate(target, FirstPath, (o, o1) =>
            {
                o.ShouldEqual(target);
                o1.ShouldEqual(stateObj);
                return value2;
            }, del, stateObj);
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void GetOrAddValueTest()
        {
            var target = new object();
            var value1 = new object();
            var value2 = new object();

            _provider.GetOrAdd(target, FirstPath, value1).ShouldEqual(value1);
            _provider.GetOrAdd(target, FirstPath, value2).ShouldEqual(value1);

            _provider.GetOrAdd(target, SecondPath, value1).ShouldEqual(value1);
            _provider.GetOrAdd(target, SecondPath, value2).ShouldEqual(value1);
        }

        [TestMethod]
        public void GetOrAddDelegateTest()
        {
            var target = new object();
            var value1 = new object();
            var value2 = new object();
            var stateObj = new object();

            bool value1DelInvoked = false;
            Func<object, object, object> value1Del = (o, o1) =>
            {
                o.ShouldEqual(target);
                stateObj.ShouldEqual(o1);
                value1DelInvoked = true;
                return value1;
            };

            bool value2DelInvoked = false;
            Func<object, object, object> value2Del = (o, o1) =>
            {
                o.ShouldEqual(target);
                stateObj.ShouldEqual(o1);
                value2DelInvoked = true;
                return value2;
            };

            _provider.GetOrAdd(target, FirstPath, value1Del, stateObj).ShouldEqual(value1);
            _provider.GetOrAdd(target, FirstPath, value2Del, stateObj).ShouldEqual(value1);
            value1DelInvoked.ShouldBeTrue();
            value2DelInvoked.ShouldBeFalse();
            value1DelInvoked = false;

            _provider.GetOrAdd(target, SecondPath, value1Del, stateObj).ShouldEqual(value1);
            _provider.GetOrAdd(target, SecondPath, value2Del, stateObj).ShouldEqual(value1);
            value1DelInvoked.ShouldBeTrue();
            value2DelInvoked.ShouldBeFalse();
        }

        [TestMethod]
        public void TryGetValueTest()
        {
            var target = new object();
            var value1 = new object();

            object value;
            _provider.TryGetValue(target, FirstPath, out value).ShouldBeFalse();
            value.ShouldBeNull();

            _provider.SetValue(target, FirstPath, value1);
            _provider.TryGetValue(target, FirstPath, out value).ShouldBeTrue();
            value.ShouldEqual(value1);
        }

        [TestMethod]
        public void GetSetValueTest()
        {
            var target = new object();
            var value1 = new object();

            _provider.GetValue<object>(target, FirstPath, false).ShouldBeNull();
            ShouldThrow(() => _provider.GetValue<object>(target, FirstPath, true));

            _provider.SetValue(target, FirstPath, value1);
            _provider.GetValue<object>(target, FirstPath, false).ShouldEqual(value1);
            _provider.GetValue<object>(target, FirstPath, true).ShouldEqual(value1);
        }

        [TestMethod]
        public void GetValuesTest()
        {
            var target = new object();
            var value1 = new object();
            var value2 = new object();

            _provider.GetValues(target, (s, o) => true).ShouldBeEmpty();
            _provider.SetValue(target, FirstPath, value1);
            _provider.SetValue(target, SecondPath, value2);

            var list = _provider.GetValues(target, (s, o) => true);
            list.Any(pair => pair.Key == FirstPath).ShouldBeTrue();
            list.Any(pair => pair.Key == SecondPath).ShouldBeTrue();
            list.Any(pair => pair.Value == value1).ShouldBeTrue();
            list.Any(pair => pair.Value == value2).ShouldBeTrue();

            list = _provider.GetValues(target, (s, o) => s == FirstPath && o == value1);
            list.Single().Key.ShouldEqual(FirstPath);
            list.Single().Value.ShouldEqual(value1);
        }

        [TestMethod]
        public void ClearPathTest()
        {
            var target = new object();
            var value1 = new object();
            var value2 = new object();

            _provider.SetValue(target, FirstPath, value1);
            _provider.SetValue(target, SecondPath, value2);

            _provider.GetValue<object>(target, FirstPath, true).ShouldEqual(value1);
            _provider.Clear(target, FirstPath).ShouldBeTrue();
            _provider.GetValue<object>(target, FirstPath, false).ShouldBeNull();
            _provider.Clear(target, FirstPath).ShouldBeFalse();

            _provider.GetValue<object>(target, SecondPath, true).ShouldEqual(value2);
            _provider.Clear(target, SecondPath).ShouldBeTrue();
            _provider.GetValue<object>(target, SecondPath, false).ShouldBeNull();
            _provider.Clear(target, SecondPath).ShouldBeFalse();
        }

        [TestMethod]
        public void ClearTest()
        {
            var target = new object();
            var value1 = new object();
            var value2 = new object();

            _provider.SetValue(target, FirstPath, value1);
            _provider.SetValue(target, SecondPath, value2);

            _provider.GetValue<object>(target, FirstPath, true).ShouldEqual(value1);
            _provider.GetValue<object>(target, SecondPath, true).ShouldEqual(value2);
            _provider.Clear(target).ShouldBeTrue();

            _provider.GetValue<object>(target, FirstPath, false).ShouldBeNull();
            _provider.GetValue<object>(target, SecondPath, false).ShouldBeNull();
            _provider.Clear(target).ShouldBeFalse();
        }

        [TestMethod]
        public virtual void WeakItemsTest()
        {
            const int count = 1000;
            var list = new List<Item>();
            var dict = Create();
            for (int i = 0; i < count; i++)
            {
                var item = new Item();
                list.Add(item);
                dict.SetValue(item, FirstPath, i);
            }
            for (int index = 0; index < list.Count; index++)
            {
                var item = list[index];
                dict.GetValue<int>(item, FirstPath, true).ShouldEqual(index);
            }
            var references = list.Select(item => new WeakReference(item)).ToArray();
            list.Clear();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            references.All(reference => !reference.IsAlive).ShouldBeTrue();
        }

        protected virtual IAttachedValueProvider Create()
        {
            return new AttachedValueProvider();
        }

        #endregion

        #region Overrides of TestBase

        protected override void OnInit()
        {
            base.OnInit();
            _provider = Create();
        }

        #endregion
    }
}