using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Internal;
using MugenMvvm.Navigation;
using MugenMvvm.Presenters;
using MugenMvvm.Presenters.Components;
using MugenMvvm.UnitTest.Internal.Internal;
using MugenMvvm.UnitTest.Navigation.Internal;
using MugenMvvm.UnitTest.Presenters.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Presenters.Components
{
    public class NavigationCallbackPresenterDecoratorTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ShowShouldAddCallbacks()
        {
            var suspended = false;
            var addedCallbacks = new List<(IPresenterResult, NavigationCallbackType)>();
            var presenterResult1 = new PresenterResult(this, "t", Default.NavigationProvider, NavigationType.Tab);
            var presenterResult2 = new PresenterResult(this, "t", Default.NavigationProvider, NavigationType.Tab);
            var presenter = new Presenter();
            var navigationDispatcher = new NavigationDispatcher();
            presenter.AddComponent(new NavigationCallbackPresenterDecorator(navigationDispatcher));
            presenter.AddComponent(new TestPresenterComponent
            {
                TryShow = (o, arg3, arg4) => new[] { presenterResult1, presenterResult2 }
            });
            navigationDispatcher.AddComponent(new TestNavigationCallbackManagerComponent
            {
                TryAddNavigationCallback = (callbackType, target, m) =>
                {
                    suspended.ShouldBeTrue();
                    addedCallbacks.Add(((IPresenterResult)target, callbackType));
                    m.ShouldEqual(DefaultMetadata);
                    return null;
                }
            });
            navigationDispatcher.Components.Add(new TestSuspendableComponent
            {
                Suspend = (o, arg3) =>
                {
                    suspended.ShouldBeFalse();
                    suspended = true;
                    o.ShouldEqual(presenter);
                    arg3.ShouldEqual(DefaultMetadata);
                    return new ActionToken((o1, o2) => suspended = false);
                }
            });

            presenter.Show(this, default, DefaultMetadata);
            suspended.ShouldBeFalse();
            addedCallbacks.Count.ShouldEqual(4);
            addedCallbacks.ShouldContain((presenterResult1, NavigationCallbackType.Showing));
            addedCallbacks.ShouldContain((presenterResult1, NavigationCallbackType.Close));
            addedCallbacks.ShouldContain((presenterResult2, NavigationCallbackType.Showing));
            addedCallbacks.ShouldContain((presenterResult2, NavigationCallbackType.Close));
        }

        [Fact]
        public void TryCloseShouldAddCallbacks()
        {
            var suspended = false;
            var addedCallbacks = new List<(IPresenterResult, NavigationCallbackType)>();
            var presenterResult1 = new PresenterResult(this, "t", Default.NavigationProvider, NavigationType.Tab);
            var presenterResult2 = new PresenterResult(this, "t", Default.NavigationProvider, NavigationType.Tab);
            var presenter = new Presenter();
            var navigationDispatcher = new NavigationDispatcher();
            presenter.AddComponent(new NavigationCallbackPresenterDecorator(navigationDispatcher));
            presenter.AddComponent(new TestPresenterComponent
            {
                TryClose = (o, arg3, arg4) => new[] { presenterResult1, presenterResult2 }
            });
            navigationDispatcher.AddComponent(new TestNavigationCallbackManagerComponent
            {
                TryAddNavigationCallback = (callbackType, target, m) =>
                {
                    suspended.ShouldBeTrue();
                    addedCallbacks.Add(((IPresenterResult)target, callbackType));
                    m.ShouldEqual(DefaultMetadata);
                    return null;
                }
            });
            navigationDispatcher.Components.Add(new TestSuspendableComponent
            {
                Suspend = (o, arg3) =>
                {
                    suspended.ShouldBeFalse();
                    suspended = true;
                    o.ShouldEqual(presenter);
                    arg3.ShouldEqual(DefaultMetadata);
                    return new ActionToken((o1, o2) => suspended = false);
                }
            });

            presenter.TryClose(this, default, DefaultMetadata);
            suspended.ShouldBeFalse();
            addedCallbacks.Count.ShouldEqual(2);
            addedCallbacks.ShouldContain((presenterResult1, NavigationCallbackType.Closing));
            addedCallbacks.ShouldContain((presenterResult2, NavigationCallbackType.Closing));
        }

        #endregion
    }
}