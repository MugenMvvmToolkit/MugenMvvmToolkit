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
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Utils;

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
            private object _weakItem;

            #endregion

            #region Constructors

            public Unsubscriber(EventListenerList eventListener, object weakItem)
            {
                _eventListener = eventListener;
                _weakItem = weakItem;
            }

            #endregion

            #region Implementation of IDisposable

            public void Dispose()
            {
                var listener = _eventListener;
                var weakItem = _weakItem;
                if (listener != null && weakItem != null)
                {
                    _eventListener = null;
                    _weakItem = null;
                    listener.Remove(weakItem);
                }
            }

            #endregion
        }

        #endregion

        #region Fields

        //Use an array to reduce the cost of memory and do not lock during a call event.
        protected object[] Listeners;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="EventListenerList" /> class.
        /// </summary>
        public EventListenerList()
        {
            Listeners = EmptyValue<object>.ArrayInstance;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     This method can be used to raise the event handler.
        /// </summary>
        public void Raise<TArg>(object sender, TArg args)
        {
            bool hasDeadRef = false;
            object[] listeners = Listeners;
            for (int i = 0; i < listeners.Length; i++)
            {
                IEventListener listener = BindingExtensions.GetEventListenerFromWeakItem(listeners[i]);
                if (listener == null)
                    hasDeadRef = true;
                else
                    listener.Handle(sender, args);
            }
            if (hasDeadRef)
            {
                //it's normal here.
                lock (this)
                    Update(null);
            }
        }

        /// <summary>
        /// Adds a listener without unsubscriber
        /// </summary>
        public void Add(IEventListener target)
        {
            AddInternal(target.ToWeakItem(), false);
        }

        /// <summary>
        /// Adds a listener with unsubscriber
        /// </summary>
        public IDisposable AddWithUnsubscriber(IEventListener target)
        {
            return AddInternal(target.ToWeakItem(), true);
        }

        /// <summary>
        /// Removes a listener by weak item.
        /// </summary>
        public void Remove(object weakItem)
        {
            //it's normal here.
            lock (this)
            {
                for (int i = 0; i < Listeners.Length; i++)
                {
                    if (ReferenceEquals(Listeners[i], weakItem))
                    {
                        Listeners[i] = MvvmUtils.EmptyWeakReference;
                        Update(null);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Clears the current collection.
        /// </summary>
        public void Clear()
        {
            Listeners = EmptyValue<object>.ArrayInstance;
        }

        /// <summary>
        /// Adds a weak item to list.
        /// </summary>
        protected IDisposable AddInternal(object weakItem, bool withUnsubscriber)
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
        /// Updates the current list.
        /// </summary>
        protected void Update(object newItem)
        {
            object[] references = newItem == null
                ? new object[Listeners.Length]
                : new object[Listeners.Length + 1];
            int index = 0;
            for (int i = 0; i < Listeners.Length; i++)
            {
                object reference = Listeners[i];
                if (BindingExtensions.GetEventListenerFromWeakItem(reference) != null)
                    references[index++] = reference;
            }
            if (newItem != null)
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
        /// Occurs on add a listener.
        /// </summary>
        protected virtual bool OnAdd(object weakItem, bool withUnsubscriber, out IDisposable unsubscriber)
        {
            unsubscriber = null;
            return false;
        }

        /// <summary>
        /// Occurs on empty collection.
        /// </summary>
        protected virtual void OnEmpty()
        {
            Listeners = EmptyValue<object>.ArrayInstance;
        }

        #endregion
    }
}