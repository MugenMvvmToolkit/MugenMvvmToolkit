using System.Collections;
using Android.Support.Design.Widget;
using MugenMvvmToolkit.Android.Binding;
using MugenMvvmToolkit.Android.Binding.Infrastructure;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Android.Design.Infrastructure
{
    internal class TabLayoutItemsSourceGenerator : ItemsSourceGeneratorBase
    {
        #region Fields

        private readonly TabLayout _tabLayout;

        #endregion

        #region Constructors

        public TabLayoutItemsSourceGenerator(TabLayout tabLayout)
        {
            _tabLayout = tabLayout;
            TryListenActivity(tabLayout.Context);
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
            for (int i = 0; i < count; i++)
            {
                int index = insertionIndex + i;
                var tab = CreateTab(index);
                _tabLayout.AddTab(tab, index, false);
            }
        }

        protected override void Remove(int removalIndex, int count)
        {
            for (int i = 0; i < count; i++)
            {
                int index = removalIndex + i;
                Remove(index);
            }
        }

        protected override void Replace(int startIndex, int count)
        {
            for (int i = 0; i < count; i++)
            {
                int index = startIndex + i;
                Remove(index);
                _tabLayout.AddTab(CreateTab(index), index, false);
            }
        }

        protected override void Refresh()
        {
            var selectedItem = _tabLayout.GetBindingMemberValue(AttachedMembersDesign.TabLayout.SelectedItem);
            _tabLayout.RemoveAllTabs();
            IEnumerable itemsSource = ItemsSource;
            if (itemsSource == null)
                return;
            int count = itemsSource.Count();
            for (int index = 0; index < count; index++)
            {
                var item = GetItem(index);
                var tab = CreateTab(item);
                _tabLayout.AddTab(tab, index, ReferenceEquals(selectedItem, item));
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

        #endregion
    }
}