using System;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Interfaces.Models;
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

        private sealed class HandlerToDelegate : IHandler<ValueChangedEventArgs>
        {
            #region Fields

            private readonly EventHandler<ValueChangedEventArgs> _handler;

            #endregion

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="HandlerToDelegate"/> class.
            /// </summary>
            public HandlerToDelegate(EventHandler<ValueChangedEventArgs> handler)
            {
                _handler = handler;
            }

            #endregion

            #region Implementation of IHandler<in ValueChangedEventArgs>

            /// <summary>
            ///     Handles the message.
            /// </summary>
            /// <param name="sender">The object that raised the event.</param>
            /// <param name="message">Information about event.</param>
            public void Handle(object sender, ValueChangedEventArgs message)
            {
                _handler(sender, message);
            }

            #endregion
        }

        #endregion

        #region Methods

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
            observer.Listener = new HandlerToDelegate((sender, args) => isInvoked = true);

            isInvoked = false;
            model.StringProperty = "test";
            isInvoked.ShouldBeTrue();
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
            observer.Listener = new HandlerToDelegate((sender, args) => isInvoked = true);

            isInvoked = false;
            model.ObjectProperty = "test";
            model.RaiseObjectPropertyChanged();
            isInvoked.ShouldBeTrue();
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
            observer.Listener = new HandlerToDelegate((sender, args) => isInvoked = true);

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
        public void ObserverShouldRaiseValueChangedEventWhenPropertyChangedComplexPropertyUsingEvent()
        {
            bool isInvoked = false;
            var model = new BindingSourceEventNotifierModel();
            var propertyName = GetMemberPath(model, sourceModel => sourceModel.NestedModel.ObjectProperty);

            var observer = CreateObserver(model, propertyName, false);
            observer.Listener = new HandlerToDelegate((sender, args) => isInvoked = true);

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
        public void ObserverShouldRaiseValueChangedEventWhenPropertyChangedIndexer()
        {
            bool isInvoked = false;
            var model = new BindingSourceModel();
            var propertyName = GetMemberPath(model, sourceModel => sourceModel["test"]);
            var observer = CreateObserver(model, propertyName, false);
            observer.Listener = new HandlerToDelegate((sender, args) => isInvoked = true);

            isInvoked = false;
            model["test"] = "test";
            isInvoked.ShouldBeFalse();
            model.OnPropertyChanged("Item[]");
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
            observer.Listener = new HandlerToDelegate((sender, args) => isInvoked = true);

            isInvoked = false;
            model.NestedModel = new BindingSourceNestedModel();
            isInvoked.ShouldBeTrue();
            isInvoked = false;

            model.NestedModel["test"] = "test";
            isInvoked.ShouldBeFalse();
            model.NestedModel.OnPropertyChanged("Item[]");
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
            observer.Listener = new HandlerToDelegate((sender, args) => isInvoked = true);

            isInvoked = false;
            model["test", 0] = "test";
            isInvoked.ShouldBeFalse();
            model.OnPropertyChanged("Item[]");
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
            observer.Listener = new HandlerToDelegate((sender, args) => isInvoked = true);

            isInvoked = false;
            model.NestedModel = new BindingSourceNestedModel();
            isInvoked.ShouldBeTrue();
            isInvoked = false;

            model.NestedModel["test", 0] = "test";
            isInvoked.ShouldBeFalse();
            model.NestedModel.OnPropertyChanged("Item[]");
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
            observer.Listener = new HandlerToDelegate((sender, args) => isInvoked = true);

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
            observer.Listener = new HandlerToDelegate((sender, args) => isInvoked = true);

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
            IBindingMemberProvider memberProvider = null, IBindingContextManager contextManager = null)
        {
            if (memberProvider != null)
                BindingServiceProvider.MemberProvider = memberProvider;
            if (contextManager != null)
                BindingServiceProvider.ContextManager = contextManager;
            var bindingPath = BindingPath.Create(path);
            if (bindingPath.IsEmpty)
                return new EmptyPathObserver(source, bindingPath);
            if (bindingPath.IsSingle)
                return new SinglePathObserver(source, bindingPath, ignoreContext);
            return new MultiPathObserver(source, bindingPath, ignoreContext);
        }

        #endregion
    }
}