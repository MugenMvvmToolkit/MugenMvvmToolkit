﻿using System.Linq;
using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Core.Components;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Tests.Bindings.Core;
using MugenMvvm.Tests.Bindings.Observation;
using MugenMvvm.UnitTests.Bindings.Compiling.Internal;
using MugenMvvm.UnitTests.Components.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Core.Components
{
    [Collection(SharedContext)]
    public class BindingCleanerTest : UnitTestBase
    {
        public BindingCleanerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            RegisterDisposeToken(WithGlobalService(BindingManager));
            BindingManager.AddComponent(new BindingCleaner());
        }

        [Fact]
        public void ShouldClearExpressionBinding()
        {
            var targetDisposed = false;
            var sourceDisposed = false;
            IMemberPathObserverListener? targetListener = null;
            IMemberPathObserverListener? sourceListener = null;
            var expression = new TestCompiledExpression();
            var target = new TestMemberPathObserver
            {
                Dispose = () => targetDisposed = true,
                AddListener = l =>
                {
                    targetListener.ShouldBeNull();
                    targetListener = l;
                },
                RemoveListener = l =>
                {
                    targetListener.ShouldEqual(l);
                    targetListener = null;
                }
            };
            var source = new TestMemberPathObserver
            {
                Dispose = () => sourceDisposed = true,
                AddListener = l =>
                {
                    sourceListener.ShouldBeNull();
                    sourceListener = l;
                },
                RemoveListener = l =>
                {
                    sourceListener.ShouldEqual(l);
                    sourceListener = null;
                }
            };

            var components = new IComponent<IBinding>[] { new TestBindingTargetObserverListener(), new TestBindingSourceObserverListener() };
            var binding = new ExpressionBinding(target, new ItemOrArray<object?>(source, true), expression);
            binding.State.ShouldEqual(BindingState.Valid);
            binding.Initialize(components, DefaultMetadata);
            targetListener.ShouldEqual(binding);
            sourceListener.ShouldEqual(binding);

            var disposeCount = 0;
            BindingManager.AddComponent(new TestBindingLifecycleListener
            {
                OnLifecycleChanged = (_, b, state, _, m) =>
                {
                    ++disposeCount;
                    b.ShouldEqual(binding);
                    state.ShouldEqual(BindingLifecycleState.Disposed);
                    m.ShouldBeNull();
                }
            });

            binding.Dispose();
            binding.State.ShouldEqual(BindingState.Disposed);
            targetDisposed.ShouldBeTrue();
            sourceDisposed.ShouldBeTrue();
            binding.GetComponents<object>().AsList().ShouldBeEmpty();
            targetListener.ShouldBeNull();
            sourceListener.ShouldBeNull();
            disposeCount.ShouldEqual(1);

            binding.TryAddComponent(components[0]).IsEmpty.ShouldBeTrue();
            binding.GetComponents<object>().AsList().ShouldBeEmpty();
        }

        protected override IBindingManager GetBindingManager() => new BindingManager(ComponentCollectionManager);

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ShouldClearBinding(int count)
        {
            var targetDisposed = false;
            var sourceDisposed = false;
            IMemberPathObserverListener? targetListener = null;
            IMemberPathObserverListener? sourceListener = null;
            var target = new TestMemberPathObserver
            {
                Dispose = () => targetDisposed = true,
                AddListener = l =>
                {
                    targetListener.ShouldBeNull();
                    targetListener = l;
                },
                RemoveListener = l =>
                {
                    targetListener.ShouldEqual(l);
                    targetListener = null;
                }
            };
            var source = new TestMemberPathObserver
            {
                Dispose = () => sourceDisposed = true,
                AddListener = l =>
                {
                    sourceListener.ShouldBeNull();
                    sourceListener = l;
                },
                RemoveListener = l =>
                {
                    sourceListener.ShouldEqual(l);
                    sourceListener = null;
                }
            };

            var disposeComponentCount = 0;
            var components = Enumerable
                             .Range(0, count)
                             .Select(i => new TestComponent<IBinding> { Dispose = () => ++disposeComponentCount })
                             .Concat(new IComponent<IBinding>[] { new TestBindingTargetObserverListener(), new TestBindingSourceObserverListener() })
                             .ToArray();

            var binding = new Binding(target, source);
            binding.State.ShouldEqual(BindingState.Valid);
            binding.Initialize(components, DefaultMetadata);
            targetListener.ShouldEqual(binding);
            sourceListener.ShouldEqual(binding);

            var disposeCount = 0;
            BindingManager.AddComponent(new TestBindingLifecycleListener
            {
                OnLifecycleChanged = (_, b, state, _, m) =>
                {
                    ++disposeCount;
                    b.ShouldEqual(binding);
                    state.ShouldEqual(BindingLifecycleState.Disposed);
                    m.ShouldBeNull();
                }
            });

            binding.Dispose();
            disposeComponentCount.ShouldEqual(count);
            binding.State.ShouldEqual(BindingState.Disposed);
            targetDisposed.ShouldBeTrue();
            sourceDisposed.ShouldBeTrue();
            binding.GetComponents<object>().AsList().ShouldBeEmpty();
            targetListener.ShouldBeNull();
            sourceListener.ShouldBeNull();
            disposeCount.ShouldEqual(1);

            binding.TryAddComponent(components[0]).IsEmpty.ShouldBeTrue();
            binding.GetComponents<object>().AsList().ShouldBeEmpty();
        }
    }
}