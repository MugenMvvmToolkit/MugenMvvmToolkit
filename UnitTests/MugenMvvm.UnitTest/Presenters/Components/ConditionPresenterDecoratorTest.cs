using System;
using System.Linq;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Presenters;
using MugenMvvm.Presenters.Components;
using MugenMvvm.UnitTest.Navigation.Internal;
using MugenMvvm.UnitTest.Presenters.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Presenters.Components
{
    public class ConditionPresenterDecoratorTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryShowShouldBeHandledByComponents()
        {
            var result = new PresenterResult(this, "2", new TestNavigationProvider(), NavigationType.Alert, DefaultMetadata);
            var presenter = new Presenter();
            presenter.AddComponent(new ConditionPresenterDecorator());

            var invoked = 0;
            var presenterComponent = new TestPresenterComponent(presenter)
            {
                TryShow = (o, arg3, arg4) =>
                {
                    ++invoked;
                    return new[] { result };
                }
            };
            presenter.AddComponent(presenterComponent);

            var canExecute = false;
            var component = new TestConditionPresenterComponent(presenter)
            {
                CanShow = (c, results, r, m) =>
                {
                    c.ShouldEqual(presenterComponent);
                    results.AsList().ShouldBeEmpty();
                    r.ShouldEqual(presenter);
                    m.ShouldEqual(DefaultMetadata);
                    return canExecute;
                }
            };
            presenter.AddComponent(component);

            ShouldThrow<InvalidOperationException>(() => presenter.Show(presenter, default, DefaultMetadata));
            invoked.ShouldEqual(0);

            canExecute = true;
            presenter.Show(presenter, default, DefaultMetadata).AsList().Single().ShouldEqual(result);
            invoked.ShouldEqual(1);
        }

        [Fact]
        public void TryCloseShouldBeHandledByComponents()
        {
            var presenter = new Presenter();
            presenter.AddComponent(new ConditionPresenterDecorator());

            var result = new PresenterResult(this, "2", new TestNavigationProvider(), NavigationType.Alert, DefaultMetadata);
            var invoked = 0;
            var presenterComponent = new TestPresenterComponent(presenter)
            {
                TryClose = (o, arg3, arg4) =>
                {
                    ++invoked;
                    return new[] { result };
                }
            };
            presenter.AddComponent(presenterComponent);

            var canExecute = false;
            var component = new TestConditionPresenterComponent
            {
                CanClose = (c, results, r, m) =>
                {
                    c.ShouldEqual(presenterComponent);
                    results.AsList().ShouldBeEmpty();
                    r.ShouldEqual(presenter);
                    m.ShouldEqual(DefaultMetadata);
                    return canExecute;
                }
            };
            presenter.AddComponent(component);

            presenter.TryClose(presenter, default, DefaultMetadata).AsList().ShouldBeEmpty();
            invoked.ShouldEqual(0);

            canExecute = true;
            presenter.TryClose(presenter, default, DefaultMetadata).AsList().Single().ShouldEqual(result);
            invoked.ShouldEqual(1);
        }

        #endregion
    }
}