using System.Collections;
using Android.Support.Design.Widget;
using MugenMvvmToolkit.Android.Binding;
using MugenMvvmToolkit.Android.Binding.Infrastructure;
using MugenMvvmToolkit.Android.Binding.Interfaces;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Android.Design.Infrastructure
{
    internal class TabLayoutItemsSourceGenerator : ItemsSourceGeneratorBase
    {
        #region Fields

        private readonly TabLayout _tabLayout;
        private readonly IBindingMemberInfo _collectionViewManagerMember;

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

        protected override bool IsTargetDisposed
        {
            get { return !_tabLayout.IsAlive(); }
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

        #endregion
    }
}