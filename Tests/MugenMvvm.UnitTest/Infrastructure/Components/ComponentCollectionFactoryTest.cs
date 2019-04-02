using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Infrastructure.Components;
using MugenMvvm.Infrastructure.Internal;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Infrastructure.Components
{
    public class ComponentCollectionFactoryTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ComponentCollectionFactoryShouldCreateArrayTargetNoPriority()
        {
            var componentCollectionFactory = CreateFactory();
            var componentCollection = componentCollectionFactory.GetComponentCollection<object>(this, Default.MetadataContext);

            componentCollection.HasItems.ShouldBeFalse();

            var objects = new List<object>();
            for (var i = 0; i < 100; i++)
            {
                var o = new object();
                objects.Add(o);
                componentCollection.Add(o);
            }

            componentCollection.HasItems.ShouldBeTrue();
            componentCollection.GetItems().SequenceEqual(objects).ShouldBeTrue();

            var array = objects.ToArray();
            for (var i = 0; i < array.Length; i++)
            {
                if (i % 2 == 0)
                {
                    objects.Remove(array[i]);
                    componentCollection.Remove(array[i]);
                }
            }

            componentCollection.HasItems.ShouldBeTrue();
            componentCollection.GetItems().SequenceEqual(objects).ShouldBeTrue();

            componentCollection.Clear();
            componentCollection.HasItems.ShouldBeFalse();
            componentCollection.GetItems().ShouldBeEmpty();
        }

        [Fact]
        public void ComponentCollectionFactoryShouldCreateOrderedArrayTargetHasPriority()
        {
            var componentCollectionFactory = CreateFactory();
            var componentCollection = componentCollectionFactory.GetComponentCollection<HasPriority>(this, Default.MetadataContext);

            componentCollection.HasItems.ShouldBeFalse();

            var objects = new List<HasPriority>();
            for (var i = 0; i < 100; i++)
            {
                var o = new HasPriority {Priority = Guid.NewGuid().GetHashCode()};
                objects.Add(o);
                componentCollection.Add(o);
            }

            objects.Sort(HasPriorityComparer.Instance);

            componentCollection.HasItems.ShouldBeTrue();
            componentCollection.GetItems().SequenceEqual(objects).ShouldBeTrue();

            var array = objects.ToArray();
            for (var i = 0; i < array.Length; i++)
            {
                if (i % 2 == 0)
                {
                    objects.Remove(array[i]);
                    componentCollection.Remove(array[i]);
                }
            }

            objects.Sort(HasPriorityComparer.Instance);

            componentCollection.HasItems.ShouldBeTrue();
            componentCollection.GetItems().SequenceEqual(objects).ShouldBeTrue();

            componentCollection.Clear();
            componentCollection.HasItems.ShouldBeFalse();
            componentCollection.GetItems().ShouldBeEmpty();
        }

        [Fact]
        public void ComponentCollectionFactoryShouldCreateOrderedArrayTargetListener()
        {
            var componentCollectionFactory = CreateFactory();
            var componentCollection = componentCollectionFactory.GetComponentCollection<Listener>(this, Default.MetadataContext);

            componentCollection.HasItems.ShouldBeFalse();

            var objects = new List<Listener>();
            for (var i = 0; i < 100; i++)
            {
                var priority = Guid.NewGuid().GetHashCode();
                var o = new Listener
                {
                    GetPriority = o1 =>
                    {
                        o1.ShouldEqual(this);
                        return priority;
                    }
                };
                objects.Add(o);
                componentCollection.Add(o);
            }

            objects.Sort((listener, listener1) => listener1.GetPriority!(this).CompareTo(listener.GetPriority!(this)));

            componentCollection.HasItems.ShouldBeTrue();
            componentCollection.GetItems().SequenceEqual(objects).ShouldBeTrue();

            var array = objects.ToArray();
            for (var i = 0; i < array.Length; i++)
            {
                if (i % 2 == 0)
                {
                    objects.Remove(array[i]);
                    componentCollection.Remove(array[i]);
                }
            }

            objects.Sort((listener, listener1) => listener1.GetPriority!(this).CompareTo(listener.GetPriority!(this)));

            componentCollection.HasItems.ShouldBeTrue();
            componentCollection.GetItems().SequenceEqual(objects).ShouldBeTrue();

            componentCollection.Clear();
            componentCollection.HasItems.ShouldBeFalse();
            componentCollection.GetItems().ShouldBeEmpty();
        }

        [Fact]
        public void ComponentCollectionFactoryShouldAttachDetachComponent1()
        {
            var attached = 0;
            var detached = 0;
            var item = new AttachableDetachable<object>();
            var componentCollectionFactory = CreateFactory();
            var componentCollection = componentCollectionFactory.GetComponentCollection<object>(this, Default.MetadataContext);
            item.OnAttached = (o, context) =>
            {
                o.ShouldEqual(this);
                context.ShouldNotBeNull();
                attached++;
            };

            item.OnDetached = (o, context) =>
            {
                o.ShouldEqual(this);
                context.ShouldNotBeNull();
                detached++;
            };

            componentCollection.Add(item);
            attached.ShouldEqual(1);
            detached.ShouldEqual(0);

            componentCollection.Remove(item);
            attached.ShouldEqual(1);
            detached.ShouldEqual(1);
        }

        [Fact]
        public void ComponentCollectionFactoryShouldAttachDetachComponent2()
        {
            var attached = 0;
            var detached = 0;
            var item = new AttachableDetachable<ComponentCollectionFactoryTest>();
            var componentCollectionFactory = CreateFactory();
            var componentCollection = componentCollectionFactory.GetComponentCollection<object>(this, Default.MetadataContext);
            item.OnAttached = (o, context) =>
            {
                o.ShouldEqual(this);
                context.ShouldNotBeNull();
                attached++;
            };

            item.OnDetached = (o, context) =>
            {
                o.ShouldEqual(this);
                context.ShouldNotBeNull();
                detached++;
            };

            componentCollection.Add(item);
            attached.ShouldEqual(1);
            detached.ShouldEqual(0);

            componentCollection.Remove(item);
            attached.ShouldEqual(1);
            detached.ShouldEqual(1);
        }

        [Fact]
        public void ComponentCollectionFactoryShouldAttachDetachComponent3()
        {
            var attached = 0;
            var detached = 0;
            var item = new AttachableDetachable<ComponentCollectionFactory>();
            var componentCollectionFactory = CreateFactory();
            var componentCollection = componentCollectionFactory.GetComponentCollection<object>(this, Default.MetadataContext);
            item.OnAttached = (o, context) => { attached++; };

            item.OnDetached = (o, context) => { detached++; };

            componentCollection.Add(item);
            attached.ShouldEqual(0);
            detached.ShouldEqual(0);

            componentCollection.Remove(item);
            attached.ShouldEqual(0);
            detached.ShouldEqual(0);
        }

        protected IComponentCollectionFactory CreateFactory()
        {
            return new ComponentCollectionFactory();
        }

        #endregion

        #region Nested types

        private class AttachableDetachable<T> : IAttachableComponent<T>, IDetachableComponent<T> where T : class
        {
            #region Properties

            public Action<T, IReadOnlyMetadataContext>? OnAttached { get; set; }

            public Action<T, IReadOnlyMetadataContext>? OnDetached { get; set; }

            #endregion

            #region Implementation of interfaces

            void IAttachableComponent<T>.OnAttached(T owner, IReadOnlyMetadataContext metadata)
            {
                OnAttached?.Invoke(owner, metadata);
            }

            void IDetachableComponent<T>.OnDetached(T owner, IReadOnlyMetadataContext metadata)
            {
                OnDetached?.Invoke(owner, metadata);
            }

            #endregion
        }

        private class Listener : IListener
        {
            #region Properties

            public Func<object, int>? GetPriority { get; set; }

            #endregion

            #region Implementation of interfaces

            int IListener.GetPriority(object source)
            {
                return GetPriority!(source);
            }

            #endregion
        }

        private sealed class HasPriority : IHasPriority
        {
            #region Properties

            public int Priority { get; set; }

            #endregion
        }

        #endregion
    }
}