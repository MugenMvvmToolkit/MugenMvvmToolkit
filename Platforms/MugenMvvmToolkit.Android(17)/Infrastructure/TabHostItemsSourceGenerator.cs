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
using System.Linq;
using Android.App;
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
using Object = Java.Lang.Object;

namespace MugenMvvmToolkit.Infrastructure
{
    internal sealed class TabHostItemsSourceGenerator : ItemsSourceGeneratorBase
    {
        #region Nested types

        private sealed class ContentSelector
        {
            #region Fields

            private object _content;
            private readonly TabHostItemsSourceGenerator _generator;

            #endregion

            #region Constructors

            public ContentSelector(TabHostItemsSourceGenerator generator)
            {
                _generator = generator;
            }

            #endregion

            #region Methods

            public object GetContent(object item)
            {
                if (_content == null)
                {
                    var content = PlatformExtensions.GetContentView(_generator._tabHost, _generator._tabHost.Context,
                        item, ValueTemplateManager.GetTemplateId(_generator._tabHost, AttachedMemberConstants.ContentTemplate),
                        ValueTemplateManager.GetDataTemplateSelector(_generator._tabHost, AttachedMemberConstants.ContentTemplateSelector));
#if !API8
                    if (content is Fragment)
                        return content;
#endif
                    _content = content;
                }
                return _content;
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

        #endregion

        #region Fields

        private object _currentTabContent;
        private readonly TabFactory _tabFactory;
        private readonly TabHost _tabHost;
        private readonly Dictionary<string, Tuple<object, Func<object, object>, TabHost.TabSpec>> _tabToContent;
        private readonly IBindingMemberInfo _selectedItemMember;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ViewGroupItemsSourceGenerator" /> class.
        /// </summary>
        public TabHostItemsSourceGenerator([NotNull] TabHost tabHost)
        {
            Should.NotBeNull(tabHost, "tabHost");
            _tabHost = tabHost;
            _tabHost.Setup();
            _tabToContent = new Dictionary<string, Tuple<object, Func<object, object>, TabHost.TabSpec>>();
            _tabFactory = new TabFactory(this);
            _selectedItemMember = BindingProvider.Instance
                                                 .MemberProvider
                                                 .GetBindingMember(tabHost.GetType(), AttachedMemberConstants.SelectedItem, false, false);
            TryListenActivity(tabHost.Context);
            _tabHost.TabChanged += TabHostOnTabChanged;
        }

        #endregion

        #region Overrides of ItemsSourceGeneratorBase

        protected override IEnumerable ItemsSource { get; set; }

        protected override void Add(int insertionIndex, int count)
        {
            if (insertionIndex == _tabHost.TabWidget.TabCount)
            {
                for (int i = 0; i < count; i++)
                    _tabHost.AddTab(CreateTabSpec(insertionIndex + i, null));
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
            string selectedTag = null;
            Tuple<object, Func<object, object>, TabHost.TabSpec> value = null;
            if (_tabHost.CurrentTabTag != null)
                _tabToContent.TryGetValue(_tabHost.CurrentTabTag, out value);

            TryClearCurrentView(_currentTabContent);
            var oldValues = _tabToContent.ToDictionary(pair => pair.Value.Item1 ?? string.Empty, pair => pair);
            _tabHost.CurrentTab = 0;
            _tabHost.ClearAllTabs();
            _tabToContent.Clear();

            int count = ItemsSource.Count();
            for (int i = 0; i < count; i++)
            {
                var tabSpec = CreateTabSpec(i, oldValues);
                _tabHost.AddTab(tabSpec);
                if (value != null)
                {
                    var item = GetItem(i);
                    if (item == value.Item1)
                        selectedTag = tabSpec.Tag;
                }
            }
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
                foreach (var o in _tabToContent)
                {
                    if (o.Value.Item1 == item)
                    {
                        if (_tabHost.CurrentTabTag != o.Key)
                            _tabHost.SetCurrentTabByTag(o.Key);
                        break;
                    }
                }
            }
        }

        private void OnEmptyTab()
        {
            if (_selectedItemMember != null)
                _selectedItemMember.SetValue(_tabHost, BindingExtensions.NullValue);
            if (_tabHost.TabContentView != null)
                _tabHost.TabContentView.RemoveAllViews();
        }

        private void TabHostOnTabChanged(object sender, TabHost.TabChangeEventArgs args)
        {
            OnTabChanged(args.TabId);
        }

        private void OnTabChanged(string id)
        {
            var oldValue = _currentTabContent;
            Tuple<object, Func<object, object>, TabHost.TabSpec> value;
            if (_tabToContent.TryGetValue(id, out value))
            {
                _currentTabContent = value.Item2(value.Item1);
                if (Equals(_currentTabContent, oldValue))
                    return;
                TryClearCurrentView(oldValue);
                _tabHost.TabContentView.SetContentView(_currentTabContent);

                if (_selectedItemMember != null)
                    _selectedItemMember.SetValue(_tabHost, new[] { value.Item1 });
            }
            else
            {
                _currentTabContent = _tabHost.CurrentTabView;
                if (_selectedItemMember != null)
                    _selectedItemMember.SetValue(_tabHost, new object[] { _tabHost.CurrentTabView });
            }
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

        private TabHost.TabSpec CreateTabSpec(int index, Dictionary<object, KeyValuePair<string, Tuple<object, Func<object, object>, TabHost.TabSpec>>> oldValues)
        {
            object item = GetItem(index);
            KeyValuePair<string, Tuple<object, Func<object, object>, TabHost.TabSpec>> pair;
            if (oldValues != null && oldValues.TryGetValue(item, out pair))
            {
                _tabToContent[pair.Key] = pair.Value;
                return pair.Value.Item3;
            }

            string id = Guid.NewGuid().ToString("n");
            var spec = _tabHost.NewTabSpec(id);
            var value = new Tuple<object, Func<object, object>, TabHost.TabSpec>(item,
                new ContentSelector(this).GetContent, spec);
            _tabToContent[id] = value;
            SetIndicator(spec, GetItem(index));
            spec.SetContent(_tabFactory);
            BindingProvider.Instance.ContextManager.GetBindingContext(spec).DataContext = item;
            return spec;
        }

        private void TryClearCurrentView(object view)
        {
#if !API8
            var fragment = view as Fragment;
            if (fragment == null)
                return;
            var fragmentManager = _tabHost.GetFragmentManager();
            if (fragmentManager == null)
                return;
            fragmentManager.BeginTransaction().Remove(fragment).Commit();
            fragmentManager.ExecutePendingTransactions();
#endif
        }

        #endregion
    }
}