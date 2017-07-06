#region Copyright

// ****************************************************************************
// <copyright file="ItemsSourceRecyclerAdapter.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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
using System.Collections.Generic;
using System.Collections.Specialized;
using Android.App;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MugenMvvmToolkit.Android.Binding;
using MugenMvvmToolkit.Android.Binding.Infrastructure;
using MugenMvvmToolkit.Android.Interfaces;
using MugenMvvmToolkit.Android.Interfaces.Views;
using MugenMvvmToolkit.Android.Models;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces.Models;

namespace MugenMvvmToolkit.Android.RecyclerView.Infrastructure
{
    public class ItemsSourceRecyclerAdapter : global::Android.Support.V7.Widget.RecyclerView.Adapter
    {
        #region Nested types

        public interface IViewHolder : IBindingContextHolder
        {
            void ApplyBindings();
        }

        public interface IViewHolderEx : IViewHolder
        {
            LayoutInflaterResult LayoutInflaterResult { get; }
        }

        private sealed class ViewHolderImpl : global::Android.Support.V7.Widget.RecyclerView.ViewHolder, IViewHolderEx
        {
            #region Fields

            private readonly IBindingContext _bindingContext;
            private LayoutInflaterResult _layoutInflaterResult;

            #endregion

            #region Constructors

            private ViewHolderImpl(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
            {
                _bindingContext = BindingServiceProvider.ContextManager.GetBindingContext(ItemView);
            }

            public ViewHolderImpl(LayoutInflaterResult result, int viewType)
                : base(result.View)
            {
                if (viewType != global::Android.Resource.Layout.SimpleListItem1)
                {
                    _bindingContext = BindingServiceProvider.ContextManager.GetBindingContext(result.View);
                    _layoutInflaterResult = result;
                }
            }

            #endregion

            #region Properties

            //In the hundreds of times faster than the access to the ItemView field.
            public IBindingContext BindingContext => _bindingContext;

            public LayoutInflaterResult LayoutInflaterResult => _layoutInflaterResult;

            public void ApplyBindings()
            {
                if (_layoutInflaterResult != null)
                {
                    _layoutInflaterResult?.ApplyBindings();
                    _layoutInflaterResult = null;
                }
            }

            #endregion
        }

        #endregion

        #region Fields

        private readonly NotifyCollectionChangedEventHandler _weakHandler;
        private List<global::Android.Support.V7.Widget.RecyclerView.AdapterDataObserver> _observers;
        private DataTemplateProvider _itemTemplateProvider;
        private LayoutInflater _layoutInflater;
        private Func<LayoutInflater, ViewGroup, int, global::Android.Support.V7.Widget.RecyclerView.ViewHolder> _createViewHolderDelegate;
        private global::Android.Support.V7.Widget.RecyclerView _recyclerView;
        private ReflectionExtensions.IWeakEventHandler<EventArgs> _listener;
        private IStableIdProvider _stableIdProvider;
        private IEnumerable _itemsSource;

        #endregion

        #region Constructors

        public ItemsSourceRecyclerAdapter()
        {
            _weakHandler = ReflectionExtensions.MakeWeakCollectionChangedHandler(this, (adapter, o, arg3) => adapter.OnCollectionChanged(o, arg3));
        }

        #endregion

        #region Properties

        protected global::Android.Support.V7.Widget.RecyclerView RecyclerView => _recyclerView;

        protected DataTemplateProvider DataTemplateProvider => _itemTemplateProvider;

        protected LayoutInflater LayoutInflater => _layoutInflater;

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
            return ItemsSource?.ElementAtIndex(position);
        }

        protected virtual void SetItemsSource(IEnumerable value, bool notifyDataSet)
        {
            if (ReferenceEquals(value, _itemsSource) || !this.IsAlive())
                return;
            var notifyCollectionChanged = _itemsSource as INotifyCollectionChanged;
            if (notifyCollectionChanged != null)
                notifyCollectionChanged.CollectionChanged -= _weakHandler;
            _itemsSource = value;
            if (notifyDataSet)
                NotifyDataSetChanged();
            notifyCollectionChanged = _itemsSource as INotifyCollectionChanged;
            if (notifyCollectionChanged != null)
                notifyCollectionChanged.CollectionChanged += _weakHandler;
        }

        protected virtual void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (_recyclerView.IsAlive() && this.IsAlive())
            {
                if (!TryUpdateItems(args))
                    NotifyDataSetChanged();
            }
            else
                SetItemsSource(null, false);
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
            var recyclerView = _recyclerView;
            if (recyclerView.IsAlive() && ReferenceEquals(recyclerView.GetAdapter(), this))
                recyclerView.SetAdapter(null);
        }

        private static void SetDataContext(global::Android.Support.V7.Widget.RecyclerView.ViewHolder holder, object context)
        {
            var contextHolder = holder as IBindingContextHolder;
            if (contextHolder == null)
            {
                if (holder.ItemViewType == global::Android.Resource.Layout.SimpleListItem1)
                {
                    var textView = holder.ItemView as TextView;
                    if (textView != null)
                        textView.Text = context.ToStringSafe("(null)");
                }
                else
                    holder.ItemView.SetDataContext(context);
            }
            else
            {
                if (contextHolder.BindingContext == null)
                {
                    var textView = holder.ItemView as TextView;
                    if (textView != null)
                        textView.Text = context.ToStringSafe("(null)");
                }
                else
                    contextHolder.BindingContext.Value = context;
            }
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

        public override void OnAttachedToRecyclerView(global::Android.Support.V7.Widget.RecyclerView recyclerView)
        {
            _recyclerView = recyclerView;
            _itemTemplateProvider = new DataTemplateProvider(_recyclerView, AttachedMemberConstants.ItemTemplate, AttachedMemberConstants.ItemTemplateSelector);
            _layoutInflater = _recyclerView.Context.GetBindableLayoutInflater();
            _createViewHolderDelegate = _recyclerView.GetBindingMemberValue(AttachedMembersRecyclerView.RecyclerView.CreateViewHolderDelegate);
            HasStableIds = recyclerView.TryGetBindingMemberValue(AttachedMembers.Object.StableIdProvider, out _stableIdProvider) && _stableIdProvider != null;
            var activityView = _recyclerView.Context.GetActivity() as IActivityView;
            if (activityView != null)
            {
                if (_listener == null)
                    _listener = ReflectionExtensions.CreateWeakEventHandler<ItemsSourceRecyclerAdapter, EventArgs>(this, (adapter, o, arg3) => adapter.ActivityViewOnDestroyed((Activity)o));
                activityView.Mediator.Destroyed += _listener.Handle;
            }
            //To prevent HasStableIds error.
            if (_observers != null)
            {
                foreach (var observer in _observers)
                    base.RegisterAdapterDataObserver(observer);
                _observers = null;
            }
            _recyclerView.SetDisableHierarchyListener(true);
            base.OnAttachedToRecyclerView(recyclerView);
        }

        public override void OnDetachedFromRecyclerView(global::Android.Support.V7.Widget.RecyclerView recyclerView)
        {
            var activityView = recyclerView.Context.GetActivity() as IActivityView;
            if (activityView != null)
                activityView.Mediator.Destroyed -= _listener.Handle;
            _layoutInflater = null;
            _itemTemplateProvider = null;
            _recyclerView = null;
            _createViewHolderDelegate = null;
            base.OnDetachedFromRecyclerView(recyclerView);
        }

        public override long GetItemId(int position)
        {
            if (_stableIdProvider == null)
                return global::Android.Support.V7.Widget.RecyclerView.NoId;
            return _stableIdProvider.GetItemId(GetRawItem(position));
        }

        public override void OnBindViewHolder(global::Android.Support.V7.Widget.RecyclerView.ViewHolder holder, int position)
        {
            SetDataContext(holder, GetRawItem(position));
            (holder as IViewHolder)?.ApplyBindings();
        }

        public override global::Android.Support.V7.Widget.RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view;
            global::Android.Support.V7.Widget.RecyclerView.ViewHolder viewHolder;
            if (_createViewHolderDelegate == null)
            {
                var result = _layoutInflater.InflateEx(viewType, parent, false);
                view = result.View;
                viewHolder = new ViewHolderImpl(result, viewType);
            }
            else
            {
                viewHolder = _createViewHolderDelegate(_layoutInflater, parent, viewType);
                view = viewHolder.ItemView;
                if (!(viewHolder is IViewHolder))
                    view.SetDataContext(null);
            }
            if (parent != null)
                view.SetBindingMemberValue(AttachedMembers.Object.Parent, parent);
            return viewHolder;
        }

        public override int GetItemViewType(int position)
        {
            object item = GetRawItem(position);
            int id;
            if (_itemTemplateProvider.TrySelectResourceTemplate(item, out id))
            {
                if (id == 0)
                    return global::Android.Resource.Layout.SimpleListItem1;
                return id;
            }
            return _itemTemplateProvider.GetTemplateId().GetValueOrDefault(global::Android.Resource.Layout.SimpleListItem1);
        }

        public override void RegisterAdapterDataObserver(global::Android.Support.V7.Widget.RecyclerView.AdapterDataObserver observer)
        {
            if (_recyclerView == null)
            {
                if (_observers == null)
                    _observers = new List<global::Android.Support.V7.Widget.RecyclerView.AdapterDataObserver>();
                _observers.Add(observer);
            }
            else
                base.RegisterAdapterDataObserver(observer);
        }

        public override void UnregisterAdapterDataObserver(global::Android.Support.V7.Widget.RecyclerView.AdapterDataObserver observer)
        {
            if (_observers == null || !_observers.Remove(observer))
                base.UnregisterAdapterDataObserver(observer);
        }

        #endregion
    }
}
