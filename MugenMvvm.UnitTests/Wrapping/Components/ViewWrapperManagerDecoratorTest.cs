using System;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.ViewModels.Internal;
using MugenMvvm.Views;
using MugenMvvm.Wrapping;
using MugenMvvm.Wrapping.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Wrapping.Components
{
    public class ViewWrapperManagerDecoratorTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void CanWrapShouldIgnoreNonViewRequest()
        {
            var wrapperType = typeof(string);
            var request = "";
            var invokeCount = 0;

            var wrapperManager = new WrapperManager();
            wrapperManager.AddComponent(new ViewWrapperManagerDecorator());
            wrapperManager.AddComponent(new DelegateWrapperManager<object, object>((type, r, arg4) =>
            {
                ++invokeCount;
                type.ShouldEqual(wrapperType);
                r.ShouldEqual(request);
                arg4.ShouldEqual(DefaultMetadata);
                return false;
            }, (type, r, arg4) => throw new NotSupportedException()));

            wrapperManager.CanWrap(wrapperType, request, DefaultMetadata).ShouldBeFalse();
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void CanWrapShouldHandleViewRequest1()
        {
            var wrapperType = typeof(string);
            var view = new object();
            var request = new View(new ViewMapping("id", typeof(TestViewModel), typeof(object), DefaultMetadata), view, new TestViewModel());
            var invokeCount = 0;

            var wrapperManager = new WrapperManager();
            wrapperManager.AddComponent(new ViewWrapperManagerDecorator());
            wrapperManager.AddComponent(new DelegateWrapperManager<object, object>((type, r, arg4) =>
            {
                ++invokeCount;
                type.ShouldEqual(wrapperType);
                r.ShouldEqual(view);
                arg4.ShouldEqual(DefaultMetadata);
                return false;
            }, (type, r, arg4) => throw new NotSupportedException()));

            wrapperManager.CanWrap(wrapperType, request, DefaultMetadata).ShouldBeFalse();
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void CanWrapShouldHandleViewRequest2()
        {
            var wrapperType = typeof(object);
            var view = new object();
            var request = new View(new ViewMapping("id", typeof(TestViewModel), typeof(object), DefaultMetadata), view, new TestViewModel());

            var wrapperManager = new WrapperManager();
            wrapperManager.AddComponent(new ViewWrapperManagerDecorator());

            wrapperManager.CanWrap(wrapperType, request, DefaultMetadata).ShouldBeTrue();
        }

        [Fact]
        public void WrapShouldIgnoreNonViewRequest()
        {
            var wrapperType = typeof(string);
            var request = "";
            var invokeCount = 0;

            var wrapperManager = new WrapperManager();
            wrapperManager.AddComponent(new ViewWrapperManagerDecorator());
            wrapperManager.AddComponent(new DelegateWrapperManager<object, object>((type, r, arg4) => throw new NotSupportedException(), (type, r, arg4) =>
            {
                ++invokeCount;
                type.ShouldEqual(wrapperType);
                r.ShouldEqual(request);
                arg4.ShouldEqual(DefaultMetadata);
                return this;
            }));

            wrapperManager.Wrap(wrapperType, request, DefaultMetadata).ShouldEqual(this);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void WrapShouldHandleViewRequest()
        {
            var wrapperType = typeof(ViewWrapperManagerDecoratorTest);
            var view = new object();
            var request = new View(new ViewMapping("id", typeof(TestViewModel), typeof(object), DefaultMetadata), view, new TestViewModel());
            var invokeCount = 0;

            var wrapperManager = new WrapperManager();
            wrapperManager.AddComponent(new ViewWrapperManagerDecorator());
            wrapperManager.AddComponent(new DelegateWrapperManager<object, object>((type, r, arg4) => throw new NotSupportedException(), (type, r, arg4) =>
            {
                ++invokeCount;
                type.ShouldEqual(wrapperType);
                r.ShouldEqual(view);
                arg4.ShouldEqual(DefaultMetadata);
                return this;
            }));

            wrapperManager.Wrap(wrapperType, request, DefaultMetadata).ShouldEqual(this);
            invokeCount.ShouldEqual(1);

            wrapperManager.Wrap(wrapperType, request, DefaultMetadata).ShouldEqual(this);
            invokeCount.ShouldEqual(1);
        }

        #endregion
    }
}