#region Copyright

// ****************************************************************************
// <copyright file="EventListenerList.cs">
// Copyright © Vyacheslav Volkov 2012-2014
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    /// <summary>
    ///     Represents the weak collection of <see cref="IEventListener" />.
    /// </summary>
    public class EventListenerList
    {
        #region Nested types

        internal sealed class Unsubscriber : IDisposable
        {
            #region Fields

            private EventListenerList _eventListener;
            private WeakEventListenerWrapper _weakItem;

            #endregion

            #region Constructors

            public Unsubscriber(EventListenerList eventListener, WeakEventListenerWrapper weakItem)
            {
                _eventListener = eventListener;
                _weakItem = weakItem;
            }

            #endregion

            #region Implementation of IDisposable

            public void Dispose()
            {
                EventListenerList listener = _eventListener;
                var weakItem = _weakItem;
                if (listener != null && !weakItem.IsEmpty)
                {
                    _eventListener = null;
                    _weakItem = WeakEventListenerWrapper.Empty;
                    listener.Remove(weakItem);
                }
            }

            #endregion
        }

        #endregion

        #region Fields

        //Use an array to reduce the cost of memory and do not lock during a call event.
        protected WeakEventListenerWrapper[] Listeners;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="EventListenerList" /> class.
        /// </summary>
        public EventListenerList()
        {
            Listeners = Empty.Array<WeakEventListenerWrapper>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets or adds an instance of <see cref="EventListenerList"/> using the attached member path.
        /// </summary>
        public static EventListenerList GetOrAdd(object item, string path)
        {
            return ServiceProvider.AttachedValueProvider.GetOrAdd(item, path, (o, o1) => new EventListenerList(), null);
        }

        /// <summary>
        ///     This method can be used to raise the event handler using the attached member path.
        /// </summary>
        public static void Raise(object item, string path, object message)
        {
            var list = ServiceProvider.AttachedValueProvider.GetValue<EventListenerList>(item, path, false);
            if (list != null)
                list.Raise(item, message);
        }

        /// <summary>
        ///     This method can be used to raise the event handler.
        /// </summary>
        public void Raise<TArg>(object sender, TArg args)
        {
            bool hasDeadRef = false;
            WeakEventListenerWrapper[] listeners = Listeners;
            for (int i = 0; i < listeners.Length; i++)
            {
                if (!listeners[i].EventListener.TryHandle(sender, args))
                    hasDeadRef = true;
            }
            if (hasDeadRef)
            {
                //it's normal here.
                lock (this)
                    Update(WeakEventListenerWrapper.Empty);
            }
        }

        /// <summary>
        ///     Adds a listener without unsubscriber
        /// </summary>
        public void Add(IEventListener target)
        {
            AddInternal(target.ToWeakWrapper(), false);
        }

        /// <summary>
        ///     Adds a listener with unsubscriber
        /// </summary>
        public IDisposable AddWithUnsubscriber(IEventListener target)
        {
            return AddInternal(target.ToWeakWrapper(), true);
        }

        /// <summary>
        ///     Removes a listener.
        /// </summary>
        public void Remove(IEventListener listener)
        {
            if (listener.IsWeak)
            {
                Remove(listener.ToWeakWrapper());
                return;
            }
            //it's normal here.
            lock (this)
            {
                for (int i = 0; i < Listeners.Length; i++)
                {
                    if (ReferenceEquals(Listeners[i].EventListener, listener))
                    {
                        Listeners[i] = WeakEventListenerWrapper.Empty;
                        Update(WeakEventListenerWrapper.Empty);
                        return;
                    }
                }
            }
        }

        /// <summary>
        ///     Clears the current collection.
        /// </summary>
        public void Clear()
        {
            Listeners = Empty.Array<WeakEventListenerWrapper>();
        }

        /// <summary>
        ///     Adds a weak item to list.
        /// </summary>
        protected IDisposable AddInternal(WeakEventListenerWrapper weakItem, bool withUnsubscriber)
        {
            //it's normal here.
            lock (this)
            {
                IDisposable disposable;
                if (OnAdd(weakItem, withUnsubscriber, out disposable))
                    return disposable;
                if (Listeners.Length == 0)
                    Listeners = new[] { weakItem };
                else
                    Update(weakItem);
            }
            if (withUnsubscriber)
                return new Unsubscriber(this, weakItem);
            return null;
        }

        /// <summary>
        ///     Updates the current list.
        /// </summary>
        protected void Update(WeakEventListenerWrapper newItem)
        {
            WeakEventListenerWrapper[] references = newItem.IsEmpty
                ? new WeakEventListenerWrapper[Listeners.Length]
                : new WeakEventListenerWrapper[Listeners.Length + 1];
            int index = 0;
            for (int i = 0; i < Listeners.Length; i++)
            {
                WeakEventListenerWrapper reference = Listeners[i];
                if (reference.EventListener.IsAlive)
                    references[index++] = reference;
            }
            if (!newItem.IsEmpty)
            {
                references[index] = newItem;
                index++;
            }
            else if (index == 0)
            {
                OnEmpty();
                return;
            }
            if (references.Length != index)
                Array.Resize(ref references, index);
            Listeners = references;
        }

        /// <summary>
        ///     Occurs on add a listener.
        /// </summary>
        protected virtual bool OnAdd(WeakEventListenerWrapper weakItem, bool withUnsubscriber, out IDisposable unsubscriber)
        {
            unsubscriber = null;
            return false;
        }

        /// <summary>
        ///     Occurs on empty collection.
        /// </summary>
        protected virtual void OnEmpty()
        {
            Listeners = Empty.Array<WeakEventListenerWrapper>();
        }

        private void Remove(WeakEventListenerWrapper weakItem)
        {
            //it's normal here.
            lock (this)
            {
                for (int i = 0; i < Listeners.Length; i++)
                {
                    if (ReferenceEquals(Listeners[i].Source, weakItem.Source))
                    {
                        Listeners[i] = WeakEventListenerWrapper.Empty;
                        Update(WeakEventListenerWrapper.Empty);
                        return;
                    }
                }
            }
        }

        #endregion
    }
}