using System.Collections.Generic;
using MugenMvvm.Binding.Core.Components;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTest.Binding.Core.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Core.Components
{
    public class BuilderListBindingExpressionBuilderTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ShouldDispatchInputBuilders()
        {
            var bindingExpressions = new List<IBindingExpression> {new TestBindingExpression(), new TestBindingExpression()};
            var builder = new BuilderListBindingExpressionBuilder();
            builder.TryBuildBindingExpression(bindingExpressions, DefaultMetadata).List.ShouldEqual(bindingExpressions);

            builder.TryBuildBindingExpression(this, DefaultMetadata).IsNullOrEmpty().ShouldBeTrue();
        }

        #endregion
    }
}