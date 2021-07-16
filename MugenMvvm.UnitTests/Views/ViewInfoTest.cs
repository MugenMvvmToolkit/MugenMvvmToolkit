using MugenMvvm.Interfaces.Views;
using MugenMvvm.Tests.ViewModels;
using MugenMvvm.Views;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Views
{
    public class ViewInfoTest : UnitTestBase
    {
        [Fact]
        public void RawViewShouldBeValid()
        {
            var viewInfo = new ViewInfo(this);
            viewInfo.RawView.ShouldEqual(this);
            viewInfo.SourceView.ShouldEqual(this);
            viewInfo.View.ShouldBeNull();
            viewInfo.Is<string>().ShouldBeFalse();
            viewInfo.Is<ViewInfoTest>().ShouldBeTrue();
            viewInfo.TryGet<ViewInfoTest>().ShouldEqual(this);
            viewInfo.TryGet<ViewInfoTest>(out var viewInfoTest).ShouldBeTrue();
            viewInfoTest.ShouldEqual(this);
            viewInfo.IsSameView(this).ShouldBeTrue();
            viewInfo.IsSameView(Metadata).ShouldBeFalse();
            viewInfo.Equals(new ViewInfo(this)).ShouldBeTrue();
        }


        [Fact]
        public void ViewShouldBeValid()
        {
            var rawView = new View(ViewMapping.Undefined, this, new TestViewModel());
            var viewInfo = new ViewInfo(rawView);
            viewInfo.RawView.ShouldEqual(rawView);
            viewInfo.SourceView.ShouldEqual(this);
            viewInfo.View.ShouldEqual(rawView);
            viewInfo.Is<string>().ShouldBeFalse();
            viewInfo.Is<IView>().ShouldBeTrue();
            viewInfo.Is<ViewInfoTest>().ShouldBeTrue();
            viewInfo.TryGet<ViewInfoTest>().ShouldEqual(this);
            viewInfo.TryGet<ViewInfoTest>(out var viewInfoTest).ShouldBeTrue();
            viewInfoTest.ShouldEqual(this);
            viewInfo.TryGet<IView>().ShouldEqual(rawView);
            viewInfo.TryGet<IView>(out var v).ShouldBeTrue();
            v.ShouldEqual(rawView);
            viewInfo.IsSameView(this).ShouldBeTrue();
            viewInfo.IsSameView(rawView).ShouldBeTrue();
            viewInfo.IsSameView(Metadata).ShouldBeFalse();
            viewInfo.Equals(new ViewInfo(this)).ShouldBeTrue();
            viewInfo.Equals(new ViewInfo(rawView)).ShouldBeTrue();
        }
    }
}