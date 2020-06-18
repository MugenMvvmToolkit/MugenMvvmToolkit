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
            request.ToConverterRequest().IsEmpty.ShouldBeTrue();
        }

        [Fact]
        public void GetShouldInitializeValues()
        {
            var converterRequest = new ExpressionConverterRequest("", "", default);
            BindingBuilderDelegate<string, string> d = target => new ExpressionConverterRequest("", "", default);
            var request = BindingBuilderRequest.Get(d);
            request.IsEmpty.ShouldBeFalse();
            request.OriginalDelegate.ShouldEqual(d);
            request.ToConverterRequest().ShouldEqual(converterRequest);
        }

        [Fact]
        public void ToConverterRequestShouldThrowClosure()
        {
            var request = BindingBuilderRequest.Get<object, object>(target =>
            {
                ToString();
                return target.Action(context => context);
            });
            ShouldThrow<InvalidOperationException>(() => request.ToConverterRequest());
        }

        #endregion
    }
}