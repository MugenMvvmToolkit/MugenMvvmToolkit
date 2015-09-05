#region Copyright

// ****************************************************************************
// <copyright file="ItemsSourceRecyclerAdapter.cs">
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
using Android.App;
using Android.Views;
using Android.Widget;
using JetBrains.Annotations;
using MugenMvvmToolkit.Android.Binding.Infrastructure;
using MugenMvvmToolkit.Android.Interfaces.Views;
using MugenMvvmToolkit.Binding;
using Object = Java.Lang.Object;

namespace MugenMvvmToolkit.Android.RecyclerView.Infrastructure
{
    public class ItemsSourceRecyclerAdapter : global::Android.Support.V7.Widget.RecyclerView.Adapter
    {
        #region Nested types

        private sealed class ViewHolderImpl : global::Android.Support.V7.Widget.RecyclerView.ViewHolder
        {
            #region Constructors

            public ViewHolderImpl(View itemView)
                : base(itemView)
            {
            }

            #endregion
        }

        #endregion

        #region Fields

        private readonly DataTemplateProvider _itemTemplateProvider;
        private readonly BindableLayoutInflater _layoutInflater;
        private readonly global::Android.Support.V7.Widget.RecyclerView _recyclerView;
        private readonly NotifyCollectionChangedEventHandler _weakHandler;
        private readonly ReflectionExtensions.IWeakEventHandler<EventArgs> _listener;
        private IEnumerable _itemsSource;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ItemsSourceAdapter" /> class.
        /// </summary>
        public ItemsSourceRecyclerAdapter([NotNull] global::Android.Support.V7.Widget.RecyclerView recyclerView)
        {
            Should.NotBeNull(recyclerView, "recyclerView");
            _recyclerView = recyclerView;
            _itemTemplateProvider = new DataTemplateProvider(_recyclerView, AttachedMemberConstants.ItemTemplate,
                AttachedMemberConstants.ItemTemplateSelector);
            _layoutInflater = _recyclerView.Context.GetBindableLayoutInflater();
            _weakHandler = ReflectionExtensions.MakeWeakCollectionChangedHandler(this,
                (adapter, o, arg3) => adapter.OnCollectionChanged(o, arg3));
            var activityView = _recyclerView.Context.GetActivity() as IActivityView;
            if (activityView != null)
            {
                _listener = ReflectionExtensions.CreateWeakEventHandler<ItemsSourceRecyclerAdapter, EventArgs>(this, (adapter, o, arg3) => adapter.ActivityViewOnDestroyed((Activity)o));
                activityView.Mediator.Destroyed += _listener.Handle;
            }
        }

        #endregion

        #region Properties

        protected global::Android.Support.V7.Widget.RecyclerView RecyclerView
        {
            get { return _recyclerView; }
        }

        protected BindableLayoutInflater LayoutInflater
        {
            get { return _layoutInflater; }
        }

        /// <summary>
        ///     Gets or sets the items source.
        /// </summary>
        public virtual IEnumerable ItemsSource
        {
            get { return _itemsSource; }
            set { SetItemsSource(value, true); }
        }

        #endregion

        #region Methods

        public virtual object GetRawItem(int position)
        {
            if (position < 0)
                return null;
            return ItemsSource.ElementAtIndex(position);
        }

        protected virtual void SetItemsSource(IEnumerable value, bool notifyDataSet)
        {
            if (ReferenceEquals(value, _itemsSource))
                return;
            if (_weakHandler == null)
                _itemsSource = value;
            else
            {
                var notifyCollectionChanged = _itemsSource as INotifyCollectionChanged;
                if (notifyCollectionChanged != null)
                    notifyCollectionChanged.CollectionChanged -= _weakHandler;
                _itemsSource = value;
                notifyCollectionChanged = _itemsSource as INotifyCollectionChanged;
                if (notifyCollectionChanged != null)
                    notifyCollectionChanged.CollectionChanged += _weakHandler;
            }
            if (notifyDataSet)
                NotifyDataSetChanged();
        }

        protected virtual void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (!_recyclerView.IsAlive())
            {
                SetItemsSource(null, false);
                return;
            }
            if (!TryUpdateItems(args))
                NotifyDataSetChanged();
        }

        protected bool TryUpdateItems(NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    NotifyItemRangeInserted(args.NewStartingIndex, args.NewItems.Count);
                    return true;
                case NotifyCollectionChangedAction.Remove:
                    NotifyItemRangeRemoved(args.OldStartingIndex, args.OldItems.Count);
                    return true;
                case NotifyCollectionChangedAction.Move:
                    if (args.NewItems.Count != 1 && args.OldItems.Count != 1)
                        return false;
                    NotifyItemMoved(args.OldStartingIndex, args.NewStartingIndex);
                    return true;
                case NotifyCollectionChangedAction.Replace:
                    if (args.NewItems.Count != args.OldItems.Count)
                        return false;
                    NotifyItemRangeChanged(args.NewStartingIndex, args.NewItems.Count);
                    return true;
                default:
                    return false;
            }
        }

        private void ActivityViewOnDestroyed(Activity sender)
        {
            ((IActivityView)sender).Mediator.Destroyed -= _listener.Handle;
            SetItemsSource(null, false);
            if (_recyclerView.IsAlive() && ReferenceEquals(_recyclerView.GetAdapter(), this))
                _recyclerView.SetAdapter(null);
        }

        #endregion

        #region Overrides of Adapter

        public override int ItemCount
        {
            get
            {
                if (ItemsSource == null)
                    return 0;
                return ItemsSource.Count();
            }
        }

        public override void OnBindViewHolder(global::Android.Support.V7.Widget.RecyclerView.ViewHolder holder, int position)
        {
            object item = GetRawItem(position);
            if (holder.ItemViewType == global::Android.Resource.Layout.SimpleListItem1)
            {
                var textView = holder.ItemView as TextView;
                if (textView != null)
                    textView.Text = item.ToStringSafe("(null)");
            }
            else
                holder.ItemView.SetDataContext(item);
        }

        public override global::Android.Support.V7.Widget.RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent,
            int viewType)
        {
            return new ViewHolderImpl(LayoutInflater.Inflate(viewType, parent, false));
        }

        public override void OnViewRecycled(Object holder)
        {
            var viewHolder = holder as global::Android.Support.V7.Widget.RecyclerView.ViewHolder;
            if (viewHolder != null && viewHolder.ItemView != null)
                viewHolder.ItemView.SetDataContext(null);
            base.OnViewRecycled(holder);
        }

        public override int GetItemViewType(int position)
        {
            object item = GetRawItem(position);
            int id;
            if (_itemTemplateProvider.TrySelectResourceTemplate(item, out id))
                return id;
            object template;
            if (_itemTemplateProvider.TrySelectTemplate(item, out template))
            {
                if (template is int)
                    return (int)template;
                Tracer.Error("The DataTemplate '{0}' is not supported by RecyclerView", template);
            }
            return _itemTemplateProvider.GetTemplateId().GetValueOrDefault(global::Android.Resource.Layout.SimpleListItem1);
        }

        #endregion
    }
}