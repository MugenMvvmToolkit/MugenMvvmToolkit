using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.Threading.Components;
using MugenMvvm.Threading;
using MugenMvvm.UnitTest.Threading;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Components
{
    public class ComponentCollectionTest : ComponentOwnerTestBase<ComponentCollection>
    {
        #region Fields

        protected const int TestIterationCount = 50;

        #endregion

        #region Methods

        public override void ComponentOwnerShouldUseCollectionFactory(bool globalValue)
        {
            if (globalValue)
                base.ComponentOwnerShouldUseCollectionFactory(globalValue);
        }

        [Fact]
        public void AddShouldAddOrderedComponent()
        {
            var componentCollection = new ComponentCollection(this);
            var components = new List<TestComponentCollectionProviderComponent>();
            for (var i = 0; i < TestIterationCount; i++)
            {
                var component = new TestComponentCollectionProviderComponent { Priority = i };
                components.Insert(0, component);
                componentCollection.Add(component, DefaultMetadata).ShouldBeTrue();
            }

            componentCollection.Count.ShouldEqual(components.Count);
            componentCollection.Owner.ShouldEqual(this);
            componentCollection.Get<TestComponentCollectionProviderComponent>().SequenceEqual(components).ShouldBeTrue();
        }

        [Fact]
        public void RemoveShouldRemoveComponent()
        {
            var componentCollection = new ComponentCollection(this);
            var components = new List<TestComponentCollectionProviderComponent>();
            for (var i = 0; i < TestIterationCount; i++)
            {
                var component = new TestComponentCollectionProviderComponent { Priority = i };
                components.Insert(0, component);
                componentCollection.Add(component, DefaultMetadata);
            }

            for (var index = 0; index < components.Count; index++)
            {
                var component = components[index];
                components.RemoveAt(index--);
                componentCollection.Remove(component).ShouldBeTrue();

                componentCollection.Count.ShouldEqual(components.Count);
                componentCollection.Get<TestComponentCollectionProviderComponent>().SequenceEqual(components).ShouldBeTrue();
            }
        }

        [Fact]
        public void ClearShouldClearComponents()
        {
            var componentCollection = new ComponentCollection(this);
            var components = new List<TestComponentCollectionProviderComponent>();
            for (var i = 0; i < TestIterationCount; i++)
            {
                var component = new TestComponentCollectionProviderComponent { Priority = i };
                components.Insert(0, component);
                componentCollection.Add(component, DefaultMetadata);
            }

            componentCollection.Count.ShouldEqual(components.Count);
            componentCollection.Clear(DefaultMetadata);
            componentCollection.Count.ShouldEqual(0);
            componentCollection.Get<TestComponentCollectionProviderComponent>().Length.ShouldEqual(0);
        }

        [Fact]
        public void AddShouldNotifyListeners()
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

            for (var i = 0; i < TestIterationCount; i++)
            {
                expectedItem = new object();
                componentCollection.Add(expectedItem, DefaultMetadata).ShouldBeFalse();
            }

            addingCount.ShouldEqual(TestIterationCount);
            addedCount.ShouldEqual(0);
            componentCollection.Count.ShouldEqual(0);
            componentCollection.Get<object>().Length.ShouldEqual(0);

            canAdd = true;
            addingCount = 0;
            for (var i = 0; i < TestIterationCount; i++)
            {
                expectedItem = new object();
                componentCollection.Add(expectedItem, DefaultMetadata).ShouldBeTrue();
            }

            addingCount.ShouldEqual(TestIterationCount);
            addedCount.ShouldEqual(TestIterationCount);
            componentCollection.Count.ShouldEqual(TestIterationCount);
            componentCollection.Get<object>().Length.ShouldEqual(TestIterationCount);
        }

        [Fact]
        public void RemoveShouldNotifyListeners()
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

            for (var i = 0; i < TestIterationCount; i++)
                componentCollection.Add(new object(), DefaultMetadata);

            var objects = componentCollection.Get<object>();
            foreach (var o in objects)
            {
                expectedItem = o;
                componentCollection.Remove(expectedItem, DefaultMetadata).ShouldBeFalse();
            }

            removingCount.ShouldEqual(TestIterationCount);
            removedCount.ShouldEqual(0);
            componentCollection.Count.ShouldEqual(TestIterationCount);
            componentCollection.Get<object>().Length.ShouldEqual(TestIterationCount);

            canRemove = true;
            removingCount = 0;
            foreach (var o in objects)
            {
                expectedItem = o;
                componentCollection.Remove(expectedItem, DefaultMetadata).ShouldBeTrue();
            }

            removingCount.ShouldEqual(TestIterationCount);
            removedCount.ShouldEqual(TestIterationCount);
            componentCollection.Count.ShouldEqual(0);
            componentCollection.Get<object>().Length.ShouldEqual(0);
        }

        [Fact]
        public void ClearShouldNotifyListeners()
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

            for (var i = 0; i < TestIterationCount; i++)
            {
                var o = new object();
                items.Add(o);
                componentCollection.Add(o, DefaultMetadata);
            }

            componentCollection.Clear(DefaultMetadata);
            removedCount.ShouldEqual(TestIterationCount);
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
                OnAttaching = (test, context) =>
                {
                    attachingCount++;
                    test.ShouldEqual(this);
                    return canAttach;
                },
                OnAttached = (test, context) =>
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
                OnDetaching = (test, context) =>
                {
                    detachingCount++;
                    test.ShouldEqual(this);
                    return canDetach;
                },
                OnDetached = (test, context) =>
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
                OnDetached = (test, context) =>
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
            int executed = 0;
            var threadDispatcher = new ThreadDispatcher();
            var componentCollection = new ComponentCollection(threadDispatcher);

            var componentDecorated1 = new TestThreadDispatcherComponent();
            var componentDecorated2 = new TestThreadDispatcherComponent();

            var decoratorComponent1 = new TestDecoratorComponent<IThreadDispatcher, IThreadDispatcherComponent> { Priority = 0 };
            var decoratorComponent2 = new TestDecoratorComponent<IThreadDispatcher, IThreadDispatcherComponent> { Priority = 1 };
            var component = new TestThreadDispatcherComponent();
            componentCollection.Add(component);

            componentCollection.Get<IThreadDispatcherComponent>(DefaultMetadata).SequenceEqual(new[] { component }).ShouldBeTrue();
            decoratorComponent1.Decorate = (list, context) =>
            {
                ++executed;
                list.SequenceEqual(new[] { component }).ShouldBeTrue();
                context.ShouldEqual(DefaultMetadata);
                list.Add(componentDecorated1);
            };
            componentCollection.AddComponent(decoratorComponent1);

            componentCollection.Get<IThreadDispatcherComponent>(DefaultMetadata).SequenceEqual(new[] { component, componentDecorated1 }).ShouldBeTrue();
            executed.ShouldEqual(1);

            decoratorComponent2.Decorate = (list, context) =>
            {
                ++executed;
                list.SequenceEqual(new[] { component, componentDecorated1 }).ShouldBeTrue();
                context.ShouldEqual(DefaultMetadata);
                list.Add(componentDecorated2);
            };
            componentCollection.AddComponent(decoratorComponent2);

            executed = 0;
            componentCollection.Get<IThreadDispatcherComponent>(DefaultMetadata).SequenceEqual(new[] { component, componentDecorated1, componentDecorated2 }).ShouldBeTrue();
            executed.ShouldEqual(2);

            componentCollection.RemoveComponent(decoratorComponent2);
            executed = 0;
            componentCollection.Get<IThreadDispatcherComponent>(DefaultMetadata).SequenceEqual(new[] { component, componentDecorated1 }).ShouldBeTrue();
            executed.ShouldEqual(1);

            executed = 0;
            componentCollection.RemoveComponent(decoratorComponent1);
            var components = componentCollection.Get<IThreadDispatcherComponent>(DefaultMetadata);
            components.SequenceEqual(new[] { component }).ShouldBeTrue();
            executed.ShouldEqual(0);
        }

        protected override ComponentCollection GetComponentOwner(IComponentCollectionProvider? collectionProvider = null)
        {
            return new ComponentCollection(this);
        }

        #endregion
    }
}