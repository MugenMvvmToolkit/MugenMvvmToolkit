using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Models;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest
{
    public abstract class HasEventListenerUnitTestBase<T> : UnitTestBase
        where T : class, IListener
    {
        #region Methods

        [Fact]
        public virtual void AddListenerShouldAddListener()
        {
            var listener = CreateListener();
            var hasEventListener = GetHasEventListener();
            hasEventListener.AddListener(listener);
            hasEventListener.GetListeners().ShouldContain(listener);
        }

        [Fact]
        public virtual void RemoveListenerShouldRemoveListener()
        {
            var listener = CreateListener();
            var hasEventListener = GetHasEventListener();
            hasEventListener.AddListener(listener);
            hasEventListener.GetListeners().ShouldContain(listener);

            hasEventListener.RemoveListener(listener);
        }

        [Fact]
        public virtual void TargetShouldUpdateListeners()
        {
            var eventListener = GetHasEventListener();
            var listeners = new List<T>();
            for (var i = 0; i < 100; i++)
                listeners.Add(CreateListener());

            for (var i = 0; i < listeners.Count; i++)
                eventListener.AddListener(listeners[i]);

            for (var i = 0; i < listeners.Count / 2; i++)
            {
                eventListener.RemoveListener(listeners[i]);
                listeners.RemoveAt(0);
                --i;
            }

            var list = eventListener.GetListeners();
            foreach (var listener in listeners)
                list.ShouldContain(listener);
        }

        [Fact]
        public virtual void TargetShouldUpdateListenersParallel()
        {
            var eventListener = GetHasEventListener();
            var listeners = new List<T>();
            for (var i = 0; i < 100; i++)
                listeners.Add(CreateListener());

            var tasks = new List<Task>();
            for (var i = 0; i < listeners.Count; i++)
            {
                var index = i;
                tasks.Add(Task.Run(() => eventListener.AddListener(listeners[index])));
            }

            Task.WaitAll(tasks.ToArray());
            tasks.Clear();

            for (var i = 0; i < listeners.Count / 2; i++)
            {
                var i1 = i;
                tasks.Add(Task.Run(() => eventListener.RemoveListener(listeners[i1])));
            }

            var list = eventListener.GetListeners();
            foreach (var listener in listeners.Skip(listeners.Count / 2))
                list.ShouldContain(listener);
        }

        [Fact]
        public virtual void RemoveAllListenersShouldRemoveAllListeners()
        {
            var listener1 = CreateListener();
            var listener2 = CreateListener();
            var hasEventListener = GetHasEventListener();
            hasEventListener.AddListener(listener1);
            hasEventListener.AddListener(listener2);
            hasEventListener.GetListeners().ShouldContain(listener1, listener2);

            hasEventListener.RemoveAllListeners();
            hasEventListener.GetListeners().ShouldBeEmpty();
        }

        protected abstract T CreateListener();

        protected abstract IHasListeners<T> GetHasEventListener();

        #endregion
    }
}