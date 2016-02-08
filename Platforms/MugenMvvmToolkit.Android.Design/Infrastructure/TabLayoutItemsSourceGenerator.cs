#region Copyright

// ****************************************************************************
// <copyright file="TabLayoutItemsSourceGenerator.cs">
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

using System.Collections;
using Android.OS;
using Android.Support.Design.Widget;
using MugenMvvmToolkit.Android.Binding;
using MugenMvvmToolkit.Android.Binding.Infrastructure;
using MugenMvvmToolkit.Android.Binding.Interfaces;
using MugenMvvmToolkit.Android.Interfaces.Views;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Android.Design.Infrastructure
{
    internal class TabLayoutItemsSourceGenerator : ItemsSourceGeneratorBase
    {
        #region Fields

        private const string SelectedIndexKey = "!~tabindex";
        private readonly TabLayout _tabLayout;
        private readonly IBindingMemberInfo _collectionViewManagerMember;
        private bool _isRestored;

        #endregion

        #region Constructors

        public TabLayoutItemsSourceGenerator(TabLayout tabLayout)
        {
            _tabLayout = tabLayout;
            TryListenActivity(tabLayout.Context);
            _collectionViewManagerMember = BindingServiceProvider
                .MemberProvider
                .GetBindingMember(tabLayout.GetType(), AttachedMembers.ViewGroup.CollectionViewManager, false, false);
        }

        #endregion

        #region Overrides of ItemsSourceGeneratorBase

        protected override IEnumerable ItemsSource { get; set; }

        protected override bool IsTargetDisposed => !_tabLayout.IsAlive();

        protected override void Update(IEnumerable itemsSource, IDataContext context = null)
        {
            base.Update(itemsSource, context);
            if (itemsSource != null && !_isRestored && _tabLayout.GetBindingMemberValue(AttachedMembersDesign.TabLayout.RestoreSelectedIndex).GetValueOrDefault(true))
            {
                _isRestored = true;
                TryRestoreSelectedIndex();
            }
        }

        protected override void Add(int insertionIndex, int count)
        {
            var manager = GetCollectionViewManager();
            for (int i = 0; i < count; i++)
            {
                int index = insertionIndex + i;
                var tab = CreateTab(index);
                if (manager == null)
                    _tabLayout.AddTab(tab, index, false);
                else
                    manager.Insert(_tabLayout, i, tab);
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
                    manager.RemoveAt(_tabLayout, index);
            }
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
                    manager.RemoveAt(_tabLayout, index);
                var tab = CreateTab(index);
                if (manager == null)
                    _tabLayout.AddTab(tab, index, false);
                else
                    manager.Insert(_tabLayout, index, tab);
            }
        }

        protected override void Refresh()
        {
            var manager = GetCollectionViewManager();
            var selectedItem = _tabLayout.GetBindingMemberValue(AttachedMembersDesign.TabLayout.SelectedItem);
            if (manager == null)
                _tabLayout.RemoveAllTabs();
            else
                manager.Clear(_tabLayout);
            IEnumerable itemsSource = ItemsSource;
            if (itemsSource == null)
                return;
            int count = itemsSource.Count();
            for (int index = 0; index < count; index++)
            {
                var item = GetItem(index);
                var tab = CreateTab(item);
                if (manager == null)
                    _tabLayout.AddTab(tab, index, ReferenceEquals(selectedItem, item));
                else
                    manager.Insert(_tabLayout, index, tab);
            }
        }

        #endregion

        #region Methods

        private void Remove(int index)
        {
            _tabLayout.RemoveTabAt(index);
        }

        private TabLayout.Tab CreateTab(object item)
        {
            var selector = _tabLayout.GetBindingMemberValue(AttachedMembers.ViewGroup.ItemTemplateSelector);
            TabLayout.Tab tab;
            if (selector == null)
            {
                tab = _tabLayout.NewTab();
                if (item is IHasDisplayName)
                    tab.SetBindings("Text DisplayName, Mode=OneWay");
                else
                    tab.SetText(item.ToStringSafe("null"));
            }
            else
                tab = (TabLayout.Tab)selector.SelectTemplate(item, _tabLayout);
            tab.SetDataContext(item);
            return tab;
        }

        private TabLayout.Tab CreateTab(int index)
        {
            return CreateTab(GetItem(index));
        }

        private ICollectionViewManager GetCollectionViewManager()
        {
            return (ICollectionViewManager)_collectionViewManagerMember.GetValue(_tabLayout, Empty.Array<object>());
        }

        private void TryRestoreSelectedIndex()
        {
            var activityView = _tabLayout.Context as IActivityView;
            if (activityView == null)
                return;
            var bundle = activityView.Mediator.Bundle;
            if (bundle != null)
            {
                var i = bundle.GetInt(SelectedIndexKey, int.MinValue);
                if (i != int.MinValue)
                {
                    if (_tabLayout.TabCount > i)
                        _tabLayout.GetTabAt(i).Select();
                }
            }
            var stateListener = ReflectionExtensions.CreateWeakEventHandler<TabLayoutItemsSourceGenerator, ValueEventArgs<Bundle>>(this, (generator, o, arg3) => generator.ActivityViewOnSaveInstanceState(arg3));
            activityView.Mediator.SaveInstanceState += stateListener.Handle;
        }

        private void ActivityViewOnSaveInstanceState(ValueEventArgs<Bundle> args)
        {
            var index = _tabLayout.SelectedTabPosition;
            if (index > 0)
                args.Value.PutInt(SelectedIndexKey, index);
        }

        #endregion
    }
}
