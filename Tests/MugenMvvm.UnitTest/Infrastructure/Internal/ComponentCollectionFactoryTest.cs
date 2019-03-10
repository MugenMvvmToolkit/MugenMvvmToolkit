using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Infrastructure.Internal;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Models;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Infrastructure.Internal
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

            objects.Sort((listener, listener1) => listener1.GetPriority(this).CompareTo(listener.GetPriority(this)));

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

            objects.Sort((listener, listener1) => listener1.GetPriority(this).CompareTo(listener.GetPriority(this)));

            componentCollection.HasItems.ShouldBeTrue();
            componentCollection.GetItems().SequenceEqual(objects).ShouldBeTrue();

            componentCollection.Clear();
            componentCollection.HasItems.ShouldBeFalse();
            componentCollection.GetItems().ShouldBeEmpty();
        }

        protected IComponentCollectionFactory CreateFactory()
        {
            return new ComponentCollectionFactory();
        }

        #endregion

        #region Nested types

        private class Listener : IListener
        {
            #region Properties

            public Func<object, int> GetPriority { get; set; }

            #endregion

            #region Implementation of interfaces

            int IListener.GetPriority(object source)
            {
                return GetPriority(source);
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