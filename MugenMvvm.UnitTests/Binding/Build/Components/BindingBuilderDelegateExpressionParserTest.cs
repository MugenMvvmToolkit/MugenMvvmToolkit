using MugenMvvm.Bindings.Build;
using MugenMvvm.Bindings.Build.Components;
using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Delegates;
using MugenMvvm.Bindings.Parsing;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Bindings.Core.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Build.Components
{
    public class BindingBuilderDelegateExpressionParserTest : UnitTestBase
    {
        #region Fields

        private static readonly BindingExpressionRequest ConverterRequest = new BindingExpressionRequest("", "", default);
        private static readonly BindingBuilderDelegate<object, object> Delegate = target => ConverterRequest;

        #endregion

        #region Methods

        [Fact]
        public void TryParseBindingExpressionShouldCacheRequest()
        {
            var invokeCount = 0;
            var testExp = new TestBindingBuilder();

            var bindingManager = new BindingManager();
            var cache = new BindingBuilderDelegateExpressionParser();
            var component = new TestBindingExpressionParserComponent
            {
                TryParseBindingExpression = (o, arg3) =>
                {
                    ++invokeCount;
                    o.ShouldEqual(ConverterRequest);
                    arg3.ShouldEqual(DefaultMetadata);
                    return testExp;
                }
            };
            bindingManager.AddComponent(cache);
            bindingManager.AddComponent(component);

            bindingManager.ParseBindingExpression(Delegate, DefaultMetadata).Item.ShouldEqual(testExp);
            invokeCount.ShouldEqual(1);
            bindingManager.ParseBindingExpression(Delegate, DefaultMetadata).Item.ShouldEqual(testExp);
            invokeCount.ShouldEqual(1);

            //invalidate
            cache.Invalidate(this, DefaultMetadata);
            bindingManager.ParseBindingExpression(Delegate, DefaultMetadata).Item.ShouldEqual(testExp);
            bindingManager.ParseBindingExpression(Delegate, DefaultMetadata).Item.ShouldEqual(testExp);
            invokeCount.ShouldEqual(2);

            //add new component
            bindingManager.RemoveComponent(component);
            bindingManager.AddComponent(component);
            bindingManager.ParseBindingExpression(Delegate, DefaultMetadata).Item.ShouldEqual(testExp);
            bindingManager.ParseBindingExpression(Delegate, DefaultMetadata).Item.ShouldEqual(testExp);
            invokeCount.ShouldEqual(3);
        }

        #endregion
    }
}