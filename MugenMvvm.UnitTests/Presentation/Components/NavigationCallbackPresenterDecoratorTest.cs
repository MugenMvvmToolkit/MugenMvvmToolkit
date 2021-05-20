using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Presentation;
using MugenMvvm.Internal;
using MugenMvvm.Navigation;
using MugenMvvm.Presentation;
using MugenMvvm.Presentation.Components;
using MugenMvvm.UnitTests.Internal.Internal;
using MugenMvvm.UnitTests.Navigation.Internal;
using MugenMvvm.UnitTests.Presentation.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Presentation.Components
{
    public class NavigationCallbackPresenterDecoratorTest : UnitTestBase
    {
        private readonly Presenter _presenter;
        private readonly NavigationDispatcher _navigationDispatcher;

        public NavigationCallbackPresenterDecoratorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _navigationDispatcher = new NavigationDispatcher(ComponentCollectionManager);
            _presenter = new Presenter(ComponentCollectionManager);
            _presenter.AddComponent(new NavigationCallbackPresenterDecorator(_navigationDispatcher));
        }

        [Fact]
        public void ShowShouldAddCallbacks()
        {
            var suspended = false;
            var addedCallbacks = new List<(IPresenterResult, NavigationCallbackType)>();
            var presenterResult1 = new PresenterResult(this, "t1", NavigationProvider.System, NavigationType.Page);
            var presenterResult2 = new PresenterResult(this, "t2", NavigationProvider.System, NavigationType.Window);
            _presenter.AddComponent(new TestPresenterComponent
            {
                TryShow = (o, arg3, arg4) => new[] {presenterResult1, presenterResult2}
            });
            _navigationDispatcher.AddComponent(new TestNavigationCallbackManagerComponent
            {
                TryAddNavigationCallback = (callbackType, id, navType, target, m) =>
                {
                    suspended.ShouldBeTrue();
                    id.ShouldEqual(presenterResult1.NavigationType == navType ? presenterResult1.NavigationId : presenterResult2.NavigationId);
                    addedCallbacks.Add(((IPresenterResult) target, callbackType));
                    m.ShouldEqual(DefaultMetadata);
                    return null;
                }
            });
            _navigationDispatcher.Components.TryAdd(new TestSuspendableComponent
            {
                Suspend = (o, arg3) =>
                {
                    suspended.ShouldBeFalse();
                    suspended = true;
                    o.ShouldEqual(_presenter);
                    arg3.ShouldEqual(DefaultMetadata);
                    return ActionToken.FromDelegate((o1, o2) => suspended = false);
                }
            });

            _presenter.Show(this, default, DefaultMetadata);
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
            var presenterResult1 = new PresenterResult(this, "t1", NavigationProvider.System, NavigationType.Page);
            var presenterResult2 = new PresenterResult(this, "t2", NavigationProvider.System, NavigationType.Window);
            _presenter.AddComponent(new TestPresenterComponent
            {
                TryClose = (o, arg3, arg4) => new[] {presenterResult1, presenterResult2}
            });
            _navigationDispatcher.AddComponent(new TestNavigationCallbackManagerComponent
            {
                TryAddNavigationCallback = (callbackType, id, navType, target, m) =>
                {
                    suspended.ShouldBeTrue();
                    id.ShouldEqual(presenterResult1.NavigationType == navType ? presenterResult1.NavigationId : presenterResult2.NavigationId);
                    addedCallbacks.Add(((IPresenterResult) target, callbackType));
                    m.ShouldEqual(DefaultMetadata);
                    return null;
                }
            });
            _navigationDispatcher.Components.TryAdd(new TestSuspendableComponent
            {
                Suspend = (o, arg3) =>
                {
                    suspended.ShouldBeFalse();
                    suspended = true;
                    o.ShouldEqual(_presenter);
                    arg3.ShouldEqual(DefaultMetadata);
                    return ActionToken.FromDelegate((o1, o2) => suspended = false);
                }
            });

            _presenter.TryClose(this, default, DefaultMetadata);
            suspended.ShouldBeFalse();
            addedCallbacks.Count.ShouldEqual(2);
            addedCallbacks.ShouldContain((presenterResult1, NavigationCallbackType.Closing));
            addedCallbacks.ShouldContain((presenterResult2, NavigationCallbackType.Closing));
        }
    }
}