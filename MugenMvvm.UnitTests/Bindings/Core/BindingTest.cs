using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Metadata;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Bindings.Observation.Observers;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Tests.Bindings.Core;
using MugenMvvm.Tests.Bindings.Members;
using MugenMvvm.Tests.Bindings.Observation;
using MugenMvvm.Tests.Components;
using MugenMvvm.UnitTests.Components;
using MugenMvvm.UnitTests.Components.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Core
{
    [Collection(SharedContext)]
    public class BindingTest : ComponentOwnerTestBase<Binding>
    {
        public BindingTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            RegisterDisposeToken(WithGlobalService(BindingManager));
            RegisterDisposeToken(WithGlobalService(GlobalValueConverter));
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(10, true)]
        [InlineData(1, false)]
        [InlineData(10, false)]
        public void AddDelegateShouldAddOrderedComponent(int count, bool nullResult)
        {
            IComponentCollection binding = GetBinding();
            var components = new List<TestComponentCollectionProviderComponent>();
            for (var i = 0; i < count; i++)
            {
                binding.TryAdd(this, (collection, s, m) =>
                {
                    collection.ShouldEqual(binding);
                    s.ShouldEqual(this);
                    m.ShouldEqual(Metadata);
                    if (nullResult)
                        return null;
                    var component = new TestComponentCollectionProviderComponent {Priority = i};
                    components.Insert(0, component);
                    return component;
                }, Metadata).ShouldEqual(nullResult ? null : components[0]);
            }

            binding.Owner.ShouldEqual(binding);
            binding.Count.ShouldEqual(nullResult ? 0 : components.Count);
            binding.Get<TestComponentCollectionProviderComponent>().ShouldEqual(components);
        }

        [Fact]
        public void AddDelegateShouldCallOnAttachingOnAttachedMethods()
        {
            var canAttachCount = 0;
            var attachingCount = 0;
            var attachedCount = 0;
            var canAttach = false;
            IComponentCollection binding = GetBinding();

            var component = new TestAttachableComponent<object>
            {
                OnAttachingHandler = (test, context) =>
                {
                    attachingCount++;
                    test.ShouldEqual(binding);
                    context.ShouldEqual(Metadata);
                },
                OnAttachedHandler = (test, context) =>
                {
                    attachedCount++;
                    test.ShouldEqual(binding);
                    context.ShouldEqual(Metadata);
                },
                CanAttach = (test, context) =>
                {
                    canAttachCount++;
                    test.ShouldEqual(binding);
                    context.ShouldEqual(Metadata);
                    return canAttach;
                }
            };

            binding.TryAdd(this, (_, _, _) => component, Metadata).ShouldBeNull();
            canAttachCount.ShouldEqual(1);
            attachingCount.ShouldEqual(0);
            attachedCount.ShouldEqual(0);
            binding.Count.ShouldEqual(0);

            canAttach = true;
            binding.TryAdd(this, (_, _, _) => component, Metadata).ShouldEqual(component);
            canAttachCount.ShouldEqual(2);
            attachingCount.ShouldEqual(1);
            attachedCount.ShouldEqual(1);
            binding.Get<object>().Single().ShouldEqual(component);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void AddDelegateShouldNotifyListeners(int count)
        {
            const int defaultCount = 3;
            var canAddCount = 0;
            var addingCount = 0;
            var addedCount = 0;
            var canAdd = false;
            object? expectedItem = null;
            IComponentCollection binding = GetBinding();

            binding.AddComponent(new TestConditionComponentCollectionComponent
            {
                CanAdd = (collection, o, arg3) =>
                {
                    if (expectedItem == null)
                        return true;
                    canAddCount++;
                    collection.ShouldEqual(binding);
                    expectedItem.ShouldEqual(o);
                    arg3.ShouldEqual(Metadata);
                    return canAdd;
                }
            });
            binding.AddComponent(new TestComponentCollectionChangedListener
            {
                OnAdded = (collection, o, arg3) =>
                {
                    if (expectedItem == null)
                        return;
                    addedCount++;
                    collection.ShouldEqual(binding);
                    expectedItem.ShouldEqual(o);
                    arg3.ShouldEqual(Metadata);
                }
            });
            binding.AddComponent(new TestComponentCollectionChangingListener
            {
                OnAdding = (collection, o, arg3) =>
                {
                    if (expectedItem == null)
                        return;
                    addingCount++;
                    collection.ShouldEqual(binding);
                    expectedItem.ShouldEqual(o);
                    arg3.ShouldEqual(Metadata);
                }
            }, Metadata);

            for (var i = 0; i < count; i++)
            {
                expectedItem = new object();
                binding.TryAdd(this, (_, _, _) => expectedItem, Metadata).ShouldBeNull();
            }

            canAddCount.ShouldEqual(count);
            addingCount.ShouldEqual(0);
            addedCount.ShouldEqual(0);
            binding.Count.ShouldEqual(defaultCount);
            binding.Get<object>().Count.ShouldEqual(defaultCount);

            canAdd = true;
            canAddCount = 0;
            for (var i = 0; i < count; i++)
            {
                expectedItem = new object();
                binding.TryAdd(this, (_, _, _) => expectedItem, Metadata).ShouldEqual(expectedItem);
            }

            canAddCount.ShouldEqual(count);
            addingCount.ShouldEqual(count);
            addedCount.ShouldEqual(count);
            binding.Count.ShouldEqual(count + defaultCount);
            binding.Get<object>().Count.ShouldEqual(count + defaultCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void AddShouldAddOrderedComponent(int count)
        {
            var binding = GetBinding();
            var componentCollection = (IComponentCollection) binding;
            var components = new List<TestComponentCollectionProviderComponent>();
            for (var i = 0; i < count; i++)
            {
                var component = new TestComponentCollectionProviderComponent {Priority = i};
                components.Insert(0, component);
                componentCollection.TryAdd(component, Metadata).ShouldBeTrue();
            }

            componentCollection.Count.ShouldEqual(components.Count);
            componentCollection.Owner.ShouldEqual(binding);
            binding.GetComponents<object>().ShouldEqual(components);
        }

        [Fact]
        public void AddShouldCallOnAttachingOnAttachedMethods()
        {
            var canAttachCount = 0;
            var attachingCount = 0;
            var attachedCount = 0;
            var canAttach = false;
            var binding = GetBinding();
            var componentCollection = (IComponentCollection) binding;
            var component = new TestAttachableComponent<IBinding>
            {
                OnAttachingHandler = (test, context) =>
                {
                    attachingCount++;
                    test.ShouldEqual(binding);
                    context.ShouldEqual(Metadata);
                },
                OnAttachedHandler = (test, context) =>
                {
                    attachedCount++;
                    test.ShouldEqual(binding);
                    context.ShouldEqual(Metadata);
                },
                CanAttach = (test, context) =>
                {
                    canAttachCount++;
                    test.ShouldEqual(binding);
                    context.ShouldEqual(Metadata);
                    return canAttach;
                }
            };

            componentCollection.TryAdd(component, Metadata).ShouldBeFalse();
            canAttachCount.ShouldEqual(1);
            attachingCount.ShouldEqual(0);
            attachedCount.ShouldEqual(0);
            componentCollection.Count.ShouldEqual(0);

            canAttach = true;
            componentCollection.TryAdd(component, Metadata).ShouldBeTrue();
            canAttachCount.ShouldEqual(2);
            attachingCount.ShouldEqual(1);
            attachedCount.ShouldEqual(1);
            binding.GetComponents<object>().Single().ShouldEqual(component);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void AddShouldNotifyListeners(int count)
        {
            const int defaultCount = 3;
            var canAddCount = 0;
            var addingCount = 0;
            var addedCount = 0;
            var canAdd = true;
            object? expectedItem = null;
            var binding = GetBinding();
            var componentCollection = (IComponentCollection) binding;
            componentCollection.AddComponent(new TestComponentCollectionChangingListener
            {
                OnAdding = (collection, o, arg3) =>
                {
                    if (expectedItem == null)
                        return;
                    addingCount++;
                    collection.ShouldEqual(binding);
                    expectedItem.ShouldEqual(o);
                    arg3.ShouldEqual(Metadata);
                }
            });
            componentCollection.AddComponent(new TestConditionComponentCollectionComponent
            {
                CanAdd = (collection, o, arg3) =>
                {
                    if (expectedItem == null)
                        return true;
                    canAddCount++;
                    collection.ShouldEqual(binding);
                    expectedItem.ShouldEqual(o);
                    arg3.ShouldEqual(Metadata);
                    return canAdd;
                }
            });

            var changedListener = new TestComponentCollectionChangedListener
            {
                OnAdded = (collection, o, arg3) =>
                {
                    if (expectedItem == null)
                        return;
                    addedCount++;
                    collection.ShouldEqual(binding);
                    expectedItem.ShouldEqual(o);
                    arg3.ShouldEqual(Metadata);
                }
            };
            expectedItem = changedListener;
            componentCollection.AddComponent(changedListener, Metadata);
            addedCount.ShouldEqual(0);
            addingCount.ShouldEqual(1);
            canAddCount.ShouldEqual(1);
            canAddCount = 0;
            addingCount = 0;
            canAdd = false;

            for (var i = 0; i < count; i++)
            {
                expectedItem = new object();
                componentCollection.TryAdd(expectedItem, Metadata).ShouldBeFalse();
            }

            canAddCount.ShouldEqual(count);
            addingCount.ShouldEqual(0);
            addedCount.ShouldEqual(0);
            componentCollection.Count.ShouldEqual(defaultCount);
            binding.GetComponents<object>().Count.ShouldEqual(defaultCount);

            canAdd = true;
            canAddCount = 0;
            for (var i = 0; i < count; i++)
            {
                expectedItem = new object();
                componentCollection.TryAdd(expectedItem, Metadata).ShouldBeTrue();
            }

            canAddCount.ShouldEqual(count);
            addingCount.ShouldEqual(count);
            addedCount.ShouldEqual(count);
            componentCollection.Count.ShouldEqual(count + defaultCount);
            binding.GetComponents<object>().Count.ShouldEqual(count + defaultCount);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 10)]
        [InlineData(10, 1)]
        [InlineData(10, 10)]
        public void BindingShouldRedirectObserverEventsToBindingSourceObserverListener(int count, int observerCount)
        {
            var listeners = new Dictionary<TestMemberPathObserver, IMemberPathObserverListener?>();
            var target = new TestMemberPathObserver
            {
                AddListener = listener => throw new NotSupportedException()
            };
            TestMemberPathObserver[] sources = new TestMemberPathObserver[observerCount];
            for (var i = 0; i < observerCount; i++)
            {
                var observer = new TestMemberPathObserver();
                listeners[observer] = null;
                sources[i] = observer;
                observer.AddListener = l =>
                {
                    listeners[observer].ShouldBeNull();
                    listeners[observer] = l;
                };
                observer.RemoveListener = l =>
                {
                    listeners[observer].ShouldEqual(l);
                    listeners[observer] = null;
                };
            }

            var exception = new Exception();
            var pathMembersCount = 0;
            var lastMemberCount = 0;
            var errorCount = 0;
            var binding = GetBinding(target, sources.Length == 1 ? sources[0] : sources);
            IMemberPathObserver? source = null;
            for (var i = 0; i < count; i++)
            {
                binding.AddComponent(new TestBindingSourceObserverListener
                {
                    OnSourcePathMembersChanged = (b, o, m) =>
                    {
                        ++pathMembersCount;
                        b.ShouldEqual(binding);
                        o.ShouldEqual(source);
                        m.ShouldEqual(binding);
                    },
                    OnSourceLastMemberChanged = (b, o, m) =>
                    {
                        ++lastMemberCount;
                        b.ShouldEqual(binding);
                        o.ShouldEqual(source);
                        m.ShouldEqual(binding);
                    },
                    OnSourceError = (b, o, e, m) =>
                    {
                        ++errorCount;
                        b.ShouldEqual(binding);
                        o.ShouldEqual(source);
                        e.ShouldEqual(exception);
                        m.ShouldEqual(binding);
                    }
                });
            }

            foreach (var listenerPair in listeners)
            {
                source = listenerPair.Key;
                var listener = listenerPair.Value;
                lastMemberCount = errorCount = pathMembersCount = 0;

                listener.ShouldEqual(binding);
                lastMemberCount.ShouldEqual(0);
                errorCount.ShouldEqual(0);
                pathMembersCount.ShouldEqual(0);

                listener!.OnError(source, exception);
                lastMemberCount.ShouldEqual(0);
                errorCount.ShouldEqual(count);
                pathMembersCount.ShouldEqual(0);

                listener!.OnLastMemberChanged(source);
                lastMemberCount.ShouldEqual(count);
                errorCount.ShouldEqual(count);
                pathMembersCount.ShouldEqual(0);

                listener!.OnPathMembersChanged(source);
                lastMemberCount.ShouldEqual(count);
                errorCount.ShouldEqual(count);
                pathMembersCount.ShouldEqual(count);
            }

            foreach (var o in binding.GetComponents<object>())
                binding.RemoveComponent((IComponent<IBinding>) o);

            foreach (var listener in listeners)
                listener.Value.ShouldBeNull();

            lastMemberCount.ShouldEqual(count);
            errorCount.ShouldEqual(count);
            pathMembersCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void BindingShouldRedirectObserverEventsToBindingTargetObserverListener(int count)
        {
            IMemberPathObserverListener? listener = null;
            var target = new TestMemberPathObserver
            {
                AddListener = l =>
                {
                    listener.ShouldBeNull();
                    listener = l;
                },
                RemoveListener = l =>
                {
                    listener.ShouldEqual(l);
                    listener = null;
                }
            };
            var source = new TestMemberPathObserver
            {
                AddListener = listener => throw new NotSupportedException()
            };
            var exception = new Exception();

            var pathMembersCount = 0;
            var lastMemberCount = 0;
            var errorCount = 0;
            var binding = GetBinding(target, source);
            for (var i = 0; i < count; i++)
            {
                binding.AddComponent(new TestBindingTargetObserverListener
                {
                    OnTargetPathMembersChanged = (b, o, m) =>
                    {
                        ++pathMembersCount;
                        b.ShouldEqual(binding);
                        o.ShouldEqual(target);
                        m.ShouldEqual(binding);
                    },
                    OnTargetLastMemberChanged = (b, o, m) =>
                    {
                        ++lastMemberCount;
                        b.ShouldEqual(binding);
                        o.ShouldEqual(target);
                        m.ShouldEqual(binding);
                    },
                    OnTargetError = (b, o, e, m) =>
                    {
                        ++errorCount;
                        b.ShouldEqual(binding);
                        o.ShouldEqual(target);
                        e.ShouldEqual(exception);
                        m.ShouldEqual(binding);
                    }
                });
            }

            listener.ShouldEqual(binding);
            lastMemberCount.ShouldEqual(0);
            errorCount.ShouldEqual(0);
            pathMembersCount.ShouldEqual(0);

            listener!.OnError(target, exception);
            lastMemberCount.ShouldEqual(0);
            errorCount.ShouldEqual(count);
            pathMembersCount.ShouldEqual(0);

            listener!.OnLastMemberChanged(target);
            lastMemberCount.ShouldEqual(count);
            errorCount.ShouldEqual(count);
            pathMembersCount.ShouldEqual(0);

            listener!.OnPathMembersChanged(target);
            lastMemberCount.ShouldEqual(count);
            errorCount.ShouldEqual(count);
            pathMembersCount.ShouldEqual(count);

            foreach (var o in binding.GetComponents<object>())
                binding.RemoveComponent((IComponent<IBinding>) o);

            listener.ShouldBeNull();
            lastMemberCount.ShouldEqual(count);
            errorCount.ShouldEqual(count);
            pathMembersCount.ShouldEqual(count);
        }

        [Fact]
        public void ClearShouldCallOnDetachedMethods()
        {
            var canDetachCount = 0;
            var detachingCount = 0;
            var detachedCount = 0;
            bool canDetach = false;
            IComponentCollection binding = GetBinding();
            var component = new TestAttachableComponent<object>
            {
                OnDetachingHandler = (test, context) =>
                {
                    test.ShouldEqual(binding);
                    context.ShouldEqual(Metadata);
                    ++detachingCount;
                },
                OnDetachedHandler = (test, context) =>
                {
                    test.ShouldEqual(binding);
                    context.ShouldEqual(Metadata);
                    detachedCount++;
                },
                CanDetach = (test, context) =>
                {
                    test.ShouldEqual(binding);
                    context.ShouldEqual(Metadata);
                    ++canDetachCount;
                    return canDetach;
                }
            };
            binding.TryAdd(component, Metadata).ShouldBeTrue();

            binding.Clear(Metadata);
            canDetachCount.ShouldEqual(1);
            detachingCount.ShouldEqual(0);
            detachedCount.ShouldEqual(0);
            binding.Get<object>().Single().ShouldEqual(component);

            canDetach = true;
            binding.Clear(Metadata);
            canDetachCount.ShouldEqual(2);
            detachingCount.ShouldEqual(1);
            detachedCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ClearShouldClearComponents(int count)
        {
            var binding = GetBinding();
            var componentCollection = (IComponentCollection) binding;
            var components = new List<TestComponentCollectionProviderComponent>();
            for (var i = 0; i < count; i++)
            {
                var component = new TestComponentCollectionProviderComponent {Priority = i};
                components.Insert(0, component);
                componentCollection.TryAdd(component, Metadata);
            }

            componentCollection.Count.ShouldEqual(components.Count);
            componentCollection.Clear(Metadata);
            componentCollection.Count.ShouldEqual(0);
            binding.GetComponents<object>().Count.ShouldEqual(0);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ClearShouldNotifyListeners(int count)
        {
            const int limit = 5;
            var items = new HashSet<object>();
            var canRemoveCount = 0;
            var removingCount = 0;
            var removedCount = 0;
            IComponentCollection binding = GetBinding();

            binding.AddComponent(new TestConditionComponentCollectionComponent
            {
                CanRemove = (collection, o, arg3) =>
                {
                    collection.ShouldEqual(binding);
                    arg3.ShouldEqual(Metadata);
                    return ++canRemoveCount < limit + 1;
                },
                Priority = int.MinValue + 10
            });
            binding.AddComponent(new TestComponentCollectionChangingListener
            {
                OnRemoving = (collection, o, arg3) =>
                {
                    if (o.GetType() != typeof(object))
                        return;
                    collection.ShouldEqual(binding);
                    arg3.ShouldEqual(Metadata);
                    removingCount++;
                },
                Priority = int.MinValue + 9
            });
            binding.AddComponent(new TestComponentCollectionChangedListener
            {
                OnRemoved = (collection, o, arg3) =>
                {
                    if (o.GetType() != typeof(object))
                        return;

                    collection.ShouldEqual(binding);
                    items.Remove(o).ShouldBeTrue();
                    arg3.ShouldEqual(Metadata);
                    removedCount++;
                },
                Priority = int.MinValue + 8
            }, Metadata);
            canRemoveCount = 0;

            for (var i = 0; i < count; i++)
            {
                var o = new object();
                items.Add(o);
                binding.TryAdd(o, Metadata);
            }

            binding.Clear(Metadata);
            canRemoveCount.ShouldEqual(count);
            if (limit > count)
            {
                removingCount.ShouldEqual(count);
                removedCount.ShouldEqual(count);
                items.Count.ShouldEqual(0);
            }
            else
            {
                removingCount.ShouldEqual(limit);
                removedCount.ShouldEqual(limit);
                items.Count.ShouldEqual(count - limit);
                binding.Get<object>().ShouldEqualUnordered(items);
            }
        }

        public override void ComponentOwnerShouldUseCollectionFactory(bool globalValue)
        {
        }

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            var binding = new Binding(EmptyPathObserver.Empty, this);
            binding.Target.ShouldEqual(EmptyPathObserver.Empty);
            binding.Source.ShouldEqual(this);
            binding.State.ShouldEqual(BindingState.Valid);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public virtual void DisposeShouldClearBinding(int count)
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

            var binding = GetBinding(target, source);
            var disposeComponentCount = 0;
            var components = Enumerable
                             .Range(0, count)
                             .Select(i => new TestComponent<IBinding>
                             {
                                 Dispose = (o, _) =>
                                 {
                                     o.ShouldEqual(binding);
                                     ++disposeComponentCount;
                                 }
                             })
                             .Concat(new IComponent<IBinding>[] {new TestBindingTargetObserverListener(), new TestBindingSourceObserverListener()})
                             .ToArray();

            binding.State.ShouldEqual(BindingState.Valid);
            binding.Initialize(components, Metadata);
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
            binding.GetComponents<object>().IsEmpty.ShouldBeTrue();
            targetListener.ShouldBeNull();
            sourceListener.ShouldBeNull();
            disposeCount.ShouldEqual(1);

            binding.TryAddComponent(components[0]).IsEmpty.ShouldBeTrue();
            binding.GetComponents<object>().IsEmpty.ShouldBeTrue();
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(1, false)]
        [InlineData(2, true)]
        [InlineData(2, false)]
        [InlineData(5, true)]
        [InlineData(5, false)]
        public void InitializeShouldInitializeComponents(int count, bool canAddAll)
        {
            var binding = GetBinding();
            var attachingCount = 0;
            var attachedCount = 0;
            var components = new IComponent<IBinding>[count];
            var addedComponents = new List<IComponent<IBinding>>();
            for (var i = 0; i < count; i++)
            {
                var canAdd = i % 2 == 0;
                var component = new TestAttachableComponent<IBinding>
                {
                    OnAttachingHandler = (b, ctx) =>
                    {
                        b.ShouldEqual(binding);
                        ctx.ShouldEqual(Metadata);
                        attachingCount++;
                    },
                    OnAttachedHandler = (b, ctx) =>
                    {
                        ++attachedCount;
                        b.ShouldEqual(binding);
                        ctx.ShouldEqual(Metadata);
                    },
                    CanAttach = (b, ctx) =>
                    {
                        b.ShouldEqual(binding);
                        ctx.ShouldEqual(Metadata);
                        return canAddAll || canAdd;
                    }
                };
                components[i] = component;
                if (canAddAll || canAdd)
                    addedComponents.Add(component);
            }

            binding.Initialize(components, Metadata);
            binding.GetComponents<object>().ShouldEqual(addedComponents);
            attachingCount.ShouldEqual(addedComponents.Count);
            attachedCount.ShouldEqual(addedComponents.Count);

            ShouldThrow<InvalidOperationException>(() => binding.Initialize(components, Metadata));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void InvalidateShouldUpdateComponentOrder(int count)
        {
            var binding = GetBinding();
            var components = new List<TestComponentCollectionProviderComponent>();
            var componentCollection = (IComponentCollection) binding;
            for (var i = 0; i < count; i++)
            {
                var component = new TestComponentCollectionProviderComponent {Priority = i + 1};
                components.Insert(0, component);
                componentCollection.TryAdd(component, Metadata).ShouldBeTrue();
            }

            componentCollection.Count.ShouldEqual(components.Count);
            componentCollection.Owner.ShouldEqual(binding);
            componentCollection.Get<object>().ShouldEqual(components);

            foreach (var component in components)
            {
                component.Priority = -component.Priority;
                componentCollection.Invalidate(component);
                var list = componentCollection.Get<object>();
                list.ShouldEqual(components.OrderByDescending(c => c.Priority));
            }
        }

        [Fact]
        public void MetadataShouldReturnBinding()
        {
            var binding = GetBinding();
            var context = (IReadOnlyMetadataContext) binding;
            context.Count.ShouldEqual(1);
            context.Contains(BindingMetadata.Binding).ShouldBeTrue();
            context.TryGet(BindingMetadata.Binding, out var b).ShouldBeTrue();
            b.ShouldEqual(binding);
            context.GetValues().ShouldEqual(new[] {BindingMetadata.Binding.ToValue(binding)});
        }

        [Fact]
        public void RemoveShouldCallOnDetachingOnDetachedMethods()
        {
            var canDetachCount = 0;
            var detachingCount = 0;
            var detachedCount = 0;
            var canDetach = false;
            var binding = GetBinding();
            var componentCollection = (IComponentCollection) binding;
            var component = new TestAttachableComponent<IBinding>
            {
                OnDetachingHandler = (test, context) =>
                {
                    detachingCount++;
                    test.ShouldEqual(binding);
                    context.ShouldEqual(Metadata);
                },
                OnDetachedHandler = (test, context) =>
                {
                    detachedCount++;
                    test.ShouldEqual(binding);
                    context.ShouldEqual(Metadata);
                },
                CanDetach = (test, context) =>
                {
                    canDetachCount++;
                    test.ShouldEqual(binding);
                    context.ShouldEqual(Metadata);
                    return canDetach;
                }
            };
            componentCollection.TryAdd(component, Metadata);

            componentCollection.Remove(component, Metadata).ShouldBeFalse();
            canDetachCount.ShouldEqual(1);
            detachingCount.ShouldEqual(0);
            detachedCount.ShouldEqual(0);
            componentCollection.Count.ShouldEqual(1);
            binding.GetComponents<object>().Single().ShouldEqual(component);

            canDetach = true;
            componentCollection.Remove(component, Metadata).ShouldBeTrue();
            canDetachCount.ShouldEqual(2);
            detachingCount.ShouldEqual(1);
            detachedCount.ShouldEqual(1);
            binding.GetComponents<object>().Count.ShouldEqual(0);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void RemoveShouldNotifyListeners(int count)
        {
            const int defaultCount = 3;
            var canRemoveCount = 0;
            var removingCount = 0;
            var removedCount = 0;
            var canRemove = false;
            object? expectedItem = null;
            var binding = GetBinding();
            var componentCollection = (IComponentCollection) binding;

            componentCollection.AddComponent(new TestConditionComponentCollectionComponent()
            {
                Priority = -1,
                CanRemove = (collection, o, arg3) =>
                {
                    canRemoveCount++;
                    collection.ShouldEqual(binding);
                    expectedItem.ShouldEqual(o);
                    arg3.ShouldEqual(Metadata);
                    return canRemove;
                }
            });
            componentCollection.AddComponent(new TestComponentCollectionChangingListener
            {
                Priority = -1,
                OnRemoving = (collection, o, arg3) =>
                {
                    removingCount++;
                    collection.ShouldEqual(binding);
                    expectedItem.ShouldEqual(o);
                    arg3.ShouldEqual(Metadata);
                }
            });
            componentCollection.AddComponent(new TestComponentCollectionChangedListener
            {
                Priority = -1,
                OnRemoved = (collection, o, arg3) =>
                {
                    removedCount++;
                    expectedItem.ShouldEqual(o);
                    arg3.ShouldEqual(Metadata);
                }
            });

            for (var i = 0; i < count; i++)
                componentCollection.TryAdd(new object(), Metadata);

            var objects = binding.GetComponents<object>().Where(o => o.GetType() == typeof(object)).ToArray();
            foreach (var o in objects)
            {
                expectedItem = o;
                componentCollection.Remove(expectedItem, Metadata).ShouldBeFalse();
            }

            canRemoveCount.ShouldEqual(count);
            removingCount.ShouldEqual(0);
            removedCount.ShouldEqual(0);
            componentCollection.Count.ShouldEqual(count + defaultCount);
            binding.GetComponents<object>().Count.ShouldEqual(count + defaultCount);

            canRemove = true;
            canRemoveCount = 0;
            foreach (var o in objects)
            {
                expectedItem = o;
                componentCollection.Remove(expectedItem, Metadata).ShouldBeTrue();
            }

            canRemoveCount.ShouldEqual(count);
            removingCount.ShouldEqual(count);
            removedCount.ShouldEqual(count);
            componentCollection.Count.ShouldEqual(defaultCount);
            binding.GetComponents<object>().Count.ShouldEqual(defaultCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void RemoveShouldRemoveComponent(int count)
        {
            var binding = GetBinding();
            var componentCollection = (IComponentCollection) binding;
            var components = new List<TestComponentCollectionProviderComponent>();
            for (var i = 0; i < count; i++)
            {
                var component = new TestComponentCollectionProviderComponent {Priority = i};
                components.Insert(0, component);
                componentCollection.TryAdd(component, Metadata);
            }

            for (var index = 0; index < components.Count; index++)
            {
                var component = components[index];
                components.RemoveAt(index--);
                componentCollection.Remove(component).ShouldBeTrue();

                componentCollection.Count.ShouldEqual(components.Count);
                binding.GetComponents<object>().ShouldEqual(components);
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void UpdateSourceShouldNotifyListeners(int count)
        {
            object targetValue = "1";
            object sourceValue = "2";
            object? targetObj = null;
            Exception? exception = null;
            var targetMember = new TestAccessorMemberInfo
            {
                Type = typeof(object),
                GetValue = (o, context) => targetValue
            };
            var sourceMember = new TestAccessorMemberInfo
            {
                Type = typeof(object),
                GetValue = (o, context) => sourceValue,
                SetValue = (o, v, context) => { }
            };
            var source = new TestMemberPathObserver
            {
                GetLastMember = metadata => new MemberPathLastMember(new object(), targetMember)
            };
            var target = new TestMemberPathObserver
            {
                GetLastMember = metadata =>
                {
                    if (exception != null)
                        return new MemberPathLastMember(exception);
                    if (targetObj == null)
                        return default;
                    return new MemberPathLastMember(targetObj, sourceMember);
                }
            };

            var updateFailed = 0;
            var updateCanceled = 0;
            var updated = 0;
            var binding = new Binding(target, source);
            for (var i = 0; i < count; i++)
            {
                binding.AddComponent(new TestBindingSourceListener
                {
                    OnSourceUpdateFailed = (b, e, m) =>
                    {
                        ++updateFailed;
                        b.ShouldEqual(binding);
                        e.ShouldEqual(exception);
                        m.ShouldEqual(binding);
                    },
                    OnSourceUpdateCanceled = (b, m) =>
                    {
                        ++updateCanceled;
                        b.ShouldEqual(binding);
                        m.ShouldEqual(binding);
                    },
                    OnSourceUpdated = (b, v, m) =>
                    {
                        ++updated;
                        b.ShouldEqual(binding);
                        v.ShouldEqual(sourceValue);
                        m.ShouldEqual(binding);
                    }
                });
            }

            binding.UpdateSource();
            updateCanceled.ShouldEqual(count);
            updateFailed.ShouldEqual(0);
            updated.ShouldEqual(0);

            updateCanceled = updateFailed = updated = 0;
            targetObj = new object();
            sourceValue = BindingMetadata.UnsetValue;
            binding.UpdateSource();
            updateCanceled.ShouldEqual(count);
            updateFailed.ShouldEqual(0);
            updated.ShouldEqual(0);

            updateCanceled = updateFailed = updated = 0;
            sourceValue = new object();
            binding.UpdateSource();
            updateCanceled.ShouldEqual(0);
            updateFailed.ShouldEqual(0);
            updated.ShouldEqual(count);

            updateCanceled = updateFailed = updated = 0;
            exception = new Exception();
            binding.UpdateSource();
            updateCanceled.ShouldEqual(0);
            updateFailed.ShouldEqual(count);
            updated.ShouldEqual(0);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void UpdateSourceShouldUseSourceSetter(int count)
        {
            Binding? binding = null;
            var targetObj = new object();
            var sourceObj = new object();
            object targetValue = 0;
            object sourceValue = 0;
            int targetGet = 0, targetSet = 0, sourceGet = 0, sourceSet = 0, interceptCount = 0;
            var targetMember = new TestAccessorMemberInfo
            {
                Type = typeof(object),
                GetValue = (o, context) =>
                {
                    ++targetGet;
                    o.ShouldEqual(targetObj);
                    context.ShouldEqual(binding);
                    return targetValue;
                },
                SetValue = (o, v, context) =>
                {
                    ++targetSet;
                    o.ShouldEqual(targetObj);
                    v.ShouldEqual(count);
                    context.ShouldEqual(binding);
                }
            };
            var sourceMember = new TestAccessorMemberInfo
            {
                Type = typeof(object),
                GetValue = (o, context) =>
                {
                    ++sourceGet;
                    o.ShouldEqual(sourceObj);
                    context.ShouldEqual(binding);
                    return sourceValue;
                },
                SetValue = (o, v, context) =>
                {
                    ++sourceSet;
                    o.ShouldEqual(sourceObj);
                    v.ShouldEqual(count);
                    context.ShouldEqual(binding);
                }
            };
            var target = new TestMemberPathObserver
            {
                GetLastMember = metadata => new MemberPathLastMember(targetObj, targetMember)
            };
            var source = new TestMemberPathObserver
            {
                GetLastMember = metadata => new MemberPathLastMember(sourceObj, sourceMember)
            };

            binding = GetBinding(target, source);
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                binding.AddComponent(new TestSourceValueSetterComponent
                {
                    Priority = -i,
                    TrySetSourceValue = (b, member, value, metadata) =>
                    {
                        ++interceptCount;
                        member.Target.ShouldEqual(sourceObj);
                        member.Member.ShouldEqual(sourceMember);
                        value.ShouldEqual(targetValue);
                        return isLast;
                    }
                });
            }

            binding.UpdateSource();
            targetGet.ShouldEqual(1);
            targetSet.ShouldEqual(0);
            sourceGet.ShouldEqual(0);
            sourceSet.ShouldEqual(0);
            interceptCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void UpdateSourceShouldUseSourceValueInterceptor(int count)
        {
            Binding? binding = null;
            var targetObj = new object();
            var sourceObj = new object();
            object targetValue = 0;
            object sourceValue = 0;
            int targetGet = 0, targetSet = 0, sourceGet = 0, sourceSet = 0, interceptCount = 0;
            var targetMember = new TestAccessorMemberInfo
            {
                Type = typeof(object),
                GetValue = (o, context) =>
                {
                    ++targetGet;
                    o.ShouldEqual(targetObj);
                    context.ShouldEqual(binding);
                    return targetValue;
                },
                SetValue = (o, v, context) =>
                {
                    ++targetSet;
                    o.ShouldEqual(targetObj);
                    v.ShouldEqual(count);
                    context.ShouldEqual(binding);
                }
            };
            var sourceMember = new TestAccessorMemberInfo
            {
                Type = typeof(object),
                GetValue = (o, context) =>
                {
                    ++sourceGet;
                    o.ShouldEqual(sourceObj);
                    context.ShouldEqual(binding);
                    return sourceValue;
                },
                SetValue = (o, v, context) =>
                {
                    ++sourceSet;
                    o.ShouldEqual(sourceObj);
                    v.ShouldEqual(count);
                    context.ShouldEqual(binding);
                }
            };
            var target = new TestMemberPathObserver
            {
                GetLastMember = metadata => new MemberPathLastMember(targetObj, targetMember)
            };
            var source = new TestMemberPathObserver
            {
                GetLastMember = metadata => new MemberPathLastMember(sourceObj, sourceMember)
            };

            binding = GetBinding(target, source);
            for (var i = 0; i < count; i++)
            {
                var expected = i;
                var v = expected + 1;
                binding.AddComponent(new TestSourceValueInterceptorComponent
                {
                    Priority = -i,
                    InterceptSourceValue = (b, member, value, metadata) =>
                    {
                        ++interceptCount;
                        member.Target.ShouldEqual(sourceObj);
                        member.Member.ShouldEqual(sourceMember);
                        value.ShouldEqual(expected);
                        return v;
                    }
                });
            }

            binding.UpdateSource();
            targetGet.ShouldEqual(1);
            targetSet.ShouldEqual(0);
            sourceGet.ShouldEqual(0);
            sourceSet.ShouldEqual(1);
            interceptCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void UpdateTargetShouldNotifyListeners(int count)
        {
            object targetValue = "1";
            object sourceValue = "2";
            object? sourceObj = null;
            Exception? exception = null;
            var targetMember = new TestAccessorMemberInfo
            {
                Type = typeof(object),
                GetValue = (o, context) => targetValue,
                SetValue = (o, v, context) => { }
            };
            var sourceMember = new TestAccessorMemberInfo
            {
                Type = typeof(object),
                GetValue = (o, context) => sourceValue
            };
            var target = new TestMemberPathObserver
            {
                GetLastMember = metadata => new MemberPathLastMember(new object(), targetMember)
            };
            var source = new TestMemberPathObserver
            {
                GetLastMember = metadata =>
                {
                    if (exception != null)
                        return new MemberPathLastMember(exception);
                    if (sourceObj == null)
                        return default;
                    return new MemberPathLastMember(sourceObj, sourceMember);
                }
            };

            var updateFailed = 0;
            var updateCanceled = 0;
            var updated = 0;
            var binding = new Binding(target, source);
            for (var i = 0; i < count; i++)
            {
                binding.AddComponent(new TestBindingTargetListener
                {
                    OnTargetUpdateFailed = (b, e, m) =>
                    {
                        ++updateFailed;
                        b.ShouldEqual(binding);
                        e.ShouldEqual(exception);
                        m.ShouldEqual(binding);
                    },
                    OnTargetUpdateCanceled = (b, m) =>
                    {
                        ++updateCanceled;
                        b.ShouldEqual(binding);
                        m.ShouldEqual(binding);
                    },
                    OnTargetUpdated = (b, v, m) =>
                    {
                        ++updated;
                        b.ShouldEqual(binding);
                        v.ShouldEqual(sourceValue);
                        m.ShouldEqual(binding);
                    }
                });
            }

            binding.UpdateTarget();
            updateCanceled.ShouldEqual(count);
            updateFailed.ShouldEqual(0);
            updated.ShouldEqual(0);

            updateCanceled = updateFailed = updated = 0;
            sourceObj = new object();
            sourceValue = BindingMetadata.UnsetValue;
            binding.UpdateTarget();
            updateCanceled.ShouldEqual(count);
            updateFailed.ShouldEqual(0);
            updated.ShouldEqual(0);

            updateCanceled = updateFailed = updated = 0;
            sourceValue = new object();
            binding.UpdateTarget();
            updateCanceled.ShouldEqual(0);
            updateFailed.ShouldEqual(0);
            updated.ShouldEqual(count);

            updateCanceled = updateFailed = updated = 0;
            exception = new Exception();
            binding.UpdateTarget();
            updateCanceled.ShouldEqual(0);
            updateFailed.ShouldEqual(count);
            updated.ShouldEqual(0);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void UpdateTargetShouldUseTargetSetter(int count)
        {
            Binding? binding = null;
            var targetObj = new object();
            var sourceObj = new object();
            object targetValue = 0;
            object sourceValue = 0;
            int targetGet = 0, targetSet = 0, sourceGet = 0, sourceSet = 0, interceptCount = 0;
            var targetMember = new TestAccessorMemberInfo
            {
                Type = typeof(object),
                GetValue = (o, context) =>
                {
                    ++targetGet;
                    o.ShouldEqual(targetObj);
                    context.ShouldEqual(binding);
                    return targetValue;
                },
                SetValue = (o, v, context) =>
                {
                    ++targetSet;
                    o.ShouldEqual(targetObj);
                    v.ShouldEqual(count);
                    context.ShouldEqual(binding);
                }
            };
            var sourceMember = new TestAccessorMemberInfo
            {
                Type = typeof(object),
                GetValue = (o, context) =>
                {
                    ++sourceGet;
                    o.ShouldEqual(sourceObj);
                    context.ShouldEqual(binding);
                    return sourceValue;
                },
                SetValue = (o, v, context) =>
                {
                    ++sourceSet;
                    o.ShouldEqual(sourceObj);
                    v.ShouldEqual(count);
                    context.ShouldEqual(binding);
                }
            };
            var target = new TestMemberPathObserver
            {
                GetLastMember = metadata => new MemberPathLastMember(targetObj, targetMember)
            };
            var source = new TestMemberPathObserver
            {
                GetLastMember = metadata => new MemberPathLastMember(sourceObj, sourceMember)
            };

            binding = GetBinding(target, source);
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                binding.AddComponent(new TestTargetValueSetterComponent
                {
                    Priority = -i,
                    TrySetTargetValue = (b, member, value, metadata) =>
                    {
                        ++interceptCount;
                        member.Target.ShouldEqual(targetObj);
                        member.Member.ShouldEqual(targetMember);
                        value.ShouldEqual(sourceValue);
                        return isLast;
                    }
                });
            }

            binding.UpdateTarget();
            targetGet.ShouldEqual(0);
            targetSet.ShouldEqual(0);
            sourceGet.ShouldEqual(1);
            sourceSet.ShouldEqual(0);
            interceptCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void UpdateTargetShouldUseTargetValueInterceptor(int count)
        {
            Binding? binding = null;
            var targetObj = new object();
            var sourceObj = new object();
            object targetValue = 0;
            object sourceValue = 0;
            int targetGet = 0, targetSet = 0, sourceGet = 0, sourceSet = 0, interceptCount = 0;
            var targetMember = new TestAccessorMemberInfo
            {
                Type = typeof(object),
                GetValue = (o, context) =>
                {
                    ++targetGet;
                    o.ShouldEqual(targetObj);
                    context.ShouldEqual(binding);
                    return targetValue;
                },
                SetValue = (o, v, context) =>
                {
                    ++targetSet;
                    o.ShouldEqual(targetObj);
                    v.ShouldEqual(count);
                    context.ShouldEqual(binding);
                }
            };
            var sourceMember = new TestAccessorMemberInfo
            {
                Type = typeof(object),
                GetValue = (o, context) =>
                {
                    ++sourceGet;
                    o.ShouldEqual(sourceObj);
                    context.ShouldEqual(binding);
                    return sourceValue;
                },
                SetValue = (o, v, context) =>
                {
                    ++sourceSet;
                    o.ShouldEqual(sourceObj);
                    v.ShouldEqual(count);
                    context.ShouldEqual(binding);
                }
            };
            var target = new TestMemberPathObserver
            {
                GetLastMember = metadata => new MemberPathLastMember(targetObj, targetMember)
            };
            var source = new TestMemberPathObserver
            {
                GetLastMember = metadata => new MemberPathLastMember(sourceObj, sourceMember)
            };

            binding = GetBinding(target, source);
            for (var i = 0; i < count; i++)
            {
                var expected = i;
                var v = expected + 1;
                binding.AddComponent(new TestTargetValueInterceptorComponent
                {
                    Priority = -i,
                    InterceptTargetValue = (b, member, value, metadata) =>
                    {
                        ++interceptCount;
                        member.Target.ShouldEqual(targetObj);
                        member.Member.ShouldEqual(targetMember);
                        value.ShouldEqual(expected);
                        return v;
                    }
                });
            }

            binding.UpdateTarget();
            targetGet.ShouldEqual(0);
            targetSet.ShouldEqual(1);
            sourceGet.ShouldEqual(1);
            sourceSet.ShouldEqual(0);
            interceptCount.ShouldEqual(count);
        }

        [Fact]
        public void UpdateTargetSourceShouldUseObserverValue()
        {
            Binding? binding = null;
            var targetObj = new object();
            var sourceObj = new object();
            object targetValue = "1";
            object sourceValue = "2";
            var targetHasMember = true;
            var sourceHasMember = true;
            int targetGet = 0, targetSet = 0, sourceGet = 0, sourceSet = 0;
            var targetMember = new TestAccessorMemberInfo
            {
                Type = typeof(object),
                GetValue = (o, context) =>
                {
                    ++targetGet;
                    o.ShouldEqual(targetObj);
                    context.ShouldEqual(binding);
                    return targetValue;
                },
                SetValue = (o, v, context) =>
                {
                    ++targetSet;
                    o.ShouldEqual(targetObj);
                    v.ShouldEqual(sourceValue);
                    context.ShouldEqual(binding);
                }
            };
            var sourceMember = new TestAccessorMemberInfo
            {
                Type = typeof(object),
                GetValue = (o, context) =>
                {
                    ++sourceGet;
                    o.ShouldEqual(sourceObj);
                    context.ShouldEqual(binding);
                    return sourceValue;
                },
                SetValue = (o, v, context) =>
                {
                    ++sourceSet;
                    o.ShouldEqual(sourceObj);
                    v.ShouldEqual(targetValue);
                    context.ShouldEqual(binding);
                }
            };
            var target = new TestMemberPathObserver
            {
                GetLastMember = metadata =>
                {
                    metadata.ShouldEqual(binding);
                    if (targetHasMember)
                        return new MemberPathLastMember(targetObj, targetMember);
                    return default;
                }
            };
            var source = new TestMemberPathObserver
            {
                GetLastMember = metadata =>
                {
                    metadata.ShouldEqual(binding);
                    if (sourceHasMember)
                        return new MemberPathLastMember(sourceObj, sourceMember);
                    return default;
                }
            };

            binding = GetBinding(target, source);
            binding.UpdateTarget();
            targetGet.ShouldEqual(0);
            targetSet.ShouldEqual(1);
            sourceGet.ShouldEqual(1);
            sourceSet.ShouldEqual(0);

            targetGet = targetSet = sourceGet = sourceSet = 0;
            targetHasMember = false;
            binding.UpdateTarget();
            targetGet.ShouldEqual(0);
            targetSet.ShouldEqual(0);
            sourceGet.ShouldEqual(0);
            sourceSet.ShouldEqual(0);

            targetGet = targetSet = sourceGet = sourceSet = 0;
            targetHasMember = true;
            sourceValue = BindingMetadata.UnsetValue;
            binding.UpdateTarget();
            targetGet.ShouldEqual(0);
            targetSet.ShouldEqual(0);
            sourceGet.ShouldEqual(1);
            sourceSet.ShouldEqual(0);

            targetGet = targetSet = sourceGet = sourceSet = 0;
            sourceValue = "2";
            binding.UpdateSource();
            targetGet.ShouldEqual(1);
            targetSet.ShouldEqual(0);
            sourceGet.ShouldEqual(0);
            sourceSet.ShouldEqual(1);

            targetGet = targetSet = sourceGet = sourceSet = 0;
            sourceHasMember = false;
            binding.UpdateSource();
            targetGet.ShouldEqual(0);
            targetSet.ShouldEqual(0);
            sourceGet.ShouldEqual(0);
            sourceSet.ShouldEqual(0);

            targetGet = targetSet = sourceGet = sourceSet = 0;
            sourceHasMember = true;
            targetValue = BindingMetadata.UnsetValue;
            binding.UpdateSource();
            targetGet.ShouldEqual(1);
            targetSet.ShouldEqual(0);
            sourceGet.ShouldEqual(0);
            sourceSet.ShouldEqual(0);
        }

        protected override IBindingManager GetBindingManager() => new BindingManager(ComponentCollectionManager);

        protected virtual Binding GetBinding(IMemberPathObserver? target = null, object? source = null) => new(target ?? EmptyPathObserver.Empty, source);

        protected override Binding GetComponentOwner(IComponentCollectionManager? componentCollectionManager = null) => GetBinding();
    }
}