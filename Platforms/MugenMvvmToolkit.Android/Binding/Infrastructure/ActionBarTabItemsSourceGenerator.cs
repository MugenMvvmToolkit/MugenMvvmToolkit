#region Copyright

// ****************************************************************************
// <copyright file="ActionBarTabItemsSourceGenerator.cs">
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

using System.Collections;
using Android.Widget;
using MugenMvvmToolkit.Android.Binding;
using MugenMvvmToolkit.Android.Binding.Interfaces;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces.Models;
#if APPCOMPAT
using MugenMvvmToolkit.Android.AppCompat.Models;
using MugenMvvmToolkit.Android.AppCompat.Views;
using MugenMvvmToolkit.Android.Binding.Infrastructure;
using ActionBar = Android.Support.V7.App.ActionBar;

namespace MugenMvvmToolkit.Android.AppCompat.Infrastructure
#else
using Android.App;
using MugenMvvmToolkit.Android.Binding.Models;
using MugenMvvmToolkit.Android.Views;

namespace MugenMvvmToolkit.Android.Binding.Infrastructure
#endif
{
    internal sealed class ActionBarTabItemsSourceGenerator : ItemsSourceGeneratorBase, IItemsSourceGeneratorEx
    {
        #region Fields

        private readonly ActionBar _actionBar;
        private readonly ActionBarTabTemplate _tabTemplate;
        private readonly IBindingMemberInfo _collectionViewManagerMember;

        #endregion

        #region Constructors

        internal ActionBarTabItemsSourceGenerator(ActionBar actionBar, ActionBarTabTemplate tabTemplate)
        {
            Should.NotBeNull(actionBar, nameof(actionBar));
            Should.NotBeNull(tabTemplate, nameof(tabTemplate));
            _actionBar = actionBar;
            _tabTemplate = tabTemplate;
            _collectionViewManagerMember = BindingServiceProvider
                .MemberProvider
                .GetBindingMember(actionBar.GetType(), AttachedMembers.ActionBar.CollectionViewManager, false, false);
            TryListenActivity(_actionBar.ThemedContext);
        }

        #endregion

        #region Implementation of IItemsSourceGeneratorEx

        public object SelectedItem
        {
            get
            {
#if APPCOMPAT
                return _actionBar.GetBindingMemberValue(AttachedMembersCompat.ActionBar.SelectedItem);
#else
                return _actionBar.GetBindingMemberValue(AttachedMembers.ActionBar.SelectedItem);
#endif
            }
            set { SetSelectedItem(value); }
        }

        #endregion

        #region Overrides of ItemsSourceGeneratorBase

        protected override IEnumerable ItemsSource { get; set; }

        protected override bool IsTargetDisposed
        {
            get { return !_actionBar.IsAlive(); }
        }

        protected override void Add(int insertionIndex, int count)
        {
            var manager = GetCollectionViewManager();
            for (int i = 0; i < count; i++)
            {
                int index = insertionIndex + i;
                var tab = CreateTab(index);
                if (manager == null)
                    _actionBar.AddTab(tab, index, false);
                else
                    manager.Insert(_actionBar, index, tab);
            }
        }

        protected override void Remove(int removalIndex, int count)
        {
            var manager = GetCollectionViewManager();
            for (int i = 0; i < count; i++)
            {
                int index = removalIndex + i;
                if (manager == null)
                    Remove(index);
                else
                    manager.RemoveAt(_actionBar, index);
            }
            if (_actionBar.TabCount == 0)
                OnEmptyTab();
        }

        protected override void Replace(int startIndex, int count)
        {
            var manager = GetCollectionViewManager();
            for (int i = 0; i < count; i++)
            {
                int index = startIndex + i;
                if (manager == null)
                    Remove(index);
                else
                    manager.RemoveAt(_actionBar, index);
                var tab = CreateTab(index);
                if (manager == null)
                    _actionBar.AddTab(tab, index, false);
                else
                    manager.Insert(_actionBar, index, tab);
            }
        }

        protected override void Refresh()
        {
            var manager = GetCollectionViewManager();
            for (int i = 0; i < _actionBar.TabCount; i++)
                ActionBarTabTemplate.ClearTab(_actionBar, _actionBar.GetTabAt(i), true);
            if (manager == null)
                _actionBar.RemoveAllTabs();
            else
                manager.Clear(_actionBar);

            IEnumerable itemsSource = ItemsSource;
            if (itemsSource == null)
                return;

#if APPCOMPAT
            var selectedItem = _actionBar.GetBindingMemberValue(AttachedMembersCompat.ActionBar.SelectedItem);
#else
            var selectedItem = _actionBar.GetBindingMemberValue(AttachedMembers.ActionBar.SelectedItem);
#endif
            int count = itemsSource.Count();
            for (int index = 0; index < count; index++)
            {
                var item = GetItem(index);
                var tab = CreateTab(item);
                if (manager == null)
                    _actionBar.AddTab(tab, index, ReferenceEquals(selectedItem, item));
                else
                    manager.Insert(_actionBar, index, tab);
            }
            if (count == 0)
                OnEmptyTab();
        }

        #endregion

        #region Methods

        private void SetSelectedItem(object selectedItem)
        {
            for (int i = 0; i < _actionBar.TabCount; i++)
            {
                var tab = _actionBar.GetTabAt(i);
                if (tab.DataContext() == selectedItem)
                {
                    if (tab.Position != _actionBar.SelectedNavigationIndex)
                        tab.Select();
                    return;
                }
            }
        }

        private void OnEmptyTab()
        {
#if APPCOMPAT
            _actionBar.SetBindingMemberValue(AttachedMembersCompat.ActionBar.SelectedItem, BindingExtensions.NullValue);
#else
            _actionBar.SetBindingMemberValue(AttachedMembers.ActionBar.SelectedItem, BindingExtensions.NullValue);
#endif
            var value = ActionBarView.GetTabContentId(_actionBar);
            if (value == null)
                return;
            var activity = _actionBar.ThemedContext.GetActivity();
            if (activity == null)
                return;
            var layout = activity.FindViewById<FrameLayout>(value.Value);
            if (layout != null)
                layout.RemoveAllViews();
        }

        private void Remove(int index)
        {
            var tabAt = _actionBar.GetTabAt(index);
            _actionBar.RemoveTabAt(index);
            if (tabAt != null)
                ActionBarTabTemplate.ClearTab(_actionBar, tabAt, true);
        }

        private ActionBar.Tab CreateTab(int index)
        {
            return CreateTab(GetItem(index));
        }

        private ActionBar.Tab CreateTab(object item)
        {
            return _tabTemplate.CreateTab(_actionBar, item);
        }

        private ICollectionViewManager GetCollectionViewManager()
        {
            return (ICollectionViewManager)_collectionViewManagerMember.GetValue(_actionBar, Empty.Array<object>());
        }

        #endregion
    }
}
