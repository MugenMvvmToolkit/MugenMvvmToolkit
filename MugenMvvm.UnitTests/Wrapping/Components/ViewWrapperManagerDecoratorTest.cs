using System;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Tests.ViewModels;
using MugenMvvm.Views;
using MugenMvvm.Wrapping;
using MugenMvvm.Wrapping.Components;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Wrapping.Components
{
    public class ViewWrapperManagerDecoratorTest : UnitTestBase
    {
        public ViewWrapperManagerDecoratorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            WrapperManager.AddComponent(new ViewWrapperManagerDecorator());
        }

        [Fact]
        public void CanWrapShouldHandleViewRequest1()
        {
            var wrapperType = typeof(string);
            var view = new object();
            var request = new View(new ViewMapping("id", typeof(TestViewModel), typeof(object), Metadata), view, new TestViewModel());
            var invokeCount = 0;

            WrapperManager.AddComponent(new DelegateWrapperManager<object, object>((type, r, arg4) =>
            {
                ++invokeCount;
                type.ShouldEqual(wrapperType);
                r.ShouldEqual(view);
                arg4.ShouldEqual(Metadata);
                return false;
            }, (type, r, arg4) => throw new NotSupportedException()));

            WrapperManager.CanWrap(wrapperType, request, Metadata).ShouldBeFalse();
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void CanWrapShouldHandleViewRequest2()
        {
            var wrapperType = typeof(object);
            var view = new object();
            var request = new View(new ViewMapping("id", typeof(TestViewModel), typeof(object), Metadata), view, new TestViewModel());
            WrapperManager.CanWrap(wrapperType, request, Metadata).ShouldBeTrue();
        }

        [Fact]
        public void CanWrapShouldIgnoreNonViewRequest()
        {
            var wrapperType = typeof(string);
            var request = "";
            var invokeCount = 0;

            WrapperManager.AddComponent(new DelegateWrapperManager<object, object>((type, r, arg4) =>
            {
                ++invokeCount;
                type.ShouldEqual(wrapperType);
                r.ShouldEqual(request);
                arg4.ShouldEqual(Metadata);
                return false;
            }, (type, r, arg4) => throw new NotSupportedException()));

            WrapperManager.CanWrap(wrapperType, request, Metadata).ShouldBeFalse();
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void WrapShouldHandleViewRequest()
        {
            var wrapperType = typeof(ViewWrapperManagerDecoratorTest);
            var view = new object();
            var request = new View(new ViewMapping("id", typeof(TestViewModel), typeof(object), Metadata), view, new TestViewModel(), null, ComponentCollectionManager);
            var invokeCount = 0;

            WrapperManager.AddComponent(new DelegateWrapperManager<object, object>((type, r, arg4) => throw new NotSupportedException(), (type, r, arg4) =>
            {
                ++invokeCount;
                type.ShouldEqual(wrapperType);
                r.ShouldEqual(view);
                arg4.ShouldEqual(Metadata);
                return this;
            }));

            WrapperManager.Wrap(wrapperType, request, Metadata).ShouldEqual(this);
            invokeCount.ShouldEqual(1);

            WrapperManager.Wrap(wrapperType, request, Metadata).ShouldEqual(this);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void WrapShouldIgnoreNonViewRequest()
        {
            var wrapperType = typeof(string);
            var request = "";
            var invokeCount = 0;

            WrapperManager.AddComponent(new DelegateWrapperManager<object, object>((type, r, arg4) => throw new NotSupportedException(), (type, r, arg4) =>
            {
                ++invokeCount;
                type.ShouldEqual(wrapperType);
                r.ShouldEqual(request);
                arg4.ShouldEqual(Metadata);
                return this;
            }));

            WrapperManager.Wrap(wrapperType, request, Metadata).ShouldEqual(this);
            invokeCount.ShouldEqual(1);
        }

        protected override IWrapperManager GetWrapperManager() => new WrapperManager(ComponentCollectionManager);
    }
}