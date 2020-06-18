using MugenMvvm.Binding.Build;
using MugenMvvm.Binding.Build.Components;
using MugenMvvm.Binding.Core;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Parsing;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTest.Binding.Core.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Build.Components
{
    public class BindingBuilderRequestExpressionParserTest : UnitTestBase
    {
        #region Fields

        private static readonly ExpressionConverterRequest ConverterRequest = new ExpressionConverterRequest("", "", default);

        #endregion

        #region Methods

        [Fact]
        public void TryParseBindingExpressionShouldCacheRequest()
        {
            var invokeCount = 0;
            var request = BindingBuilderRequest.Get<object, object>(target => ConverterRequest);
            var testExp = new TestBindingBuilder();

            var bindingManager = new BindingManager();
            var cache = new BindingBuilderRequestExpressionParser();
            var component = new TestBindingExpressionParserComponent
            {
                TryParseBindingExpression = (o, type, arg3) =>
                {
                    ++invokeCount;
                    o.ShouldEqual(ConverterRequest);
                    type.ShouldEqual(ConverterRequest.GetType());
                    arg3.ShouldEqual(DefaultMetadata);
                    return testExp;
                }
            };
            bindingManager.AddComponent(cache);
            bindingManager.AddComponent(component);

            bindingManager.ParseBindingExpression(request, DefaultMetadata).Item.ShouldEqual(testExp);
            invokeCount.ShouldEqual(1);
            bindingManager.ParseBindingExpression(request, DefaultMetadata).Item.ShouldEqual(testExp);
            invokeCount.ShouldEqual(1);

            //invalidate
            cache.Invalidate(this, DefaultMetadata);
            bindingManager.ParseBindingExpression(request, DefaultMetadata).Item.ShouldEqual(testExp);
            bindingManager.ParseBindingExpression(request, DefaultMetadata).Item.ShouldEqual(testExp);
            invokeCount.ShouldEqual(2);

            //add new component
            bindingManager.RemoveComponent(component);
            bindingManager.AddComponent(component);
            bindingManager.ParseBindingExpression(request, DefaultMetadata).Item.ShouldEqual(testExp);
            bindingManager.ParseBindingExpression(request, DefaultMetadata).Item.ShouldEqual(testExp);
            invokeCount.ShouldEqual(3);
        }

        #endregion
    }
}