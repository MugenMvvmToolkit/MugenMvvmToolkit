#region Copyright
// ****************************************************************************
// <copyright file="TabHostItemsSourceGenerator.cs">
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
using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Core;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Infrastructure.Mediators;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Models.EventArg;
using Object = Java.Lang.Object;
#if API8
using FragmentTransaction = System.Object;
#endif


namespace MugenMvvmToolkit.Infrastructure
{
    internal sealed class TabHostItemsSourceGenerator : ItemsSourceGeneratorBase
    {
        #region Nested types

        private sealed class TabFactory : Object, TabHost.ITabContentFactory
        {
            #region Fields

            private readonly TabHostItemsSourceGenerator _generator;

            #endregion

            #region Constructors

            public TabFactory([NotNull] TabHostItemsSourceGenerator generator)
            {
                Should.NotBeNull(generator, "generator");
                _generator = generator;
            }

            #endregion

            #region Implementation of ITabContentFactory

            public View CreateTabContent(string tag)
            {
                var view = new View(_generator._tabHost.Context);
                view.SetMinimumWidth(0);
                view.SetMinimumHeight(0);
                return view;
            }

            #endregion
        }

        private sealed class EmptyTemplateSelector : IDataTemplateSelector
        {
            #region Fields

            public static readonly EmptyTemplateSelector Instance = new EmptyTemplateSelector();
            public static readonly View EmptyView = new View(Application.Context);

            #endregion

            #region Implementation of IDataTemplateSelector

            public object SelectTemplate(object item, object container)
            {
                return EmptyView;
            }

            #endregion
        }

        private sealed class TabInfo
        {
            #region Fields

            public readonly object Item;
            public readonly TabHost.TabSpec TabSpec;
            public readonly object Content;

            #endregion

            #region Constructors

            public TabInfo(object item, TabHost.TabSpec tabSpec, object content)
            {
                Item = item;
                TabSpec = tabSpec;
                Content = content;
            }

            #endregion
        }

        #endregion

        #region Fields

        private const string SelectedTabIndexKey = "~@tindex";
        private const string Key = "!#tabgen";
        private bool _isRestored;

        private object _currentTabContent;
        private readonly TabFactory _tabFactory;
        private readonly TabHost _tabHost;
        private readonly Dictionary<string, TabInfo> _tabToContent;
        private readonly IBindingMemberInfo _selectedItemMember;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ViewGroupItemsSourceGenerator" /> class.
        /// </summary>
        private TabHostItemsSourceGenerator([NotNull] TabHost tabHost)
        {
            Should.NotBeNull(tabHost, "tabHost");
            _tabHost = tabHost;
            _tabHost.Setup();
            _tabToContent = new Dictionary<string, TabInfo>();
            _tabFactory = new TabFactory(this);
            _selectedItemMember = BindingProvider.Instance
                                                 .MemberProvider
                                                 .GetBindingMember(tabHost.GetType(), AttachedMemberConstants.SelectedItem, false, false);
            TryListenActivity(tabHost.Context);
            _tabHost.TabChanged += TabHostOnTabChanged;
        }

        #endregion

        #region Overrides of ItemsSourceGeneratorBase

        public override void Update(IEnumerable itemsSource)
        {
            base.Update(itemsSource);
            if (!_isRestored)
            {
                _isRestored = true;
                TryRestoreSelectedIndex();
            }
        }

        protected override IEnumerable ItemsSource { get; set; }

        protected override void Add(int insertionIndex, int count)
        {
            if (insertionIndex == _tabHost.TabWidget.TabCount)
            {
                for (int i = 0; i < count; i++)
                    _tabHost.AddTab(CreateTabSpec(insertionIndex + i));
            }
            else
                Refresh();
        }

        protected override void Remove(int removalIndex, int count)
        {
            Refresh();
        }

        protected override void Replace(int startIndex, int count)
        {
            Refresh();
        }

        protected override void Refresh()
        {
            string selectedTag = _tabHost.CurrentTabTag;
            var oldValues = new Dictionary<string, TabInfo>(_tabToContent);
            _tabHost.CurrentTab = 0;
            _tabHost.ClearAllTabs();
            _tabToContent.Clear();

            int count = ItemsSource.Count();
            for (int i = 0; i < count; i++)
            {
                var tabInfo = TryRecreateTabInfo(i, oldValues);
                _tabHost.AddTab(tabInfo.TabSpec);
            }
            foreach (var oldValue in oldValues)
                RemoveTab(oldValue.Value);


            if (count == 0)
                OnEmptyTab();
            else
            {
                if (selectedTag == null)
                {
                    _tabHost.CurrentTab = 0;
                    if (_selectedItemMember != null)
                        _selectedItemMember.SetValue(_tabHost, new[] { GetItem(0) });
                }
                else
                    _tabHost.SetCurrentTabByTag(selectedTag);
            }
        }

        #endregion

        #region Methods

        public static TabHostItemsSourceGenerator Get(TabHost tabHost)
        {
            return ServiceProvider.AttachedValueProvider.GetValue<TabHostItemsSourceGenerator>(tabHost, Key, false);
        }

        public static TabHostItemsSourceGenerator GetOrAdd(TabHost tabHost)
        {
            return ServiceProvider.AttachedValueProvider.GetOrAdd(tabHost, Key,
                (host, o) => new TabHostItemsSourceGenerator(host), null);
        }

        public void SetSelectedItem(object item)
        {
            if (item == null)
            {
                _tabHost.CurrentTab = 0;
                if (_tabHost.CurrentTabTag != null)
                    OnTabChanged(_tabHost.CurrentTabTag);
            }
            else
            {
                foreach (var pair in _tabToContent)
                {
                    if (pair.Value.Item == item)
                    {
                        if (_tabHost.CurrentTabTag != pair.Key)
                            _tabHost.SetCurrentTabByTag(pair.Key);
                        break;
                    }
                }
            }
        }

        private void TabHostOnTabChanged(object sender, TabHost.TabChangeEventArgs args)
        {
            OnTabChanged(args.TabId);
        }

        private void OnTabChanged(string id)
        {
            var oldValue = _currentTabContent;
            TabInfo info;
            if (_tabToContent.TryGetValue(id, out info))
            {
                _currentTabContent = info.Content;
                if (ReferenceEquals(_currentTabContent, oldValue))
                    return;

                var ft = OnTabUnselected(oldValue);
                ft = OnTabSelected(_currentTabContent, ft);
#if !API8
                if (ft != null)
                    ft.Commit();
#endif
                if (_selectedItemMember != null)
                    _selectedItemMember.SetValue(_tabHost, new[] { info.Item });
            }
            else
            {
                _currentTabContent = _tabHost.CurrentTabView;
                if (_selectedItemMember != null)
                    _selectedItemMember.SetValue(_tabHost, new object[] { _tabHost.CurrentTabView });
            }
        }

        private void OnEmptyTab()
        {
            if (_selectedItemMember != null)
                _selectedItemMember.SetValue(_tabHost, BindingExtensions.NullValue);
            if (_tabHost.TabContentView != null)
                _tabHost.TabContentView.RemoveAllViews();
        }

        private TabHost.TabSpec CreateTabSpec(int index)
        {
            return CreateTabInfo(GetItem(index)).TabSpec;
        }

        private TabInfo TryRecreateTabInfo(int index, Dictionary<string, TabInfo> oldValues)
        {
            object item = GetItem(index);
            foreach (var tabInfo in oldValues)
            {
                if (Equals(tabInfo.Value.Item, item))
                {
                    oldValues.Remove(tabInfo.Key);
                    _tabToContent[tabInfo.Key] = tabInfo.Value;
                    return tabInfo.Value;
                }
            }
            return CreateTabInfo(item);
        }

        private TabInfo CreateTabInfo(object item)
        {
            string id = Guid.NewGuid().ToString("n");
            var spec = _tabHost.NewTabSpec(id);
            var tabInfo = new TabInfo(item, spec, GetContent(item));
            _tabToContent[id] = tabInfo;
            SetIndicator(spec, item);
            spec.SetContent(_tabFactory);
            BindingProvider.Instance.ContextManager.GetBindingContext(spec).Value = item;
            return tabInfo;
        }

        private void SetIndicator(TabHost.TabSpec tabSpec, object item)
        {
            var viewModel = item as IViewModel;
#if !API8
            if (viewModel != null)
                viewModel.Settings.Metadata.AddOrUpdate(MvvmFragmentMediator.StateNotNeeded, true);
#endif

            var templateId = ValueTemplateManager.GetTemplateId(_tabHost, AttachedMemberConstants.ItemTemplate);
            var selector = ValueTemplateManager.GetDataTemplateSelector(_tabHost, AttachedMemberConstants.ItemTemplateSelector);
            if (templateId == null && selector == null)
                selector = EmptyTemplateSelector.Instance;
            object content = PlatformExtensions.GetContentView(_tabHost, _tabHost.Context, item, templateId, selector);
            if (content == EmptyTemplateSelector.EmptyView)
            {
                if (viewModel is IHasDisplayName)
                    BindingProvider.Instance.CreateBindingsFromString(tabSpec, "Title DisplayName", null);
                else
                    tabSpec.SetIndicator(item.ToStringSafe("(null)"));
            }
            var view = content as View;
            if (view == null)
                tabSpec.SetIndicator(content.ToStringSafe("(null)"));
            else
                tabSpec.SetIndicator(view);
        }

        private object GetContent(object item)
        {
            return PlatformExtensions.GetContentView(_tabHost, _tabHost.Context,
                item, ValueTemplateManager.GetTemplateId(_tabHost, AttachedMemberConstants.ContentTemplate),
                ValueTemplateManager.GetDataTemplateSelector(_tabHost, AttachedMemberConstants.ContentTemplateSelector));
        }

        private FragmentTransaction OnTabUnselected(object content)
        {
#if !API8
            var fragment = content as Fragment;
            if (fragment != null)
            {
                var fragmentManager = _tabHost.GetFragmentManager();
                if (fragmentManager == null)
                    return null;
                return fragmentManager.BeginTransaction().Detach(fragment);
            }
#endif
            var view = content as View;
            if (view != null)
                _tabHost.TabContentView.RemoveView(view);
            return null;
        }

        private FragmentTransaction OnTabSelected(object content, FragmentTransaction ft)
        {
#if !API8
            var fragment = content as Fragment;
            if (fragment != null)
            {
                if (ft == null)
                {
                    var fragmentManager = _tabHost.GetFragmentManager();
                    if (fragmentManager == null)
                        return null;
                    ft = fragmentManager.BeginTransaction();
                }

                if (fragment.IsDetached)
                    return ft.Attach(fragment);
                return ft.Replace(_tabHost.TabContentView.Id, fragment);
            }
#endif
            var view = content as View;
            if (view == null)
                _tabHost.TabContentView.RemoveAllViews();
            else
                _tabHost.TabContentView.AddView(view);
            return null;
        }

        private void RemoveTab(TabInfo tab)
        {
#if !API8
            var fragment = tab.Content as Fragment;
            if (fragment == null)
                return;
            var fragmentManager = _tabHost.GetFragmentManager();
            if (fragmentManager == null)
                return;
            fragmentManager.BeginTransaction()
                .Remove(fragment)
                .Commit();
            fragmentManager.ExecutePendingTransactions();
#endif
        }

        private void TryRestoreSelectedIndex()
        {
            var activityView = _tabHost.Context as IActivityView;
            if (activityView == null)
                return;
            var bundle = activityView.Bundle;
            if (bundle != null)
            {
                var i = bundle.GetInt(SelectedTabIndexKey, int.MinValue);
                if (i != int.MinValue)
                    _tabHost.CurrentTab = i;
            }
            activityView.SaveInstanceState += ActivityViewOnSaveInstanceState;
        }

        private void ActivityViewOnSaveInstanceState(Activity sender, ValueEventArgs<Bundle> args)
        {
            var index = _tabHost.CurrentTab;
            if (index > 0)
                args.Value.PutInt(SelectedTabIndexKey, index);
        }

        #endregion

    }
}