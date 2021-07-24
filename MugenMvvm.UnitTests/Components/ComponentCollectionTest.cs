using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.Threading.Components;
using MugenMvvm.Tests.Components;
using MugenMvvm.Tests.Threading;
using MugenMvvm.UnitTests.Components.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Components
{
    public class ComponentCollectionTest : ComponentOwnerTestBase<IComponentCollection>
    {
        private readonly ComponentCollection _componentCollection;

        public ComponentCollectionTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _componentCollection = new ComponentCollection(this, ComponentCollectionManager);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void AddShouldAddOrderedComponent(int count)
        {
            var components = new List<TestComponentCollectionProviderComponent>();
            for (var i = 0; i < count; i++)
            {
                var component = new TestComponentCollectionProviderComponent {Priority = i};
                components.Insert(0, component);
                _componentCollection.TryAdd(component, Metadata).ShouldBeTrue();
            }

            _componentCollection.Count.ShouldEqual(components.Count);
            _componentCollection.Owner.ShouldEqual(this);
            _componentCollection.Get<TestComponentCollectionProviderComponent>().ShouldEqual(components);
        }

        [Fact]
        public void AddShouldCallOnAttachingOnAttachedMethods()
        {
            var attachingCount = 0;
            var attachedCount = 0;
            var canAttach = false;

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
                    context.ShouldEqual(Metadata);
                }
            };

            _componentCollection.TryAdd(component, Metadata).ShouldBeFalse();
            attachingCount.ShouldEqual(1);
            attachedCount.ShouldEqual(0);
            _componentCollection.Count.ShouldEqual(0);

            canAttach = true;
            _componentCollection.TryAdd(component, Metadata).ShouldBeTrue();
            attachingCount.ShouldEqual(2);
            attachedCount.ShouldEqual(1);
            _componentCollection.Get<object>().Single().ShouldEqual(component);
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

            var changingListener = new TestComponentCollectionChangingListener
            {
                OnAdding = (collection, o, arg3) =>
                {
                    addingCount++;
                    expectedItem.ShouldEqual(o);
                    arg3.ShouldEqual(Metadata);
                    return canAdd;
                }
            };
            _componentCollection.AddComponent(changingListener);
            var changedListener = new TestComponentCollectionChangedListener
            {
                OnAdded = (collection, o, arg3) =>
                {
                    addedCount++;
                    expectedItem.ShouldEqual(o);
                    arg3.ShouldEqual(Metadata);
                }
            };
            _componentCollection.AddComponent(changedListener);

            for (var i = 0; i < count; i++)
            {
                expectedItem = new object();
                _componentCollection.TryAdd(expectedItem, Metadata).ShouldBeFalse();
            }

            addingCount.ShouldEqual(count);
            addedCount.ShouldEqual(0);
            _componentCollection.Count.ShouldEqual(0);
            _componentCollection.Get<object>().Count.ShouldEqual(0);

            canAdd = true;
            addingCount = 0;
            for (var i = 0; i < count; i++)
            {
                expectedItem = new object();
                _componentCollection.TryAdd(expectedItem, Metadata).ShouldBeTrue();
            }

            addingCount.ShouldEqual(count);
            addedCount.ShouldEqual(count);
            _componentCollection.Count.ShouldEqual(count);
            _componentCollection.Get<object>().Count.ShouldEqual(count);
        }

        [Fact]
        public void ClearShouldCallOnDetachedMethods()
        {
            var detachingCount = 0;
            var detachedCount = 0;
            bool canDetach = false;
            var component = new TestAttachableComponent<ComponentCollectionTest>
            {
                OnDetachingHandler = (test, context) =>
                {
                    test.ShouldEqual(this);
                    context.ShouldEqual(Metadata);
                    ++detachingCount;
                    return canDetach;
                },
                OnDetachedHandler = (test, context) =>
                {
                    test.ShouldEqual(this);
                    context.ShouldEqual(Metadata);
                    detachedCount++;
                }
            };
            _componentCollection.TryAdd(component, Metadata).ShouldBeTrue();

            _componentCollection.Clear(Metadata);
            detachingCount.ShouldEqual(1);
            detachedCount.ShouldEqual(0);
            _componentCollection.Get<object>().Single().ShouldEqual(component);

            canDetach = true;
            _componentCollection.Clear(Metadata);
            detachingCount.ShouldEqual(2);
            detachedCount.ShouldEqual(1);
            _componentCollection.Count.ShouldEqual(0);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ClearShouldClearComponents(int count)
        {
            var components = new List<TestComponentCollectionProviderComponent>();
            for (var i = 0; i < count; i++)
            {
                var component = new TestComponentCollectionProviderComponent {Priority = i};
                components.Insert(0, component);
                _componentCollection.TryAdd(component, Metadata);
            }

            _componentCollection.Count.ShouldEqual(components.Count);
            _componentCollection.Clear(Metadata);
            _componentCollection.Count.ShouldEqual(0);
            _componentCollection.Get<TestComponentCollectionProviderComponent>().Count.ShouldEqual(0);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ClearShouldNotifyListeners(int count)
        {
            const int limit = 5;
            var items = new HashSet<object>();
            var removingCount = 0;
            var removedCount = 0;

            _componentCollection.AddComponent(new TestComponentCollectionChangingListener
            {
                OnRemoving = (collection, o, arg3) =>
                {
                    collection.ShouldEqual(_componentCollection);
                    arg3.ShouldEqual(Metadata);
                    return ++removingCount < limit + 1;
                }
            });
            _componentCollection.AddComponent(new TestComponentCollectionChangedListener
            {
                OnRemoved = (collection, o, arg3) =>
                {
                    collection.ShouldEqual(_componentCollection);
                    items.Remove(o).ShouldBeTrue();
                    arg3.ShouldEqual(Metadata);
                    removedCount++;
                }
            });

            for (var i = 0; i < count; i++)
            {
                var o = new object();
                items.Add(o);
                _componentCollection.TryAdd(o, Metadata);
            }

            _componentCollection.Clear(Metadata);
            removingCount.ShouldEqual(count);
            if (limit > count)
            {
                removedCount.ShouldEqual(count);
                items.Count.ShouldEqual(0);
            }
            else
            {
                removedCount.ShouldEqual(limit);
                items.Count.ShouldEqual(count - limit);
                _componentCollection.Get<object>().OrderBy(o => o.GetHashCode()).ShouldEqual(items.OrderBy(o => o.GetHashCode()));
            }
        }

        public override void ComponentOwnerShouldUseCollectionFactory(bool globalValue)
        {
            if (globalValue)
                base.ComponentOwnerShouldUseCollectionFactory(globalValue);
        }

        [Fact]
        public void GetShouldDecorateItems1()
        {
            var executed = 0;
            var owner = new TestComponentOwner<object>();
            var componentCollection = new ComponentCollection(owner, ComponentCollectionManager);

            var componentDecorated1 = new TestThreadDispatcherComponent();
            var componentDecorated2 = new TestThreadDispatcherComponent();

            var decoratorComponent1 = new TestComponentDecorator<IThreadDispatcher, IThreadDispatcherComponent> {Priority = 0};
            var decoratorComponent2 = new TestComponentDecorator<IThreadDispatcher, IThreadDispatcherComponent> {Priority = 1};
            var component = new TestThreadDispatcherComponent();
            componentCollection.TryAdd(component);

            componentCollection.Get<IThreadDispatcherComponent>(Metadata).ShouldEqual(new[] {component});
            decoratorComponent1.DecorateHandler = (IComponentCollection c, ref ItemOrListEditor<IThreadDispatcherComponent> list, IReadOnlyMetadataContext? context) =>
            {
                ++executed;
                c.ShouldEqual(componentCollection);
                list.AsList().ShouldEqual(new[] {component});
                context.ShouldEqual(Metadata);
                list.Add(componentDecorated1);
            };
            componentCollection.AddComponent(decoratorComponent1);

            componentCollection.Get<IThreadDispatcherComponent>(Metadata).ShouldEqual(new[] {component, componentDecorated1});
            executed.ShouldEqual(1);

            decoratorComponent2.DecorateHandler = (IComponentCollection c, ref ItemOrListEditor<IThreadDispatcherComponent> list, IReadOnlyMetadataContext? context) =>
            {
                ++executed;
                c.ShouldEqual(componentCollection);
                list.AsList().ShouldEqual(new[] {component, componentDecorated1});
                context.ShouldEqual(Metadata);
                list.Add(componentDecorated2);
            };
            componentCollection.AddComponent(decoratorComponent2);

            executed = 0;
            componentCollection.Get<IThreadDispatcherComponent>(Metadata).ShouldEqual(new[] {component, componentDecorated1, componentDecorated2});
            executed.ShouldEqual(2);

            componentCollection.RemoveComponent(decoratorComponent2);
            executed = 0;
            componentCollection.Get<IThreadDispatcherComponent>(Metadata).ShouldEqual(new[] {component, componentDecorated1});
            executed.ShouldEqual(1);

            executed = 0;
            componentCollection.RemoveComponent(decoratorComponent1);
            var components = componentCollection.Get<IThreadDispatcherComponent>(Metadata);
            components.ShouldEqual(new[] {component});
            executed.ShouldEqual(0);
        }

        [Fact]
        public void GetShouldDecorateItems2()
        {
            var executed = 0;
            var owner = new TestComponentOwner<object>();
            var componentCollection = new ComponentCollection(owner, ComponentCollectionManager);

            var componentDecorated1 = new TestThreadDispatcherComponent();
            var componentDecorated2 = new TestThreadDispatcherComponent();

            var decoratorComponent1 = new TestComponentCollectionDecorator<IThreadDispatcherComponent> {Priority = 0};
            var decoratorComponent2 = new TestComponentCollectionDecorator<IThreadDispatcherComponent> {Priority = 1};
            var component = new TestThreadDispatcherComponent();
            componentCollection.TryAdd(component);

            componentCollection.Get<IThreadDispatcherComponent>(Metadata).ShouldEqual(new[] {component});
            decoratorComponent1.DecorateHandler = (IComponentCollection c, ref ItemOrListEditor<IThreadDispatcherComponent> list, IReadOnlyMetadataContext? context) =>
            {
                ++executed;
                c.ShouldEqual(componentCollection);
                list.AsList().ShouldEqual(new[] {component});
                context.ShouldEqual(Metadata);
                list.Add(componentDecorated1);
            };
            componentCollection.AddComponent(decoratorComponent1);

            componentCollection.Get<IThreadDispatcherComponent>(Metadata).ShouldEqual(new[] {component, componentDecorated1});
            executed.ShouldEqual(1);

            decoratorComponent2.DecorateHandler = (IComponentCollection c, ref ItemOrListEditor<IThreadDispatcherComponent> list, IReadOnlyMetadataContext? context) =>
            {
                ++executed;
                c.ShouldEqual(componentCollection);
                list.AsList().ShouldEqual(new[] {component, componentDecorated1});
                context.ShouldEqual(Metadata);
                list.Add(componentDecorated2);
            };
            componentCollection.AddComponent(decoratorComponent2);

            executed = 0;
            componentCollection.Get<IThreadDispatcherComponent>(Metadata).ShouldEqual(new[] {component, componentDecorated1, componentDecorated2});
            executed.ShouldEqual(2);

            componentCollection.RemoveComponent(decoratorComponent2);
            executed = 0;
            componentCollection.Get<IThreadDispatcherComponent>(Metadata).ShouldEqual(new[] {component, componentDecorated1});
            executed.ShouldEqual(1);

            executed = 0;
            componentCollection.RemoveComponent(decoratorComponent1);
            var components = componentCollection.Get<IThreadDispatcherComponent>(Metadata);
            components.ShouldEqual(new[] {component});
            executed.ShouldEqual(0);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void InvalidateShouldUpdateComponentOrder(int count)
        {
            var components = new List<TestComponentCollectionProviderComponent>();
            for (var i = 0; i < count; i++)
            {
                var component = new TestComponentCollectionProviderComponent {Priority = i + 1};
                components.Insert(0, component);
                _componentCollection.TryAdd(component, Metadata).ShouldBeTrue();
            }

            _componentCollection.Count.ShouldEqual(components.Count);
            _componentCollection.Owner.ShouldEqual(this);
            _componentCollection.Get<TestComponentCollectionProviderComponent>().ShouldEqual(components);

            foreach (var component in components)
            {
                component.Priority = -component.Priority;
                _componentCollection.Invalidate(component);
                var list = _componentCollection.Get<TestComponentCollectionProviderComponent>();
                list.ShouldEqual(components.OrderByDescending(c => c.Priority));
            }
        }

        [Fact]
        public void RemoveShouldCallOnDetachingOnDetachedMethods()
        {
            var detachingCount = 0;
            var detachedCount = 0;
            var canDetach = false;

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
                    context.ShouldEqual(Metadata);
                }
            };
            _componentCollection.TryAdd(component, Metadata);

            _componentCollection.Remove(component, Metadata).ShouldBeFalse();
            detachingCount.ShouldEqual(1);
            detachedCount.ShouldEqual(0);
            _componentCollection.Count.ShouldEqual(1);
            _componentCollection.Get<object>().Single().ShouldEqual(component);

            canDetach = true;
            _componentCollection.Remove(component, Metadata).ShouldBeTrue();
            detachingCount.ShouldEqual(2);
            detachedCount.ShouldEqual(1);
            _componentCollection.Get<object>().Count.ShouldEqual(0);
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

            var changingListener = new TestComponentCollectionChangingListener
            {
                OnRemoving = (collection, o, arg3) =>
                {
                    removingCount++;
                    expectedItem.ShouldEqual(o);
                    arg3.ShouldEqual(Metadata);
                    return canRemove;
                }
            };
            _componentCollection.AddComponent(changingListener);
            var changedListener = new TestComponentCollectionChangedListener
            {
                OnRemoved = (collection, o, arg3) =>
                {
                    removedCount++;
                    expectedItem.ShouldEqual(o);
                    arg3.ShouldEqual(Metadata);
                }
            };
            _componentCollection.AddComponent(changedListener);

            for (var i = 0; i < count; i++)
                _componentCollection.TryAdd(new object(), Metadata);

            var objects = _componentCollection.Get<object>();
            foreach (var o in objects)
            {
                expectedItem = o;
                _componentCollection.Remove(expectedItem, Metadata).ShouldBeFalse();
            }

            removingCount.ShouldEqual(count);
            removedCount.ShouldEqual(0);
            _componentCollection.Count.ShouldEqual(count);
            _componentCollection.Get<object>().Count.ShouldEqual(count);

            canRemove = true;
            removingCount = 0;
            foreach (var o in objects)
            {
                expectedItem = o;
                _componentCollection.Remove(expectedItem, Metadata).ShouldBeTrue();
            }

            removingCount.ShouldEqual(count);
            removedCount.ShouldEqual(count);
            _componentCollection.Count.ShouldEqual(0);
            _componentCollection.Get<object>().Count.ShouldEqual(0);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void RemoveShouldRemoveComponent(int count)
        {
            var components = new List<TestComponentCollectionProviderComponent>();
            for (var i = 0; i < count; i++)
            {
                var component = new TestComponentCollectionProviderComponent {Priority = i};
                components.Insert(0, component);
                _componentCollection.TryAdd(component, Metadata);
            }

            for (var index = 0; index < components.Count; index++)
            {
                var component = components[index];
                components.RemoveAt(index--);
                _componentCollection.Remove(component).ShouldBeTrue();

                _componentCollection.Count.ShouldEqual(components.Count);
                _componentCollection.Get<TestComponentCollectionProviderComponent>().ShouldEqual(components);
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ShouldUseCorrectOrderForDecorators(int count)
        {
            var owner = new TestComponentOwner<object>();
            var componentCollection = new ComponentCollection(owner, ComponentCollectionManager);

            var executed = 0;
            for (var i = 0; i < count; i++)
            {
                var decoratorComponent = new TestThreadDispatcherDecorator
                {
                    DecorateHandler = (IComponentCollection c, ref ItemOrListEditor<IThreadDispatcherComponent> list, IReadOnlyMetadataContext? context) =>
                    {
                        ++executed;
                        c.ShouldEqual(componentCollection);
                        list.Add(new TestThreadDispatcherComponent());
                    }
                };
                componentCollection.TryAdd(decoratorComponent);
                componentCollection.AddComponent(decoratorComponent);
            }

            var components = componentCollection.Get<IThreadDispatcherComponent>();
            executed.ShouldEqual(count);
            components.Count.ShouldEqual(count * 2);
            components.OfType<TestThreadDispatcherComponent>().Count().ShouldEqual(count);
        }

        protected override IComponentCollection GetComponentOwner(IComponentCollectionManager? componentCollectionManager = null) =>
            new ComponentCollection(this, componentCollectionManager);

        private sealed class TestThreadDispatcherDecorator : TestComponentDecorator<IThreadDispatcher, IThreadDispatcherComponent>, IThreadDispatcherComponent
        {
            public bool CanExecuteInline(IThreadDispatcher threadDispatcher, ThreadExecutionMode executionMode, IReadOnlyMetadataContext? metadata) =>
                throw new NotSupportedException();

            public bool TryExecute(IThreadDispatcher threadDispatcher, ThreadExecutionMode executionMode, object handler, object? state, IReadOnlyMetadataContext? metadata) =>
                throw new NotSupportedException();
        }
    }
}