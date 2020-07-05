using System.Collections.Generic;
using MugenMvvm.Binding.Core.Components;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTest.Binding.Core.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Core.Components
{
    public class BindingBuilderListExpressionParserTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ShouldDispatchInputBuilders()
        {
            var bindingExpressions = new List<IBindingBuilder> { new TestBindingBuilder(), new TestBindingBuilder() };
            var builder = new BindingBuilderListExpressionParser();
            builder.TryParseBindingExpression(null!, bindingExpressions, DefaultMetadata).List.ShouldEqual(bindingExpressions);

            builder.TryParseBindingExpression(null!, this, DefaultMetadata).IsNullOrEmpty().ShouldBeTrue();
        }

        #endregion
    }
}