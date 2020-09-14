using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Binding.Core;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Binding.Core.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Binding.Core
{
    public class BindingManagerTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void BuildBindingExpressionShouldThrowNoComponents()
        {
            var bindingManager = new BindingManager();
            ShouldThrow<InvalidOperationException>(() => bindingManager.ParseBindingExpression(this, DefaultMetadata));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void BuildBindingExpressionShouldBeHandledByComponents(int count)
        {
            var request = "t";
            var bindingManager = new BindingManager();
            var expression = new TestBindingBuilder();
            var invokeCount = 0;
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                var component = new TestBindingExpressionParserComponent(bindingManager)
                {
                    Priority = -i,
                    TryParseBindingExpression = (r, m) =>
                    {
                        ++invokeCount;
                        r.ShouldEqual(request);
                        m.ShouldEqual(DefaultMetadata);
                        if (isLast)
                            return expression;
                        return default;
                    }
                };
                bindingManager.AddComponent(component);
            }

            var result = bindingManager.ParseBindingExpression(request, DefaultMetadata);
            result.Iterator().Count.ShouldEqual(1);
            result.Item.ShouldEqual(expression);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetBindingsShouldBeHandledByComponents(int count)
        {
            var target = this;
            var path = "t";
            var bindingManager = new BindingManager();
            var list1 = new List<IBinding>();
            var list2 = new List<IBinding>();
            for (var i = 0; i < count; i++)
            {
                var binding = new TestBinding();
                list1.Add(binding);
                var component = new TestBindingHolderComponent(bindingManager)
                {
                    Priority = -i,
                    TryGetBindings = (t, p, m) =>
                    {
                        list2.Add(binding);
                        t.ShouldEqual(target);
                        p.ShouldEqual(path);
                        m.ShouldEqual(DefaultMetadata);
                        return binding;
                    }
                };
                bindingManager.AddComponent(component);
            }

            var result = bindingManager.GetBindings(target, path, DefaultMetadata);
            list1.SequenceEqual(result.AsList()).ShouldBeTrue();
            list1.SequenceEqual(list2).ShouldBeTrue();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void OnLifecycleChangedShouldBeHandledByComponents(int count)
        {
            var manager = new BindingManager();
            var invokeCount = 0;
            var state = "state";
            var binding = new TestBinding();
            var lifecycleState = BindingLifecycleState.Disposed;
            for (var i = 0; i < count; i++)
            {
                var component = new TestBindingStateDispatcherComponent(manager)
                {
                    OnLifecycleChanged = (vm, viewModelLifecycleState, st, metadata) =>
                    {
                        ++invokeCount;
                        vm.ShouldEqual(binding);
                        st.ShouldEqual(state);
                        viewModelLifecycleState.ShouldEqual(lifecycleState);
                        metadata.ShouldEqual(DefaultMetadata);
                    },
                    Priority = i
                };
                manager.AddComponent(component);
            }

            manager.OnLifecycleChanged(binding, lifecycleState, state, DefaultMetadata);
            invokeCount.ShouldEqual(count);
        }

        #endregion
    }
}