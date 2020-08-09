using System.Linq;
using MugenMvvm.Binding.Core;
using MugenMvvm.Binding.Core.Components;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.UnitTest.Binding.Compiling.Internal;
using MugenMvvm.UnitTest.Binding.Core.Internal;
using MugenMvvm.UnitTest.Binding.Observation.Internal;
using MugenMvvm.UnitTest.Components.Internal;
using MugenMvvm.UnitTest.Internal.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Core.Components
{
    public class BindingCleanerTest : UnitTestBase
    {
        #region Methods

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
                .Select(i => new TestComponent<IBinding> {Dispose = () => ++disposeComponentCount})
                .OfType<IComponent<IBinding>>()
                .Concat(new IComponent<IBinding>[] {new TestBindingTargetObserverListener(), new TestBindingSourceObserverListener()})
                .ToArray();

            var binding = new MugenMvvm.Binding.Core.Binding(target, source);
            binding.State.ShouldEqual(BindingState.Valid);
            binding.Initialize(components, DefaultMetadata);
            targetListener.ShouldEqual(binding);
            sourceListener.ShouldEqual(binding);

            var disposeCount = 0;
            var testLifecycleListener = new TestBindingStateDispatcherComponent
            {
                OnLifecycleChanged = (b, state, _, m) =>
                {
                    ++disposeCount;
                    b.ShouldEqual(binding);
                    state.ShouldEqual(BindingLifecycleState.Disposed);
                    m.ShouldBeNull();
                }
            };
            using var subscribe = TestComponentSubscriber.Subscribe(testLifecycleListener, new BindingCleaner());

            binding.Dispose();
            disposeComponentCount.ShouldEqual(count);
            binding.State.ShouldEqual(BindingState.Disposed);
            targetDisposed.ShouldBeTrue();
            sourceDisposed.ShouldBeTrue();
            binding.GetComponents().AsList().ShouldBeEmpty();
            targetListener.ShouldBeNull();
            sourceListener.ShouldBeNull();
            disposeCount.ShouldEqual(1);

            binding.AddComponent(components[0]).ShouldBeFalse();
            binding.GetComponents().AsList().ShouldBeEmpty();
        }

        [Fact]
        public void ShouldClearMultiBinding()
        {
            var targetDisposed = false;
            var sourceDisposed = false;
            var expressionDisposed = false;
            IMemberPathObserverListener? targetListener = null;
            IMemberPathObserverListener? sourceListener = null;
            var expression = new TestCompiledExpression {Dispose = () => expressionDisposed = true};
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

            var components = new IComponent<IBinding>[] {new TestBindingTargetObserverListener(), new TestBindingSourceObserverListener()};
            var binding = new MultiBinding(target, sources: source, expression: expression);
            binding.State.ShouldEqual(BindingState.Valid);
            binding.Initialize(components, DefaultMetadata);
            targetListener.ShouldEqual(binding);
            sourceListener.ShouldEqual(binding);

            var disposeCount = 0;
            var testLifecycleListener = new TestBindingStateDispatcherComponent
            {
                OnLifecycleChanged = (b, state, _, m) =>
                {
                    ++disposeCount;
                    b.ShouldEqual(binding);
                    state.ShouldEqual(BindingLifecycleState.Disposed);
                    m.ShouldBeNull();
                }
            };
            using var subscribe = TestComponentSubscriber.Subscribe(testLifecycleListener, new BindingCleaner());

            binding.Dispose();
            binding.State.ShouldEqual(BindingState.Disposed);
            targetDisposed.ShouldBeTrue();
            sourceDisposed.ShouldBeTrue();
            binding.GetComponents().AsList().ShouldBeEmpty();
            targetListener.ShouldBeNull();
            sourceListener.ShouldBeNull();
            disposeCount.ShouldEqual(1);
            expressionDisposed.ShouldBeFalse();

            binding.AddComponent(components[0]).ShouldBeFalse();
            binding.GetComponents().AsList().ShouldBeEmpty();
        }

        #endregion
    }
}