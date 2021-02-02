using System;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.ViewModels.Internal;
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
        private readonly WrapperManager _wrapperManager;

        public ViewWrapperManagerDecoratorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _wrapperManager = new WrapperManager(ComponentCollectionManager);
            _wrapperManager.AddComponent(new ViewWrapperManagerDecorator());
        }

        [Fact]
        public void CanWrapShouldHandleViewRequest1()
        {
            var wrapperType = typeof(string);
            var view = new object();
            var request = new View(new ViewMapping("id", typeof(TestViewModel), typeof(object), DefaultMetadata), view, new TestViewModel());
            var invokeCount = 0;

            _wrapperManager.AddComponent(new DelegateWrapperManager<object, object>((type, r, arg4) =>
            {
                ++invokeCount;
                type.ShouldEqual(wrapperType);
                r.ShouldEqual(view);
                arg4.ShouldEqual(DefaultMetadata);
                return false;
            }, (type, r, arg4) => throw new NotSupportedException()));

            _wrapperManager.CanWrap(wrapperType, request, DefaultMetadata).ShouldBeFalse();
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void CanWrapShouldHandleViewRequest2()
        {
            var wrapperType = typeof(object);
            var view = new object();
            var request = new View(new ViewMapping("id", typeof(TestViewModel), typeof(object), DefaultMetadata), view, new TestViewModel());
            _wrapperManager.CanWrap(wrapperType, request, DefaultMetadata).ShouldBeTrue();
        }

        [Fact]
        public void CanWrapShouldIgnoreNonViewRequest()
        {
            var wrapperType = typeof(string);
            var request = "";
            var invokeCount = 0;

            _wrapperManager.AddComponent(new DelegateWrapperManager<object, object>((type, r, arg4) =>
            {
                ++invokeCount;
                type.ShouldEqual(wrapperType);
                r.ShouldEqual(request);
                arg4.ShouldEqual(DefaultMetadata);
                return false;
            }, (type, r, arg4) => throw new NotSupportedException()));

            _wrapperManager.CanWrap(wrapperType, request, DefaultMetadata).ShouldBeFalse();
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void WrapShouldHandleViewRequest()
        {
            var wrapperType = typeof(ViewWrapperManagerDecoratorTest);
            var view = new object();
            var request = new View(new ViewMapping("id", typeof(TestViewModel), typeof(object), DefaultMetadata), view, new TestViewModel(), null, ComponentCollectionManager);
            var invokeCount = 0;

            _wrapperManager.AddComponent(new DelegateWrapperManager<object, object>((type, r, arg4) => throw new NotSupportedException(), (type, r, arg4) =>
            {
                ++invokeCount;
                type.ShouldEqual(wrapperType);
                r.ShouldEqual(view);
                arg4.ShouldEqual(DefaultMetadata);
                return this;
            }));

            _wrapperManager.Wrap(wrapperType, request, DefaultMetadata).ShouldEqual(this);
            invokeCount.ShouldEqual(1);

            _wrapperManager.Wrap(wrapperType, request, DefaultMetadata).ShouldEqual(this);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void WrapShouldIgnoreNonViewRequest()
        {
            var wrapperType = typeof(string);
            var request = "";
            var invokeCount = 0;

            _wrapperManager.AddComponent(new DelegateWrapperManager<object, object>((type, r, arg4) => throw new NotSupportedException(), (type, r, arg4) =>
            {
                ++invokeCount;
                type.ShouldEqual(wrapperType);
                r.ShouldEqual(request);
                arg4.ShouldEqual(DefaultMetadata);
                return this;
            }));

            _wrapperManager.Wrap(wrapperType, request, DefaultMetadata).ShouldEqual(this);
            invokeCount.ShouldEqual(1);
        }
    }
}