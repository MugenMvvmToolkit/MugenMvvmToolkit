using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presentation;
using MugenMvvm.Internal;
using MugenMvvm.Navigation;
using MugenMvvm.Presentation;
using MugenMvvm.Presentation.Components;
using MugenMvvm.Tests.Internal;
using MugenMvvm.Tests.Navigation;
using MugenMvvm.Tests.Presentation;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Presentation.Components
{
    public class NavigationCallbackPresenterDecoratorTest : UnitTestBase
    {
        public NavigationCallbackPresenterDecoratorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            Presenter.AddComponent(new NavigationCallbackPresenterDecorator(NavigationDispatcher));
        }

        [Fact]
        public void ShowShouldAddCallbacks()
        {
            var suspended = false;
            var addedCallbacks = new List<(IPresenterResult, NavigationCallbackType)>();
            var presenterResult1 = new PresenterResult(this, "t1", NavigationProvider.System, NavigationType.Page);
            var presenterResult2 = new PresenterResult(this, "t2", NavigationProvider.System, NavigationType.Window);
            Presenter.AddComponent(new TestPresenterComponent
            {
                TryShow = (_, _, _, _) => new[] { presenterResult1, presenterResult2 }
            });
            NavigationDispatcher.AddComponent(new TestNavigationCallbackManagerComponent
            {
                TryAddNavigationCallback = (_, callbackType, id, navType, target, m) =>
                {
                    suspended.ShouldBeTrue();
                    id.ShouldEqual(presenterResult1.NavigationType == navType ? presenterResult1.NavigationId : presenterResult2.NavigationId);
                    addedCallbacks.Add(((IPresenterResult)target, callbackType));
                    m.ShouldEqual(Metadata);
                    return null;
                }
            });
            NavigationDispatcher.Components.TryAdd(new TestSuspendableComponent<INavigationDispatcher>
            {
                TrySuspend = (d, o, arg3) =>
                {
                    d.ShouldEqual(NavigationDispatcher);
                    suspended.ShouldBeFalse();
                    suspended = true;
                    o.ShouldEqual(Presenter);
                    arg3.ShouldEqual(Metadata);
                    return ActionToken.FromDelegate((_, _) => suspended = false);
                }
            });

            Presenter.Show(this, default, Metadata);
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
            Presenter.AddComponent(new TestPresenterComponent
            {
                TryClose = (_, _, _, _) => new[] { presenterResult1, presenterResult2 }
            });
            NavigationDispatcher.AddComponent(new TestNavigationCallbackManagerComponent
            {
                TryAddNavigationCallback = (_, callbackType, id, navType, target, m) =>
                {
                    suspended.ShouldBeTrue();
                    id.ShouldEqual(presenterResult1.NavigationType == navType ? presenterResult1.NavigationId : presenterResult2.NavigationId);
                    addedCallbacks.Add(((IPresenterResult)target, callbackType));
                    m.ShouldEqual(Metadata);
                    return null;
                }
            });
            NavigationDispatcher.Components.TryAdd(new TestSuspendableComponent<INavigationDispatcher>
            {
                TrySuspend = (d, o, arg3) =>
                {
                    d.ShouldEqual(NavigationDispatcher);
                    suspended.ShouldBeFalse();
                    suspended = true;
                    o.ShouldEqual(Presenter);
                    arg3.ShouldEqual(Metadata);
                    return ActionToken.FromDelegate((_, _) => suspended = false);
                }
            });

            Presenter.TryClose(this, default, Metadata);
            suspended.ShouldBeFalse();
            addedCallbacks.Count.ShouldEqual(2);
            addedCallbacks.ShouldContain((presenterResult1, NavigationCallbackType.Closing));
            addedCallbacks.ShouldContain((presenterResult2, NavigationCallbackType.Closing));
        }

        protected override IPresenter GetPresenter() => new Presenter(ComponentCollectionManager);

        protected override INavigationDispatcher GetNavigationDispatcher() => new NavigationDispatcher(ComponentCollectionManager);
    }
}