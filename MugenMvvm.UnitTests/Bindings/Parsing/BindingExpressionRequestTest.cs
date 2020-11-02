using System.Collections.Generic;
using System.Linq.Expressions;
using MugenMvvm.Bindings.Parsing;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing
{
    public class BindingExpressionRequestTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            var target = Expression.Constant("1");
            var source = Expression.Constant("2");
            var parameter = new KeyValuePair<string?, object>("", "");
            var memberManagerRequest = new BindingExpressionRequest(target, source, parameter.ToItemOrList());
            memberManagerRequest.Target.ShouldEqual(target);
            memberManagerRequest.Source.ShouldEqual(source);
            memberManagerRequest.Parameters.ShouldEqual(parameter);
        }

        #endregion
    }
}