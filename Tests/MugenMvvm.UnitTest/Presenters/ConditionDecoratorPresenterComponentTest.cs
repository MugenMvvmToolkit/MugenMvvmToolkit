using System;
using System.Linq;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Presenters;
using MugenMvvm.Presenters.Components;
using MugenMvvm.UnitTest.Navigation;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Presenters
{
    public class ConditionDecoratorPresenterComponentTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryShowShouldBeHandledByComponents()
        {
            var presenter = new Presenter();
            presenter.AddComponent(new ConditionPresenterDecorator());

            var canExecute = false;
            var component = new TestConditionPresenterComponent
            {
                CanShow = (component1, o, arg3, arg4) => canExecute
            };
            presenter.AddComponent(component);

            var result = new PresenterResult("2", new TestNavigationProvider(), NavigationType.Alert, DefaultMetadata);
            var invoked = 0;
            var presenterComponent = new TestPresenterComponent
            {
                TryShow = (o, type, arg3, arg4) =>
                {
                    ++invoked;
                    return result;
                }
            };
            presenter.AddComponent(presenterComponent);

            ShouldThrow<InvalidOperationException>(() => presenter.Show(presenter));
            invoked.ShouldEqual(0);

            canExecute = true;
            presenter.Show(presenter).ShouldEqual(result);
            invoked.ShouldEqual(1);
        }

        [Fact]
        public void TryCloseShouldBeHandledByComponents()
        {
            var presenter = new Presenter();
            presenter.AddComponent(new ConditionPresenterDecorator());

            var canExecute = false;
            var component = new TestConditionPresenterComponent
            {
                CanClose = (component1, list, arg3, arg4, arg5) => canExecute
            };
            presenter.AddComponent(component);

            var result = new PresenterResult("2", new TestNavigationProvider(), NavigationType.Alert, DefaultMetadata);
            var invoked = 0;
            var presenterComponent = new TestPresenterComponent
            {
                TryClose = (o, type, arg3, arg4) =>
                {
                    ++invoked;
                    return new[] {result};
                }
            };
            presenter.AddComponent(presenterComponent);

            presenter.TryClose(presenter).ShouldBeEmpty();
            invoked.ShouldEqual(0);

            canExecute = true;
            presenter.TryClose(presenter).Single().ShouldEqual(result);
            invoked.ShouldEqual(1);
        }

        [Fact]
        public void TryRestoreShouldBeHandledByComponents()
        {
            var presenter = new Presenter();
            presenter.AddComponent(new ConditionPresenterDecorator());

            var canExecute = false;
            var component = new TestConditionPresenterComponent
            {
                CanRestore = (component1, list, arg3, arg4, arg5) => canExecute
            };
            presenter.AddComponent(component);

            var result = new PresenterResult("2", new TestNavigationProvider(), NavigationType.Alert, DefaultMetadata);
            var invoked = 0;
            var presenterComponent = new TestPresenterComponent
            {
                TryRestore = (o, type, arg3, arg4) =>
                {
                    ++invoked;
                    return new[] {result};
                }
            };
            presenter.AddComponent(presenterComponent);

            presenter.TryRestore(presenter).ShouldBeEmpty();
            invoked.ShouldEqual(0);

            canExecute = true;
            presenter.TryRestore(presenter).Single().ShouldEqual(result);
            invoked.ShouldEqual(1);
        }

        #endregion
    }
}