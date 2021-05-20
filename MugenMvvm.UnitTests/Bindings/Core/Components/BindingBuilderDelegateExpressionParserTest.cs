﻿using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Core.Components;
using MugenMvvm.Bindings.Delegates;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Parsing;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Bindings.Core.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Core.Components
{
    public class BindingBuilderDelegateExpressionParserTest : UnitTestBase
    {
        private static readonly BindingExpressionRequest ConverterRequest = new("", "", default);
        private static readonly BindingBuilderDelegate<object, object> Delegate = target => ConverterRequest;

        [Fact]
        public void TryParseBindingExpressionShouldCacheRequest()
        {
            var invokeCount = 0;
            var testExp = new TestBindingBuilder();

            var bindingManager = new BindingManager(ComponentCollectionManager);
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
            cache.Invalidate(this, this, DefaultMetadata);
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
    }
}