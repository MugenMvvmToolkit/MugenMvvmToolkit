using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Tests.Bindings.Core;
using MugenMvvm.UnitTests.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Core
{
    public class BindingManagerTest : ComponentOwnerTestBase<BindingManager>
    {
        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetBindingsShouldBeHandledByComponents(int count)
        {
            var target = this;
            var path = "t";
            var list1 = new List<IBinding>();
            var list2 = new List<IBinding>();
            for (var i = 0; i < count; i++)
            {
                var binding = new TestBinding();
                list1.Add(binding);
                BindingManager.AddComponent(new TestBindingHolderComponent
                {
                    Priority = -i,
                    TryGetBindings = (bm, t, p, m) =>
                    {
                        bm.ShouldEqual(BindingManager);
                        list2.Add(binding);
                        t.ShouldEqual(target);
                        p.ShouldEqual(path);
                        m.ShouldEqual(Metadata);
                        return binding;
                    }
                });
            }

            var result = BindingManager.GetBindings(target, path, Metadata);
            list1.ShouldEqual(result);
            list1.ShouldEqual(list2);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void OnLifecycleChangedShouldBeHandledByComponents(int count)
        {
            var invokeCount = 0;
            var state = "state";
            var lifecycleState = BindingLifecycleState.Disposed;
            for (var i = 0; i < count; i++)
            {
                BindingManager.AddComponent(new TestBindingLifecycleListener
                {
                    OnLifecycleChanged = (bm, vm, viewModelLifecycleState, st, metadata) =>
                    {
                        ++invokeCount;
                        bm.ShouldEqual(BindingManager);
                        vm.ShouldEqual(Binding);
                        st.ShouldEqual(state);
                        viewModelLifecycleState.ShouldEqual(lifecycleState);
                        metadata.ShouldEqual(Metadata);
                    },
                    Priority = i
                });
            }

            BindingManager.OnLifecycleChanged(Binding, lifecycleState, state, Metadata);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ParseBindingExpressionShouldBeHandledByComponents(int count)
        {
            var request = "t";
            var expression = new TestBindingBuilder();
            var invokeCount = 0;
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                BindingManager.AddComponent(new TestBindingExpressionParserComponent
                {
                    Priority = -i,
                    TryParseBindingExpression = (bm, r, m) =>
                    {
                        ++invokeCount;
                        bm.ShouldEqual(BindingManager);
                        r.ShouldEqual(request);
                        m.ShouldEqual(Metadata);
                        if (isLast)
                            return expression;
                        return default;
                    }
                });
            }

            var result = BindingManager.ParseBindingExpression(request, Metadata);
            result.Count.ShouldEqual(1);
            result.Item.ShouldEqual(expression);
            invokeCount.ShouldEqual(count);
        }

        [Fact]
        public void ParseBindingExpressionShouldThrowNoComponents() => ShouldThrow<InvalidOperationException>(() => BindingManager.ParseBindingExpression(this, Metadata));

        [Fact]
        public void TryParseBindingExpressionShouldHandleBuildersList()
        {
            var bindingExpressions = new List<IBindingBuilder> { new TestBindingBuilder(), new TestBindingBuilder() };
            BindingManager.TryParseBindingExpression(bindingExpressions, Metadata).List.ShouldEqual(bindingExpressions);
            BindingManager.TryParseBindingExpression(this, Metadata).IsEmpty.ShouldBeTrue();
        }

        protected override IBindingManager GetBindingManager() => GetComponentOwner(ComponentCollectionManager);

        protected override BindingManager GetComponentOwner(IComponentCollectionManager? componentCollectionManager = null) => new(componentCollectionManager);
    }
}