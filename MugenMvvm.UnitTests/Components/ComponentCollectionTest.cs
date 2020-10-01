using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.Threading.Components;
using MugenMvvm.Threading;
using MugenMvvm.UnitTests.Components.Internal;
using MugenMvvm.UnitTests.Threading.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Components
{
    public class ComponentCollectionTest : ComponentOwnerTestBase<IComponentCollection>
    {
        #region Methods

        public override void ComponentOwnerShouldUseCollectionFactory(bool globalValue)
        {
            if (globalValue)
                base.ComponentOwnerShouldUseCollectionFactory(globalValue);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void AddShouldAddOrderedComponent(int count)
        {
            var componentCollection = new ComponentCollection(this);
            var components = new List<TestComponentCollectionProviderComponent>();
            for (var i = 0; i < count; i++)
            {
                var component = new TestComponentCollectionProviderComponent {Priority = i};
                components.Insert(0, component);
                componentCollection.Add(component, DefaultMetadata).ShouldBeTrue();
            }

            componentCollection.Count.ShouldEqual(components.Count);
            componentCollection.Owner.ShouldEqual(this);
            componentCollection.Get<TestComponentCollectionProviderComponent>().ShouldEqual(components);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void RemoveShouldRemoveComponent(int count)
        {
            var componentCollection = new ComponentCollection(this);
            var components = new List<TestComponentCollectionProviderComponent>();
            for (var i = 0; i < count; i++)
            {
                var component = new TestComponentCollectionProviderComponent {Priority = i};
                components.Insert(0, component);
                componentCollection.Add(component, DefaultMetadata);
            }

            for (var index = 0; index < components.Count; index++)
            {
                var component = components[index];
                components.RemoveAt(index--);
                componentCollection.Remove(component).ShouldBeTrue();

                componentCollection.Count.ShouldEqual(components.Count);
                componentCollection.Get<TestComponentCollectionProviderComponent>().ShouldEqual(components);
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ClearShouldClearComponents(int count)
        {
            var componentCollection = new ComponentCollection(this);
            var components = new List<TestComponentCollectionProviderComponent>();
            for (var i = 0; i < count; i++)
            {
                var component = new TestComponentCollectionProviderComponent {Priority = i};
                components.Insert(0, component);
                componentCollection.Add(component, DefaultMetadata);
            }

            componentCollection.Count.ShouldEqual(components.Count);
            componentCollection.Clear(DefaultMetadata);
            componentCollection.Count.ShouldEqual(0);
            componentCollection.Get<TestComponentCollectionProviderComponent>().Length.ShouldEqual(0);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void AddShouldNotifyListeners(int count)
        {
            var addingCount = 0;
            var addedCount = 0;
            var canAdd = false;
            object? expectedItem = null;
            var componentCollection = new ComponentCollection(this);
            var changingListener = new TestComponentCollectionChangingListener
            {
                OnAdding = (collection, o, arg3) =>
                {
                    addingCount++;
                    expectedItem.ShouldEqual(o);
                    arg3.ShouldEqual(DefaultMetadata);
                    return canAdd;
                }
            };
            componentCollection.AddComponent(changingListener);
            var changedListener = new TestComponentCollectionChangedListener
            {
                OnAdded = (collection, o, arg3) =>
                {
                    addedCount++;
                    expectedItem.ShouldEqual(o);
                    arg3.ShouldEqual(DefaultMetadata);
                }
            };
            componentCollection.AddComponent(changedListener);

            for (var i = 0; i < count; i++)
            {
                expectedItem = new object();
                componentCollection.Add(expectedItem, DefaultMetadata).ShouldBeFalse();
            }

            addingCount.ShouldEqual(count);
            addedCount.ShouldEqual(0);
            componentCollection.Count.ShouldEqual(0);
            componentCollection.Get<object>().Length.ShouldEqual(0);

            canAdd = true;
            addingCount = 0;
            for (var i = 0; i < count; i++)
            {
                expectedItem = new object();
                componentCollection.Add(expectedItem, DefaultMetadata).ShouldBeTrue();
            }

            addingCount.ShouldEqual(count);
            addedCount.ShouldEqual(count);
            componentCollection.Count.ShouldEqual(count);
            componentCollection.Get<object>().Length.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void RemoveShouldNotifyListeners(int count)
        {
            var removingCount = 0;
            var removedCount = 0;
            var canRemove = false;
            object? expectedItem = null;
            var componentCollection = new ComponentCollection(this);
            var changingListener = new TestComponentCollectionChangingListener
            {
                OnRemoving = (collection, o, arg3) =>
                {
                    removingCount++;
                    expectedItem.ShouldEqual(o);
                    arg3.ShouldEqual(DefaultMetadata);
                    return canRemove;
                }
            };
            componentCollection.AddComponent(changingListener);
            var changedListener = new TestComponentCollectionChangedListener
            {
                OnRemoved = (collection, o, arg3) =>
                {
                    removedCount++;
                    expectedItem.ShouldEqual(o);
                    arg3.ShouldEqual(DefaultMetadata);
                }
            };
            componentCollection.AddComponent(changedListener);

            for (var i = 0; i < count; i++)
                componentCollection.Add(new object(), DefaultMetadata);

            var objects = componentCollection.Get<object>();
            foreach (var o in objects)
            {
                expectedItem = o;
                componentCollection.Remove(expectedItem, DefaultMetadata).ShouldBeFalse();
            }

            removingCount.ShouldEqual(count);
            removedCount.ShouldEqual(0);
            componentCollection.Count.ShouldEqual(count);
            componentCollection.Get<object>().Length.ShouldEqual(count);

            canRemove = true;
            removingCount = 0;
            foreach (var o in objects)
            {
                expectedItem = o;
                componentCollection.Remove(expectedItem, DefaultMetadata).ShouldBeTrue();
            }

            removingCount.ShouldEqual(count);
            removedCount.ShouldEqual(count);
            componentCollection.Count.ShouldEqual(0);
            componentCollection.Get<object>().Length.ShouldEqual(0);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ClearShouldNotifyListeners(int count)
        {
            var items = new HashSet<object>();
            var removedCount = 0;
            var componentCollection = new ComponentCollection(this);
            var changedListener = new TestComponentCollectionChangedListener
            {
                OnRemoved = (collection, o, arg3) =>
                {
                    removedCount++;
                    items.Remove(o).ShouldBeTrue();
                    arg3.ShouldEqual(DefaultMetadata);
                }
            };
            componentCollection.AddComponent(changedListener);

            for (var i = 0; i < count; i++)
            {
                var o = new object();
                items.Add(o);
                componentCollection.Add(o, DefaultMetadata);
            }

            componentCollection.Clear(DefaultMetadata);
            removedCount.ShouldEqual(count);
            items.Count.ShouldEqual(0);
        }

        [Fact]
        public void AddShouldCallOnAttachingOnAttachedMethods()
        {
            var attachingCount = 0;
            var attachedCount = 0;
            var canAttach = false;
            var componentCollection = new ComponentCollection(this);
            var component = new TestAttachableComponent<ComponentCollectionTest>
            {
                OnAttachingHandler = (test, context) =>
                {
                    attachingCount++;
                    test.ShouldEqual(this);
                    return canAttach;
                },
                OnAttachedHandler = (test, context) =>
                {
                    attachedCount++;
                    test.ShouldEqual(this);
                    context.ShouldEqual(DefaultMetadata);
                }
            };

            componentCollection.Add(component, DefaultMetadata).ShouldBeFalse();
            attachingCount.ShouldEqual(1);
            attachedCount.ShouldEqual(0);
            componentCollection.Count.ShouldEqual(0);

            canAttach = true;
            componentCollection.Add(component, DefaultMetadata).ShouldBeTrue();
            attachingCount.ShouldEqual(2);
            attachedCount.ShouldEqual(1);
            componentCollection.Get<object>().Single().ShouldEqual(component);
        }

        [Fact]
        public void RemoveShouldCallOnDetachingOnDetachedMethods()
        {
            var detachingCount = 0;
            var detachedCount = 0;
            var canDetach = false;
            var componentCollection = new ComponentCollection(this);
            var component = new TestAttachableComponent<ComponentCollectionTest>
            {
                OnDetachingHandler = (test, context) =>
                {
                    detachingCount++;
                    test.ShouldEqual(this);
                    return canDetach;
                },
                OnDetachedHandler = (test, context) =>
                {
                    detachedCount++;
                    test.ShouldEqual(this);
                    context.ShouldEqual(DefaultMetadata);
                }
            };
            componentCollection.Add(component, DefaultMetadata);

            componentCollection.Remove(component, DefaultMetadata).ShouldBeFalse();
            detachingCount.ShouldEqual(1);
            detachedCount.ShouldEqual(0);
            componentCollection.Count.ShouldEqual(1);
            componentCollection.Get<object>().Single().ShouldEqual(component);

            canDetach = true;
            componentCollection.Remove(component, DefaultMetadata).ShouldBeTrue();
            detachingCount.ShouldEqual(2);
            detachedCount.ShouldEqual(1);
            componentCollection.Get<object>().Length.ShouldEqual(0);
        }

        [Fact]
        public void ClearShouldCallOnDetachedMethods()
        {
            var detachedCount = 0;
            var componentCollection = new ComponentCollection(this);
            var component = new TestAttachableComponent<ComponentCollectionTest>
            {
                OnDetachedHandler = (test, context) =>
                {
                    detachedCount++;
                    test.ShouldEqual(this);
                    context.ShouldEqual(DefaultMetadata);
                }
            };
            componentCollection.Add(component, DefaultMetadata);
            componentCollection.Clear(DefaultMetadata);
            detachedCount.ShouldEqual(1);
        }

        [Fact]
        public void GetShouldDecorateItems()
        {
            var executed = 0;
            var threadDispatcher = new ThreadDispatcher();
            var componentCollection = new ComponentCollection(threadDispatcher);

            var componentDecorated1 = new TestThreadDispatcherComponent();
            var componentDecorated2 = new TestThreadDispatcherComponent();

            var decoratorComponent1 = new TestComponentDecorator<IThreadDispatcher, IThreadDispatcherComponent> {Priority = 0};
            var decoratorComponent2 = new TestComponentDecorator<IThreadDispatcher, IThreadDispatcherComponent> {Priority = 1};
            var component = new TestThreadDispatcherComponent();
            componentCollection.Add(component);

            componentCollection.Get<IThreadDispatcherComponent>(DefaultMetadata).ShouldEqual(new[] {component});
            decoratorComponent1.DecorateHandler = (c, list, context) =>
            {
                ++executed;
                c.ShouldEqual(componentCollection);
                list.ShouldEqual(new[] {component});
                context.ShouldEqual(DefaultMetadata);
                list.Add(componentDecorated1);
            };
            componentCollection.AddComponent(decoratorComponent1);

            componentCollection.Get<IThreadDispatcherComponent>(DefaultMetadata).ShouldEqual(new[] {component, componentDecorated1});
            executed.ShouldEqual(1);

            decoratorComponent2.DecorateHandler = (c, list, context) =>
            {
                ++executed;
                c.ShouldEqual(componentCollection);
                list.ShouldEqual(new[] {component, componentDecorated1});
                context.ShouldEqual(DefaultMetadata);
                list.Add(componentDecorated2);
            };
            componentCollection.AddComponent(decoratorComponent2);

            executed = 0;
            componentCollection.Get<IThreadDispatcherComponent>(DefaultMetadata).ShouldEqual(new[] {component, componentDecorated1, componentDecorated2});
            executed.ShouldEqual(2);

            componentCollection.RemoveComponent(decoratorComponent2);
            executed = 0;
            componentCollection.Get<IThreadDispatcherComponent>(DefaultMetadata).ShouldEqual(new[] {component, componentDecorated1});
            executed.ShouldEqual(1);

            executed = 0;
            componentCollection.RemoveComponent(decoratorComponent1);
            var components = componentCollection.Get<IThreadDispatcherComponent>(DefaultMetadata);
            components.ShouldEqual(new[] {component});
            executed.ShouldEqual(0);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ShouldUseCorrectOrderForDecorators(int count)
        {
            var threadDispatcher = new ThreadDispatcher();
            var componentCollection = new ComponentCollection(threadDispatcher);

            var executed = 0;
            for (var i = 0; i < count; i++)
            {
                var decoratorComponent = new TestThreadDispatcherDecorator();
                decoratorComponent.DecorateHandler = (c, list, context) =>
                {
                    ++executed;
                    c.ShouldEqual(componentCollection);
                    list.Add(new TestThreadDispatcherComponent());
                };
                componentCollection.Add(decoratorComponent);
                componentCollection.AddComponent(decoratorComponent);
            }

            var components = componentCollection.Get<IThreadDispatcherComponent>();
            executed.ShouldEqual(count);
            components.Length.ShouldEqual(count * 2);
            components.OfType<TestThreadDispatcherComponent>().Count().ShouldEqual(count);
        }

        protected override IComponentCollection GetComponentOwner(IComponentCollectionManager? collectionProvider = null) => new ComponentCollection(this);

        #endregion

        #region Nested types

        private sealed class TestThreadDispatcherDecorator : TestComponentDecorator<IThreadDispatcher, IThreadDispatcherComponent>, IThreadDispatcherComponent
        {
            #region Implementation of interfaces

            public bool CanExecuteInline(IThreadDispatcher threadDispatcher, ThreadExecutionMode executionMode, IReadOnlyMetadataContext? metadata) => throw new NotSupportedException();

            public bool TryExecute(IThreadDispatcher threadDispatcher, ThreadExecutionMode executionMode, object handler, object? state, IReadOnlyMetadataContext? metadata) => throw new NotSupportedException();

            #endregion
        }

        #endregion
    }
}