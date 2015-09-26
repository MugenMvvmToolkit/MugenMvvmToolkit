#region Copyright

// ****************************************************************************
// <copyright file="ItemsSourceGeneratorBase.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
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
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Interfaces.Models;

#if ANDROID
using Android.Content;
using MugenMvvmToolkit.Android.Binding.Interfaces;
using MugenMvvmToolkit.Android.Interfaces.Views;

namespace MugenMvvmToolkit.Android.Binding.Infrastructure
#elif WINFORMS
using System.ComponentModel;
using MugenMvvmToolkit.WinForms.Binding.Interfaces;

namespace MugenMvvmToolkit.WinForms.Binding.Infrastructure
#elif TOUCH
using ObjCRuntime;
using MugenMvvmToolkit.iOS.Binding.Interfaces;
using MugenMvvmToolkit.iOS.Interfaces.Views;

namespace MugenMvvmToolkit.iOS.Binding.Infrastructure
#elif SILVERLIGHT
using MugenMvvmToolkit.Silverlight.Binding.Interfaces;

namespace MugenMvvmToolkit.Silverlight.Binding.Infrastructure
#endif
{
    public abstract class ItemsSourceGeneratorBase : IItemsSourceGenerator
    {
        #region Fields

        public static readonly BindingMemberDescriptor<object, IItemsSourceGenerator> MemberDescriptor;
        private readonly NotifyCollectionChangedEventHandler _handler;
        private ReflectionExtensions.IWeakEventHandler<EventArgs> _listener;

        #endregion

        #region Constructors

        static ItemsSourceGeneratorBase()
        {
            MemberDescriptor = new BindingMemberDescriptor<object, IItemsSourceGenerator>("ItemsSourceGenerator");
            BindingServiceProvider.BindingMemberPriorities[MemberDescriptor] = 2;
        }

        protected ItemsSourceGeneratorBase()
        {
            _handler = ReflectionExtensions.MakeWeakCollectionChangedHandler(this, (@base, o, arg3) => @base.OnCollectionChanged(arg3));
        }

        #endregion

        #region Properites

        protected abstract IEnumerable ItemsSource { get; set; }

        protected abstract bool IsTargetDisposed { get; }

        #endregion

        #region Methods

        private void InitializeListener()
        {
            _listener = ReflectionExtensions.CreateWeakEventHandler<ItemsSourceGeneratorBase, EventArgs>(this, (@base, o, arg3) => @base.OnTargetDisposed(o, arg3));
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
            {
                InitializeListener();
                activityView.Mediator.Destroyed += _listener.Handle;
            }
        }
#elif WINFORMS
        protected void ListenDisposeEvent(IComponent component)
        {
            if (component != null)
            {
                InitializeListener();
                component.Disposed += _listener.Handle;
            }
        }
#elif TOUCH
        protected void TryListenController(INativeObject item)
        {
            var viewControllerView = item.FindParent<IViewControllerView>();
            if (viewControllerView != null)
            {
                InitializeListener();
                viewControllerView.Mediator.DisposeHandler += _listener.Handle;
            }
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
            if (_listener != null)
            {
#if ANDROID
                var activityView = sender as IActivityView;
                if (activityView != null)
                    activityView.Mediator.Destroyed -= _listener.Handle;
#elif WINFORMS
                var component = sender as IComponent;
                if (component != null)
                    component.Disposed -= _listener.Handle;
#elif TOUCH
                var controllerView = sender as IViewControllerView;
                if (controllerView != null)
                    controllerView.Mediator.DisposeHandler -= _listener.Handle;
#endif
            }
            var collectionChanged = ItemsSource as INotifyCollectionChanged;
            if (collectionChanged != null)
                collectionChanged.CollectionChanged -= _handler;
            ItemsSource = null;
            _listener = null;
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

        IEnumerable IItemsSourceGenerator.ItemsSource
        {
            get { return ItemsSource; }
        }

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
