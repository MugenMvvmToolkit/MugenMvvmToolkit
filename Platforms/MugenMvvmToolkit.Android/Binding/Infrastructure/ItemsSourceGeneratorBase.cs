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
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
#if ANDROID
using Android.Content;
using MugenMvvmToolkit.Interfaces.Views;
#elif WINFORMS
using System.ComponentModel;
#endif

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    public abstract class ItemsSourceGeneratorBase : IItemsSourceGenerator
    {
        #region Fields

        private readonly NotifyCollectionChangedEventHandler _handler;
        protected internal const string Key = "#ItemsSourceGeneratorMember";

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemsSourceGeneratorBase"/> class.
        /// </summary>
        protected ItemsSourceGeneratorBase()
        {
            _handler = ReflectionExtensions.MakeWeakCollectionChangedHandler(this, (@base, o, arg3) => @base.OnCollectionChanged(arg3));
        }

        #endregion

        #region Properites

        /// <summary>
        ///     Gets the current items source, if any.
        /// </summary>
        protected abstract IEnumerable ItemsSource { get; set; }

        /// <summary>
        ///     Gets the current state of the target.
        /// </summary>
        protected abstract bool IsTargetDisposed { get; }

        #endregion

        #region Methods

        [CanBeNull]
        public static IItemsSourceGenerator Get(object item, string key = null)
        {
            return ServiceProvider.AttachedValueProvider.GetValue<IItemsSourceGenerator>(item, key ?? Key, false);
        }

        protected virtual void Update(IEnumerable itemsSource, IDataContext context = null)
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
                activityView.Mediator.Destroyed += OnTargetDisposed;
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
                activityView.Mediator.Destroyed -= OnTargetDisposed;
#elif WINFORMS
            ((IComponent)sender).Disposed -= OnTargetDisposed;
#endif
            var collectionChanged = ItemsSource as INotifyCollectionChanged;
            if (collectionChanged != null)
                collectionChanged.CollectionChanged -= _handler;
            ItemsSource = null;
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (IsTargetDisposed)
            {
                OnTargetDisposed(null, args);
                return;
            }
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

        protected abstract void Refresh();

        #endregion

        #region Implementation of IItemsSourceGenerator

        /// <summary>
        ///     Gets the current items source, if any.
        /// </summary>
        IEnumerable IItemsSourceGenerator.ItemsSource
        {
            get { return ItemsSource; }
        }

        /// <summary>
        ///     Sets the current items source.
        /// </summary>
        void IItemsSourceGenerator.SetItemsSource(IEnumerable itemsSource, IDataContext context)
        {
            Update(itemsSource, context);
        }

        void IItemsSourceGenerator.Reset()
        {
            Refresh();
        }

        #endregion
    }
}