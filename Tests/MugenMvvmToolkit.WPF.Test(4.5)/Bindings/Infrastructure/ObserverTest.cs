#region Copyright

// ****************************************************************************
// <copyright file="ObserverTest.cs">
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
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Test.TestModels;
using Should;

namespace MugenMvvmToolkit.Test.Bindings.Infrastructure
{
    [TestClass]
    public class ObserverTest : BindingTestBase
    {
        #region Nested types

        private sealed class Source
        {
            public int StringProperty { get; set; }
        }

        #endregion

        #region Methods

        [TestMethod]
        public void ObserverShouldThrowExceptionInvalidPath()
        {
            var model = new BindingSourceModel();
            var observer = CreateObserver(model, "invalid", false, optional: false);

            observer.Source.ShouldEqual(model);
            ShouldThrow(() =>
            {
                var members = observer.GetPathMembers(true);
            });
        }

        [TestMethod]
        public void ObserverShouldNotThrowExceptionInvalidPathOptional()
        {
            var model = new BindingSourceModel();
            var observer = CreateObserver(model, "invalid", false, optional: true);

            observer.Source.ShouldEqual(model);
            var members = observer.GetPathMembers(true);
            members.AllMembersAvailable.ShouldBeFalse();

            members = observer.GetPathMembers(false);
            members.AllMembersAvailable.ShouldBeFalse();
        }

        [TestMethod]
        public void ObserverShouldUseObjectAsSource()
        {
            var model = new BindingSourceModel();
            var propertyName = GetMemberPath<BindingSourceModel>(sourceModel => sourceModel.StringProperty);
            var observer = CreateObserver(model, propertyName, false);

            observer.Source.ShouldEqual(model);
            var members = observer.GetPathMembers(true);
            members.AllMembersAvailable.ShouldBeTrue();
            members.Members.Single().Type.ShouldEqual(typeof(string));
            observer.Path.Path.ShouldEqual(propertyName);
        }

        [TestMethod]
        public void ObserverShouldUseBindingContextAsSource()
        {
            var model = new BindingSourceModel();
            var contextMock = new BindingContextMock { Value = model };
            var propertyName = GetMemberPath<BindingSourceModel>(sourceModel => sourceModel.StringProperty);
            var observer = CreateObserver(contextMock, propertyName, false);

            observer.Source.ShouldEqual(contextMock);
            var members = observer.GetPathMembers(true);
            members.AllMembersAvailable.ShouldBeTrue();
            members.Members.Single().Type.ShouldEqual(typeof(string));
            observer.Path.Path.ShouldEqual(propertyName);
        }

        [TestMethod]
        public void ObserverShouldRaiseValueChangedEventWhenPropertyChanged()
        {
            bool isInvoked = false;
            var model = new BindingSourceModel();
            var propertyName = GetMemberPath<BindingSourceModel>(sourceModel => sourceModel.StringProperty);
            var observer = CreateObserver(model, propertyName, false);
            observer.ValueChanged += (sender, args) => isInvoked = true;

            isInvoked = false;
            model.StringProperty = "test";
            isInvoked.ShouldBeTrue();
            isInvoked = false;

            model.IntProperty = 10;
            isInvoked.ShouldBeFalse();
        }

        [TestMethod]
        public void ObserverShouldNotRaiseValueChangedEventWhenPropertyChangedObservableFalse()
        {
            bool isInvoked = false;
            var model = new BindingSourceModel();
            var propertyName = GetMemberPath<BindingSourceModel>(sourceModel => sourceModel.StringProperty);
            var observer = CreateObserver(model, propertyName, false, observable: false);
            observer.ValueChanged += (sender, args) => isInvoked = true;

            isInvoked = false;
            model.StringProperty = "test";
            isInvoked.ShouldBeFalse();
            isInvoked = false;

            model.IntProperty = 10;
            isInvoked.ShouldBeFalse();
        }

        [TestMethod]
        public void ObserverShouldRaiseValueChangedEventWhenPropertyChangedUsingEvent()
        {
            bool isInvoked = false;
            var model = new BindingSourceEventNotifierModel();
            var propertyName = GetMemberPath<BindingSourceEventNotifierModel>(sourceModel => sourceModel.ObjectProperty);
            var observer = CreateObserver(model, propertyName, false);
            observer.ValueChanged += (sender, args) => isInvoked = true;

            isInvoked = false;
            model.ObjectProperty = "test";
            model.RaiseObjectPropertyChanged();
            isInvoked.ShouldBeTrue();
            isInvoked = false;

            model.ObjectProperty = "test1";
            isInvoked.ShouldBeFalse();
        }

        [TestMethod]
        public void ObserverShouldNotRaiseValueChangedEventWhenPropertyChangedUsingEventObservableFalse()
        {
            bool isInvoked = false;
            var model = new BindingSourceEventNotifierModel();
            var propertyName = GetMemberPath<BindingSourceEventNotifierModel>(sourceModel => sourceModel.ObjectProperty);
            var observer = CreateObserver(model, propertyName, false, observable: false);
            observer.ValueChanged += (sender, args) => isInvoked = true;

            isInvoked = false;
            model.ObjectProperty = "test";
            model.RaiseObjectPropertyChanged();
            isInvoked.ShouldBeFalse();
            isInvoked = false;

            model.ObjectProperty = "test1";
            isInvoked.ShouldBeFalse();
        }

        [TestMethod]
        public void ObserverShouldRaiseValueChangedEventWhenPropertyChangedComplexProperty()
        {
            bool isInvoked = false;
            var model = new BindingSourceModel();
            var propertyName = GetMemberPath(model, sourceModel => sourceModel.NestedModel.StringProperty);

            var observer = CreateObserver(model, propertyName, false);
            observer.ValueChanged += (sender, args) => isInvoked = true;

            isInvoked = false;
            model.NestedModel = new BindingSourceNestedModel();
            isInvoked.ShouldBeTrue();

            isInvoked = false;
            model.NestedModel.StringProperty = "test";
            isInvoked.ShouldBeTrue();
            isInvoked = false;

            model.NestedModel.IntProperty = 10;
            isInvoked.ShouldBeFalse();
        }

        [TestMethod]
        public void ObserverShouldNotRaiseValueChangedEventWhenPropertyChangedComplexPropertyObservableFalse()
        {
            bool isInvoked = false;
            var model = new BindingSourceModel();
            var propertyName = GetMemberPath(model, sourceModel => sourceModel.NestedModel.StringProperty);

            var observer = CreateObserver(model, propertyName, false, observable: false);
            observer.ValueChanged += (sender, args) => isInvoked = true;

            isInvoked = false;
            model.NestedModel = new BindingSourceNestedModel();
            isInvoked.ShouldBeFalse();

            isInvoked = false;
            model.NestedModel.StringProperty = "test";
            isInvoked.ShouldBeFalse();
            isInvoked = false;

            model.NestedModel.IntProperty = 10;
            isInvoked.ShouldBeFalse();
        }

        [TestMethod]
        public void ObserverShouldRaiseValueChangedEventWhenPropertyChangedComplexPropertyUsingEvent()
        {
            bool isInvoked = false;
            var model = new BindingSourceEventNotifierModel();
            var propertyName = GetMemberPath(model, sourceModel => sourceModel.NestedModel.ObjectProperty);

            var observer = CreateObserver(model, propertyName, false);
            observer.ValueChanged += (sender, args) => isInvoked = true;

            isInvoked = false;
            model.NestedModel = new BindingSourceEventNotifierModel();
            model.RaiseNestedModelChanged();
            isInvoked.ShouldBeTrue();

            isInvoked = false;
            model.NestedModel.ObjectProperty = "test";
            model.NestedModel.RaiseObjectPropertyChanged();
            isInvoked.ShouldBeTrue();
            isInvoked = false;

            model.NestedModel.ObjectProperty = 10;
            isInvoked.ShouldBeFalse();
        }

        [TestMethod]
        public void ObserverShouldNotRaiseValueChangedEventWhenPropertyChangedComplexPropertyUsingEventObservableFalse()
        {
            bool isInvoked = false;
            var model = new BindingSourceEventNotifierModel();
            var propertyName = GetMemberPath(model, sourceModel => sourceModel.NestedModel.ObjectProperty);

            var observer = CreateObserver(model, propertyName, false, observable: false);
            observer.ValueChanged += (sender, args) => isInvoked = true;

            isInvoked = false;
            model.NestedModel = new BindingSourceEventNotifierModel();
            model.RaiseNestedModelChanged();
            isInvoked.ShouldBeFalse();

            isInvoked = false;
            model.NestedModel.ObjectProperty = "test";
            model.NestedModel.RaiseObjectPropertyChanged();
            isInvoked.ShouldBeFalse();
            isInvoked = false;

            model.NestedModel.ObjectProperty = 10;
            isInvoked.ShouldBeFalse();
        }

        [TestMethod]
        public void ObserverShouldRaiseValueChangedEventWhenPropertyChangedIndexer()
        {
            bool isInvoked = false;
            var model = new BindingSourceModel();
            var propertyName = GetMemberPath(model, sourceModel => sourceModel["test"]);
            var observer = CreateObserver(model, propertyName, false);
            observer.ValueChanged += (sender, args) => isInvoked = true;

            isInvoked = false;
            model["test"] = "test";
            isInvoked.ShouldBeFalse();
            model.OnPropertyChanged(ReflectionExtensions.IndexerName);
            isInvoked.ShouldBeTrue();
            isInvoked = false;

            model["test"] = "test1";
            isInvoked.ShouldBeFalse();
            model.OnPropertyChanged("Item[test]");
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void ObserverShouldRaiseValueChangedEventWhenPropertyChangedIndexerComplexProperty()
        {
            bool isInvoked = false;
            var model = new BindingSourceModel();
            var propertyName = GetMemberPath(model, sourceModel => sourceModel.NestedModel["test"]);

            var observer = CreateObserver(model, propertyName, false);
            observer.ValueChanged += (sender, args) => isInvoked = true;

            isInvoked = false;
            model.NestedModel = new BindingSourceNestedModel();
            isInvoked.ShouldBeTrue();
            isInvoked = false;

            model.NestedModel["test"] = "test";
            isInvoked.ShouldBeFalse();
            model.NestedModel.OnPropertyChanged(ReflectionExtensions.IndexerName);
            isInvoked.ShouldBeTrue();
            isInvoked = false;

            model.NestedModel["test"] = "test1";
            isInvoked.ShouldBeFalse();
            model.NestedModel.OnPropertyChanged("Item[test]");
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void ObserverShouldRaiseValueChangedEventWhenPropertyChangedDoubleIndexer()
        {
            bool isInvoked = false;
            var model = new BindingSourceModel();
            var propertyName = GetMemberPath(model, sourceModel => sourceModel["test", 0]);
            var observer = CreateObserver(model, propertyName, false);
            observer.ValueChanged += (sender, args) => isInvoked = true;

            isInvoked = false;
            model["test", 0] = "test";
            isInvoked.ShouldBeFalse();
            model.OnPropertyChanged(ReflectionExtensions.IndexerName);
            isInvoked.ShouldBeTrue();
            isInvoked = false;

            model["test", 0] = "test1";
            isInvoked.ShouldBeFalse();
            model.OnPropertyChanged("Item[test,0]");
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void ObserverShouldRaiseValueChangedEventWhenPropertyChangedDoubleIndexerComplexProperty()
        {
            bool isInvoked = false;
            var model = new BindingSourceModel();
            var propertyName = GetMemberPath(model, sourceModel => sourceModel.NestedModel["test", 0]);

            var observer = CreateObserver(model, propertyName, false);
            observer.ValueChanged += (sender, args) => isInvoked = true;

            isInvoked = false;
            model.NestedModel = new BindingSourceNestedModel();
            isInvoked.ShouldBeTrue();
            isInvoked = false;

            model.NestedModel["test", 0] = "test";
            isInvoked.ShouldBeFalse();
            model.NestedModel.OnPropertyChanged(ReflectionExtensions.IndexerName);
            isInvoked.ShouldBeTrue();
            isInvoked = false;

            model.NestedModel["test", 0] = "test1";
            isInvoked.ShouldBeFalse();
            model.NestedModel.OnPropertyChanged("Item[test,0]");
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void ObserverShouldNotRaiseValueChangedEventForOldComplexProperty()
        {
            bool isInvoked = false;
            var model = new BindingSourceModel();
            var propertyName = GetMemberPath(model, sourceModel => sourceModel.NestedModel.StringProperty);

            var observer = CreateObserver(model, propertyName, false);
            observer.ValueChanged += (sender, args) => isInvoked = true;

            isInvoked = false;
            model.NestedModel = new BindingSourceNestedModel();
            isInvoked.ShouldBeTrue();

            isInvoked = false;
            model.NestedModel.StringProperty = "test";
            isInvoked.ShouldBeTrue();

            var oldProperty = model.NestedModel;
            model.NestedModel = new BindingSourceNestedModel();
            isInvoked = false;

            oldProperty.StringProperty = string.Empty;
            isInvoked.ShouldBeFalse();
        }

        [TestMethod]
        public void ObserverShouldUpdateInformationWhenContextChanged()
        {
            var model = new BindingSourceModel();
            var contextMock = new BindingContextMock { Value = model, Source = new object() };
            var propertyName = GetMemberPath<BindingSourceModel>(sourceModel => sourceModel.StringProperty);
            var observer = CreateObserver(contextMock, propertyName, false);
            var members = observer.GetPathMembers(true);
            members.Members.Single().Type.ShouldEqual(typeof(string));
            observer.Path.Path.ShouldEqual(propertyName);

            contextMock.Value = new Source();
            members = observer.GetPathMembers(true);
            members.Members.Single().Type.ShouldEqual(typeof(int));
            observer.Path.Path.ShouldEqual(propertyName);
        }

        [TestMethod]
        public void ObserverShouldUpdateInformationWhenContextChangedHasStablePathTrue()
        {
            var model = new BindingSourceModel { StringProperty = "st" };
            var contextMock = new BindingContextMock { Value = model, Source = new object() };
            var propertyName = GetMemberPath<BindingSourceModel>(sourceModel => sourceModel.StringProperty);
            var observer = CreateObserver(contextMock, propertyName, false, hasStablePath: true);
            var members = observer.GetPathMembers(true);
            members.Members.Single().Type.ShouldEqual(typeof(string));
            observer.Path.Path.ShouldEqual(propertyName);
            members.LastMember.GetValue(members.PenultimateValue, null).ShouldEqual(model.StringProperty);

            var model2 = new BindingSourceModel { StringProperty = "st1" };
            contextMock.Value = model2;
            members = observer.GetPathMembers(true);
            members.Members.Single().Type.ShouldEqual(typeof(string));
            observer.Path.Path.ShouldEqual(propertyName);
            members.LastMember.GetValue(members.PenultimateValue, null).ShouldEqual(model2.StringProperty);
        }

        [TestMethod]
        public void ObserverShouldUpdateInformationWhenContextChangedHasStablePathTrueComplexPath()
        {
            var model = new BindingSourceModel { NestedModel = new BindingSourceNestedModel { StringProperty = "st" } };
            var contextMock = new BindingContextMock { Value = model, Source = new object() };
            var propertyName = GetMemberPath<BindingSourceModel>(sourceModel => sourceModel.NestedModel.StringProperty);
            var observer = CreateObserver(contextMock, propertyName, false, hasStablePath: true);
            var members = observer.GetPathMembers(true);
            members.Members.Last().Type.ShouldEqual(typeof(string));
            observer.Path.Path.ShouldEqual(propertyName);
            members.LastMember.GetValue(members.PenultimateValue, null).ShouldEqual(model.NestedModel.StringProperty);

            var model2 = new BindingSourceModel { NestedModel = new BindingSourceNestedModel { StringProperty = "st1" } };
            contextMock.Value = model2;
            members = observer.GetPathMembers(true);
            members.Members.Last().Type.ShouldEqual(typeof(string));
            observer.Path.Path.ShouldEqual(propertyName);
            members.LastMember.GetValue(members.PenultimateValue, null).ShouldEqual(model2.NestedModel.StringProperty);
        }

        [TestMethod]
        public void ObserverShouldUpdateInformationWhenContextChangedNotValidContext()
        {
            var model = new BindingSourceModel();
            var contextMock = new BindingContextMock { Value = model, Source = new object() };
            var propertyName = GetMemberPath<BindingSourceModel>(sourceModel => sourceModel.StringProperty);
            var observer = CreateObserver(contextMock, propertyName, false);
            var members = observer.GetPathMembers(true);
            members.Members.Single().Type.ShouldEqual(typeof(string));
            observer.Path.Path.ShouldEqual(propertyName);

            contextMock.Value = new object();
            members = observer.GetPathMembers(false);
            members.Members.ShouldBeEmpty();
            observer.Path.Path.ShouldEqual(propertyName);
        }

        [TestMethod]
        public void ObserverShouldClearOnDispose()
        {
            bool isInvoked = false;
            var model = new BindingSourceModel();
            var propertyName = GetMemberPath<BindingSourceModel>(sourceModel => sourceModel.StringProperty);
            var observer = CreateObserver(model, propertyName, false);
            observer.ValueChanged += (sender, args) => isInvoked = true;

            isInvoked = false;
            model.StringProperty = "test";
            isInvoked.ShouldBeTrue();
            isInvoked = false;

            observer.Dispose();
            model.StringProperty = "test";
            isInvoked.ShouldBeFalse();
        }

        [TestMethod]
        public void ObserverShouldUseWeakReferenceForTarget()
        {
            var model = new BindingSourceModel();
            var propertyName = GetMemberPath<BindingSourceModel>(sourceModel => sourceModel.StringProperty);
            var observer = CreateObserver(model, propertyName, false);
            observer.Source.ShouldEqual(model);

            model = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            observer.Source.ShouldBeNull();
        }

        protected virtual IObserver CreateObserver(object source, string path, bool ignoreContext,
            IBindingMemberProvider memberProvider = null, IBindingContextManager contextManager = null, bool hasStablePath = false, bool observable = true, bool optional = false)
        {
            if (memberProvider != null)
                BindingServiceProvider.MemberProvider = memberProvider;
            if (contextManager != null)
                BindingServiceProvider.ContextManager = contextManager;
            var bindingPath = new BindingPath(path);
            if (bindingPath.IsEmpty)
                return new EmptyPathObserver(source, bindingPath);
            if (bindingPath.IsSingle)
                return new SinglePathObserver(source, bindingPath, ignoreContext, hasStablePath, observable, optional);
            return new MultiPathObserver(source, bindingPath, ignoreContext, hasStablePath, observable, optional);
        }

        #endregion
    }
}
