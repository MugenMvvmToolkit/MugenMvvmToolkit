using System;
using MugenMvvm.Binding.Build;
using MugenMvvm.Binding.Delegates;
using MugenMvvm.Binding.Parsing;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Build
{
    public class BindingBuilderRequestTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void DefaultShouldReturnDefaultValues()
        {
            BindingBuilderRequest request = default;
            request.IsEmpty.ShouldBeTrue();
            request.ToBindingExpressionRequest().IsEmpty.ShouldBeTrue();
        }

        [Fact]
        public void GetShouldInitializeValues()
        {
            var converterRequest = new BindingExpressionRequest("", "", default);
            BindingBuilderDelegate<string, string> d = target => new BindingExpressionRequest("", "", default);
            var request = BindingBuilderRequest.Get(d);
            request.IsEmpty.ShouldBeFalse();
            request.OriginalDelegate.ShouldEqual(d);
            request.ToBindingExpressionRequest().ShouldEqual(converterRequest);
        }

        [Fact]
        public void ToBindingExpressionRequestShouldThrowClosure()
        {
            var request = BindingBuilderRequest.Get<object, object>(target =>
            {
                ToString();
                return target.Action(context => context);
            });
            ShouldThrow<InvalidOperationException>(() => request.ToBindingExpressionRequest());
        }

        #endregion
    }
}