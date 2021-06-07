using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Core.Components;
using MugenMvvm.Bindings.Delegates;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Parsing;
using MugenMvvm.Extensions;
using MugenMvvm.Tests.Bindings.Core;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Core.Components
{
    public class BindingBuilderDelegateExpressionParserTest : UnitTestBase
    {
        private static readonly BindingExpressionRequest ConverterRequest = new("", "", default);
        private static readonly BindingBuilderDelegate<object, object> Delegate = _ => ConverterRequest;

        [Fact]
        public void TryParseBindingExpressionShouldCacheRequest()
        {
            var invokeCount = 0;
            var testExp = new TestBindingBuilder();


            var cache = new BindingBuilderDelegateExpressionParser();
            var component = new TestBindingExpressionParserComponent
            {
                TryParseBindingExpression = (_, o, arg3) =>
                {
                    ++invokeCount;
                    o.ShouldEqual(ConverterRequest);
                    arg3.ShouldEqual(DefaultMetadata);
                    return testExp;
                }
            };
            BindingManager.AddComponent(cache);
            BindingManager.AddComponent(component);

            BindingManager.ParseBindingExpression(Delegate, DefaultMetadata).Item.ShouldEqual(testExp);
            invokeCount.ShouldEqual(1);
            BindingManager.ParseBindingExpression(Delegate, DefaultMetadata).Item.ShouldEqual(testExp);
            invokeCount.ShouldEqual(1);

            //invalidate
            cache.Invalidate(this, this, DefaultMetadata);
            BindingManager.ParseBindingExpression(Delegate, DefaultMetadata).Item.ShouldEqual(testExp);
            BindingManager.ParseBindingExpression(Delegate, DefaultMetadata).Item.ShouldEqual(testExp);
            invokeCount.ShouldEqual(2);

            //add new component
            BindingManager.RemoveComponent(component);
            BindingManager.AddComponent(component);
            BindingManager.ParseBindingExpression(Delegate, DefaultMetadata).Item.ShouldEqual(testExp);
            BindingManager.ParseBindingExpression(Delegate, DefaultMetadata).Item.ShouldEqual(testExp);
            invokeCount.ShouldEqual(3);
        }

        protected override IBindingManager GetBindingManager() => new BindingManager(ComponentCollectionManager);
    }
}