using System.Collections.Generic;
using System.Linq.Expressions;
using MugenMvvm.Binding.Parsing;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Parsing
{
    public class BindingExpressionRequestTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void DefaultShouldBeEmpty()
        {
            default(BindingExpressionRequest).IsEmpty.ShouldBeTrue();
        }

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            var target = Expression.Constant("1");
            var source = Expression.Constant("2");
            var parameter = new KeyValuePair<string?, object>("", "");
            var memberManagerRequest = new BindingExpressionRequest(target, source, parameter);
            memberManagerRequest.Target.ShouldEqual(target);
            memberManagerRequest.Source.ShouldEqual(source);
            memberManagerRequest.IsEmpty.ShouldBeFalse();
            memberManagerRequest.Parameters.ShouldEqual(parameter);
        }

        #endregion
    }
}