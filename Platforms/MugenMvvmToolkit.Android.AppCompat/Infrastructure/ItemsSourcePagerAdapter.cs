#region Copyright

// ****************************************************************************
// <copyright file="ItemsSourcePagerAdapter.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
using Android.OS;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;
using Java.Lang;
using JetBrains.Annotations;
using MugenMvvmToolkit.Android.Binding.Infrastructure;
using MugenMvvmToolkit.Android.Interfaces.Views;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models.EventArg;
using Fragment = Android.Support.V4.App.Fragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;
using FragmentTransaction = Android.Support.V4.App.FragmentTransaction;
using Object = Java.Lang.Object;
using String = Java.Lang.String;

namespace MugenMvvmToolkit.Android.AppCompat.Infrastructure
{
    public class ItemsSourcePagerAdapter : PagerAdapter
    {
        #region Fields

        private readonly NotifyCollectionChangedEventHandler _weakHandler;
        private readonly DataTemplateProvider _itemTemplateProvider;
        private readonly FragmentManager _fragmentManager;
        private readonly ViewPager _viewPager;
        private readonly ReflectionExtensions.IWeakEventHandler<EventArgs> _listener;

        private IEnumerable _itemsSource;
        private Fragment _currentPrimaryItem;
        private FragmentTransaction _currentTransaction;
        private bool _isRestored;
        private const string ContentPath = "!~#vpcontent";

        #endregion

        #region Constructors

        public ItemsSourcePagerAdapter([NotNull] ViewPager viewPager)
        {
            Should.NotBeNull(viewPager, nameof(viewPager));
            _viewPager = viewPager;
            _fragmentManager = viewPager.GetFragmentManager();
            _itemTemplateProvider = new DataTemplateProvider(viewPager, AttachedMemberConstants.ItemTemplate, AttachedMemberConstants.ItemTemplateSelector);
            _weakHandler = ReflectionExtensions.MakeWeakCollectionChangedHandler(this,
                (adapter, o, arg3) => adapter.OnCollectionChanged(o, arg3));
            var activityView = _viewPager.Context.GetActivity() as IActivityView;
            if (activityView != null)
            {
                _listener = ReflectionExtensions.CreateWeakEventHandler<ItemsSourcePagerAdapter, EventArgs>(this, (adapter, o, arg3) => adapter.ActivityViewOnDestroyed((Activity)o));
                activityView.Mediator.Destroyed += _listener.Handle;
            }
        }

        #endregion

        #region Properties

        public virtual IEnumerable ItemsSource
        {
            get { return _itemsSource; }
            set { SetItemsSource(value, true); }
        }

        protected DataTemplateProvider DataTemplateProvider => _itemTemplateProvider;

        #endregion

        #region Methods

        public virtual object GetRawItem(int position)
        {
            if (position < 0)
                return null;
            return ItemsSource.ElementAtIndex(position);
        }

        public virtual int GetPosition(object value)
        {
            if (ItemsSource == null)
                return PositionNone;
            var index = ItemsSource.IndexOf(value, ReferenceEqualityComparer.Instance);
            if (index < 0)
                return PositionNone;
            return index;
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
            if (value != null && !_isRestored && _viewPager.GetBindingMemberValue(AttachedMembersCompat.ViewPager.RestoreSelectedIndex).GetValueOrDefault(true))
            {
                _isRestored = true;
                TryRestoreSelectedIndex();
            }
        }

        protected virtual void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (_viewPager.IsAlive())
                NotifyDataSetChanged();
            else
                SetItemsSource(null, false);
        }

        private void TryRestoreSelectedIndex()
        {
            var activityView = _viewPager.Context as IActivityView;
            if (activityView == null)
                return;
            var bundle = activityView.Mediator.Bundle;
            if (bundle != null)
            {
                var i = bundle.GetInt(ContentPath, int.MinValue);
                if (i != int.MinValue)
                    _viewPager.CurrentItem = i;
            }
            var stateListener = ReflectionExtensions.CreateWeakEventHandler<ItemsSourcePagerAdapter, ValueEventArgs<Bundle>>(this, (adapter, o, arg3) => adapter.ActivityViewOnSaveInstanceState(arg3));
            activityView.Mediator.SaveInstanceState += stateListener.Handle;
        }

        private void ActivityViewOnSaveInstanceState(ValueEventArgs<Bundle> args)
        {
            var index = _viewPager.CurrentItem;
            if (index > 0)
                args.Value.PutInt(ContentPath, index);
        }

        private void ActivityViewOnDestroyed(Activity sender)
        {
            ((IActivityView)sender).Mediator.Destroyed -= _listener.Handle;
            if (!_viewPager.IsAlive())
                return;
            if (ReferenceEquals(_viewPager.Adapter, this))
            {
                _viewPager.Adapter = null;
                if (ItemsSource != null)
                {
                    foreach (var item in ItemsSource)
                    {
                        if (item != null)
                            ServiceProvider.AttachedValueProvider.Clear(item, ContentPath);
                    }
                }
            }
            else
            {
                if (ItemsSource != null)
                {
                    foreach (var item in ItemsSource)
                    {
                        if (item == null)
                            continue;
                        var value = ServiceProvider.AttachedValueProvider.GetValue<Object>(item, ContentPath, false);
                        if (value != null)
                            DestroyItem(_viewPager, PositionNone, value);
                    }
                    FinishUpdate(_viewPager);
                }
            }
            SetItemsSource(null, false);
        }

        #endregion

        #region Overrides of FragmentPagerAdapter

        public override ICharSequence GetPageTitleFormatted(int position)
        {
            if (ItemsSource == null)
                return base.GetPageTitleFormatted(position);
            var item = ItemsSource.ElementAtIndex(position);
            var func = _viewPager.GetBindingMemberValue(AttachedMembersCompat.ViewPager.GetPageTitleDelegate);
            if (func != null)
                return func(item);

            var displayName = item as IHasDisplayName;
            if (displayName == null)
                return null;
            return new String(displayName.DisplayName);
        }

        public override Object InstantiateItem(ViewGroup container, int position)
        {
            if (ItemsSource == null)
                return null;
            var item = GetRawItem(position);
            if (item == null)
                return new TextView(container.Context) { Text = "(null)" };
            var viewModel = item as IViewModel;
            if (viewModel != null)
                viewModel.Settings.Metadata.AddOrUpdate(ViewModelConstants.StateNotNeeded, true);

            var view = ServiceProvider.AttachedValueProvider.GetOrAdd(item, ContentPath,
                (o, o1) => (Object)PlatformExtensions.GetContentView(container, container.Context, o,
                    _itemTemplateProvider.GetTemplateId(), _itemTemplateProvider.GetDataTemplateSelector()), null);
            var fragment = view as Fragment;
            if (fragment == null)
                container.AddView((View)view);
            else
            {
                if (_currentTransaction == null)
                    _currentTransaction = _fragmentManager.BeginTransaction();
                if (fragment.IsDetached)
                    _currentTransaction.Attach(fragment);
                else if (!fragment.IsAdded)
                    _currentTransaction.Add(container.Id, fragment);
                if (fragment != _currentPrimaryItem)
                {
                    fragment.SetMenuVisibility(false);
                    fragment.UserVisibleHint = false;
                }
            }
            return view;
        }

        public override void FinishUpdate(ViewGroup container)
        {
            if (_currentTransaction == null || _fragmentManager.IsDestroyed)
                return;
            _currentTransaction.CommitAllowingStateLoss();
            _currentTransaction = null;
            _fragmentManager.ExecutePendingTransactions();
        }

        public override bool IsViewFromObject(View view, Object @object)
        {
            var fragment = @object as Fragment;
            if (fragment != null)
                @object = fragment.View;
            return view == @object;
        }

        public override void DestroyItem(ViewGroup container, int position, Object @object)
        {
            var dataContext = @object.DataContext();
            if (position != PositionNone)
                position = GetPosition(dataContext);
            bool removed = position == PositionNone;
            if (removed && dataContext != null)
                ServiceProvider.AttachedValueProvider.Clear(dataContext, ContentPath);
            var fragment = @object as Fragment;
            if (fragment == null)
            {
                var view = (View)@object;
                container.RemoveView(view);
            }
            else
            {
                if (_currentTransaction == null)
                    _currentTransaction = _fragmentManager.BeginTransaction();
                if (removed)
                    _currentTransaction.Remove(fragment);
                else
                    _currentTransaction.Detach(fragment);
            }
        }

        public override void SetPrimaryItem(View container, int position, Object @object)
        {
            var fragment = @object as Fragment;
            if (fragment != _currentPrimaryItem)
            {
                if (_currentPrimaryItem.IsAlive())
                {
                    _currentPrimaryItem.SetMenuVisibility(false);
                    _currentPrimaryItem.UserVisibleHint = false;
                }
                if (fragment != null)
                {
                    fragment.SetMenuVisibility(true);
                    fragment.UserVisibleHint = true;
                }
            }
            _currentPrimaryItem = fragment;
        }

        public override int GetItemPosition(Object @object)
        {
            if (ItemsSource == null)
                return PositionNone;
            var dataContext = @object.DataContext();
            return GetPosition(dataContext);
        }

        public override int Count
        {
            get
            {
                if (ItemsSource == null)
                    return 0;
                return ItemsSource.Count();
            }
        }

        #endregion
    }
}
