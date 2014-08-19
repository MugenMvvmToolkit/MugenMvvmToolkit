#region Copyright
// ****************************************************************************
// <copyright file="ItemsSourceGeneratorBase.cs">
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
using System.Collections;
using System.Collections.Specialized;
#if ANDROID
using Android.Content;
using MugenMvvmToolkit.Interfaces.Views;
#elif WINFORMS
using System.ComponentModel;
#endif

namespace MugenMvvmToolkit.Infrastructure
{
    public abstract class ItemsSourceGeneratorBase
    {
        #region Fields

        private readonly NotifyCollectionChangedEventHandler _handler;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemsSourceGeneratorBase"/> class.
        /// </summary>
        protected ItemsSourceGeneratorBase()
        {
            _handler = PlatformExtensions.MakeWeakCollectionChangedHandler(this, (@base, o, arg3) => @base.ItemsSourceOnCollectionChanged(arg3));
        }

        #endregion

        #region Properites

        protected abstract IEnumerable ItemsSource { get; set; }

        #endregion

        #region Methods

        public virtual void Reset()
        {
            Refresh();
        }

        public virtual void Update(IEnumerable itemsSource)
        {
            if (ReferenceEquals(itemsSource, ItemsSource))
                return;
            var collectionChanged = ItemsSource as INotifyCollectionChanged;
            if (collectionChanged != null)
                collectionChanged.CollectionChanged -= _handler;
            ItemsSource = itemsSource;
            Refresh();
            collectionChanged = itemsSource as INotifyCollectionChanged;
            if (collectionChanged != null)
                collectionChanged.CollectionChanged += _handler;
        }

#if ANDROID
        protected void TryListenActivity(Context context)
        {
            var activity = context.GetActivity();
            var activityView = activity as IActivityView;
            if (activityView == null)
                Tracer.Warn("{0} - The type {1} does not implement the IActivityView.", this, context);
            else
                activityView.Destroyed += OnTargetDisposed;
        }
#elif WINFORMS
        protected void ListenDisposeEvent(IComponent component)
        {
            if (component != null)
                component.Disposed += OnTargetDisposed;
        }
#endif
        protected object GetItem(int position)
        {
            var itemsSource = ItemsSource;
            if (itemsSource == null)
                return null;
            return itemsSource.ElementAtIndex(position);
        }

        protected virtual void OnTargetDisposed(object sender, EventArgs e)
        {
#if ANDROID
            var activityView = sender as IActivityView;
            if (activityView != null)
                activityView.Destroyed -= OnTargetDisposed;
#elif WINFORMS
            ((IComponent)sender).Disposed -= OnTargetDisposed;
#endif

            var collectionChanged = ItemsSource as INotifyCollectionChanged;
            if (collectionChanged != null)
                collectionChanged.CollectionChanged -= _handler;
            ItemsSource = null;
        }

        private void ItemsSourceOnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Add(args.NewStartingIndex, args.NewItems.Count);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Remove(args.OldStartingIndex, args.OldItems.Count);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (args.NewItems.Count == args.OldItems.Count)
                        Replace(args.NewStartingIndex, args.NewItems.Count);
                    else
                        Refresh();
                    break;
                default:
                    Refresh();
                    break;
            }
        }

        protected abstract void Add(int insertionIndex, int count);

        protected abstract void Remove(int removalIndex, int count);

        protected abstract void Replace(int startIndex, int count);

        //TODO OPTIMIZE: CHECK OLD ITEMS
        protected abstract void Refresh();

        #endregion
    }
}