#region Copyright
// ****************************************************************************
// <copyright file="ActionBarTabItemsSourceGenerator.cs">
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

using System.Collections;
using Android.App;
using Android.Widget;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
#if APPCOMPAT
using Android.Support.V7.App;
using MugenMvvmToolkit.AppCompat.Models;
using MugenMvvmToolkit.AppCompat.Modules;
using MugenMvvmToolkit.AppCompat.Views;
using ActionBar = Android.Support.V7.App.ActionBar;

namespace MugenMvvmToolkit.AppCompat.Infrastructure
#else
using MugenMvvmToolkit.ActionBarSupport.Models;
using MugenMvvmToolkit.ActionBarSupport.Modules;
using MugenMvvmToolkit.ActionBarSupport.Views;

namespace MugenMvvmToolkit.ActionBarSupport.Infrastructure
#endif
{
    internal sealed class ActionBarTabItemsSourceGenerator : ItemsSourceGeneratorBase
    {
        #region Fields

        private readonly ActionBar _actionBar;
        private readonly ActionBarTabTemplate _tabTemplate;

        #endregion

        #region Constructors

        private ActionBarTabItemsSourceGenerator(ActionBar actionBar, ActionBarTabTemplate tabTemplate)
        {
            Should.NotBeNull(actionBar, "actionBar");
            Should.NotBeNull(tabTemplate, "tabTemplate");
            _actionBar = actionBar;
            _tabTemplate = tabTemplate;
            TryListenActivity(_actionBar.ThemedContext);
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
            for (int i = 0; i < count; i++)
            {
                int index = insertionIndex + i;
                var tab = CreateTab(index);
                _actionBar.AddTab(tab, index, false);
            }
        }

        protected override void Remove(int removalIndex, int count)
        {
            for (int i = 0; i < count; i++)
            {
                int index = removalIndex + i;
                Remove(index);
            }
            if (_actionBar.TabCount == 0)
                OnEmptyTab();
        }

        protected override void Replace(int startIndex, int count)
        {
            for (int i = 0; i < count; i++)
            {
                int index = startIndex + i;
                Remove(index);
                var tab = CreateTab(index);
                _actionBar.AddTab(tab, index, false);
            }
        }

        protected override void Refresh()
        {
            for (int i = 0; i < _actionBar.TabCount; i++)
                ActionBarTabTemplate.ClearTab(_actionBar, _actionBar.GetTabAt(i), true);
            _actionBar.RemoveAllTabs();

            IEnumerable itemsSource = ItemsSource;
            if (itemsSource == null)
                return;

            var selectedItem = ActionBarModule.ActionBarSelectedItemMember.GetValue(_actionBar, null);

            int count = itemsSource.Count();
            for (int index = 0; index < count; index++)
            {
                var item = GetItem(index);
                var tab = CreateTab(item);
                _actionBar.AddTab(tab, index, ReferenceEquals(selectedItem, item));
            }
            if (count == 0)
                OnEmptyTab();
        }

        #endregion

        #region Methods

        public static void Set(ActionBar actionBar, ActionBarTabTemplate tabTemplate)
        {
            ServiceProvider.AttachedValueProvider.SetValue(actionBar, Key, new ActionBarTabItemsSourceGenerator(actionBar, tabTemplate));
        }

        public void SetSelectedItem(object selectedItem, IDataContext context = null)
        {
            IBindingContextManager contextManager = BindingServiceProvider.ContextManager;
            for (int i = 0; i < _actionBar.TabCount; i++)
            {
                var tab = _actionBar.GetTabAt(i);
                if (contextManager.GetBindingContext(tab).Value == selectedItem)
                {
                    if (tab.Position != _actionBar.SelectedNavigationIndex)
                        tab.Select();
                    return;
                }
            }
        }

        private void OnEmptyTab()
        {
            ActionBarModule
                .ActionBarSelectedItemMember
                .SetValue(_actionBar, BindingExtensions.NullValue);
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

        #endregion
    }
}