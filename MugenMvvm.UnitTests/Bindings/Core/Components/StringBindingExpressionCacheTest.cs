using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Core.Components;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Extensions;
using MugenMvvm.Tests.Bindings.Core;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Core.Components
{
    public class StringBindingExpressionCacheTest : UnitTestBase
    {
        [Fact]
        public void TryParseBindingExpressionShouldCacheRequest()
        {
            var invokeCount = 0;
            var request = "t";
            var testExp = new TestBindingBuilder();

            var cache = new StringBindingExpressionCache();
            var component = new TestBindingExpressionParserComponent
            {
                TryParseBindingExpression = (m, o, arg3) =>
                {
                    ++invokeCount;
                    m.ShouldEqual(BindingManager);
                    o.ShouldEqual(request);
                    arg3.ShouldEqual(DefaultMetadata);
                    return testExp;
                }
            };
            BindingManager.AddComponent(cache);
            BindingManager.AddComponent(component);

            BindingManager.ParseBindingExpression(request, DefaultMetadata).Item.ShouldEqual(testExp);
            invokeCount.ShouldEqual(1);
            BindingManager.ParseBindingExpression(request, DefaultMetadata).Item.ShouldEqual(testExp);
            invokeCount.ShouldEqual(1);

            //invalidate
            cache.Invalidate(this, this, DefaultMetadata);
            BindingManager.ParseBindingExpression(request, DefaultMetadata).Item.ShouldEqual(testExp);
            BindingManager.ParseBindingExpression(request, DefaultMetadata).Item.ShouldEqual(testExp);
            invokeCount.ShouldEqual(2);

            //add new component
            BindingManager.RemoveComponent(component);
            BindingManager.AddComponent(component);
            BindingManager.ParseBindingExpression(request, DefaultMetadata).Item.ShouldEqual(testExp);
            BindingManager.ParseBindingExpression(request, DefaultMetadata).Item.ShouldEqual(testExp);
            invokeCount.ShouldEqual(3);
        }

        protected override IBindingManager GetBindingManager() => new BindingManager(ComponentCollectionManager);
    }
}