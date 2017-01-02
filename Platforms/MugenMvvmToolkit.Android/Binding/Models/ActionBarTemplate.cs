#region Copyright

// ****************************************************************************
// <copyright file="ActionBarTemplate.cs">
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

using System.Collections.Generic;
using Android.App;
using Android.OS;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.Android.Binding.Infrastructure;
using MugenMvvmToolkit.Android.Interfaces.Views;
using MugenMvvmToolkit.Models;

#if APPCOMPAT
using MugenMvvmToolkit.Android.Binding;
using ActionBar = Android.Support.V7.App.ActionBar;
using ActionBarTabItemsSourceGenerator = MugenMvvmToolkit.Android.AppCompat.Infrastructure.ActionBarTabItemsSourceGenerator;

namespace MugenMvvmToolkit.Android.AppCompat.Models
#else

namespace MugenMvvmToolkit.Android.Binding.Models
#endif
{
    public sealed class ActionBarTemplate
    {
        #region Fields

        private static EventHandler<Activity, ValueEventArgs<Bundle>> _activityViewOnSaveInstanceStateDelegate;
        private const string SelectedTabIndexKey = "~@tabindex";
        internal const string TabContentIdKey = "!@tabcontentId";

        #endregion

        #region Properties

        public string DataContext { get; set; }

        public string Bind { get; set; }

        public string BackgroundDrawable { get; set; }

        public string CustomView { get; set; }

        public string DisplayHomeAsUpEnabled { get; set; }

        public string DisplayOptions { get; set; }

        public string DisplayShowCustomEnabled { get; set; }

        public string DisplayShowHomeEnabled { get; set; }

        public string DisplayShowTitleEnabled { get; set; }

        public string DisplayUseLogoEnabled { get; set; }

        public string HomeButtonEnabled { get; set; }

        public string HomeButtonClick { get; set; }

        public string BackButtonClick
        {
            get { return HomeButtonClick; }
            set { HomeButtonClick = value; }
        }

        public string Icon { get; set; }

        public string Logo { get; set; }

        public string NavigationMode { get; set; }

        public string SplitBackgroundDrawable { get; set; }

        public string StackedBackgroundDrawable { get; set; }

        public string IsShowing { get; set; }

        public string Subtitle { get; set; }

        public string Title { get; set; }

        public string Visible { get; set; }

        public string SelectedItem { get; set; }

        public string ItemsSource { get; set; }

        public string RestoreTabSelectedIndex { get; set; }

        public ActionBarTabTemplate TabTemplate { get; set; }

        public List<ActionBarTabTemplate> Items { get; set; }

        public string ContextActionBarVisible { get; set; }

        public string ContextActionBarTemplate { get; set; }

        #endregion

        #region Methods

        public void Apply(Activity activity)
        {
            PlatformExtensions.ValidateTemplate(ItemsSource, Items);
            var actionBar = activity.GetActionBar();

            var setter = new XmlPropertySetter(actionBar, activity);
            setter.SetEnumProperty<ActionBarNavigationMode>(nameof(NavigationMode), NavigationMode);
            setter.SetProperty(nameof(DataContext), DataContext);

            if (!string.IsNullOrEmpty(Bind))
                setter.Bind(actionBar, Bind);
            setter.SetProperty(nameof(ContextActionBarTemplate), ContextActionBarTemplate);
            setter.SetBinding(nameof(ContextActionBarVisible), ContextActionBarVisible, false);
            setter.SetProperty(nameof(BackgroundDrawable), BackgroundDrawable);
            setter.SetProperty(nameof(CustomView), CustomView);
            setter.SetEnumProperty<ActionBarDisplayOptions>(nameof(DisplayOptions), DisplayOptions);
            setter.SetBoolProperty(nameof(DisplayHomeAsUpEnabled), DisplayHomeAsUpEnabled);
            setter.SetBoolProperty(nameof(DisplayShowCustomEnabled), DisplayShowCustomEnabled);
            setter.SetBoolProperty(nameof(DisplayShowHomeEnabled), DisplayShowHomeEnabled);
            setter.SetBoolProperty(nameof(DisplayShowTitleEnabled), DisplayShowTitleEnabled);
            setter.SetBoolProperty(nameof(DisplayUseLogoEnabled), DisplayUseLogoEnabled);
            setter.SetBoolProperty(nameof(HomeButtonEnabled), HomeButtonEnabled);
            setter.SetProperty(nameof(Icon), Icon);
            setter.SetProperty(nameof(Logo), Logo);
            setter.SetProperty(nameof(SplitBackgroundDrawable), SplitBackgroundDrawable);
            setter.SetProperty(nameof(StackedBackgroundDrawable), StackedBackgroundDrawable);
            setter.SetBoolProperty(nameof(IsShowing), IsShowing);
            setter.SetStringProperty(nameof(Subtitle), Subtitle);
            setter.SetStringProperty(nameof(Title), Title);
            setter.SetBoolProperty(nameof(Visible), Visible);
            setter.SetBinding(AttachedMembers.ActionBar.HomeButtonClick, HomeButtonClick, false);

            if (string.IsNullOrEmpty(ItemsSource))
            {
                if (Items != null)
                {
                    ActionBar.Tab firstTab = null;
                    for (int index = 0; index < Items.Count; index++)
                    {
                        var tab = Items[index].CreateTab(actionBar);
                        if (firstTab == null)
                            firstTab = tab;
                        actionBar.AddTab(tab);
                    }
                    TryRestoreSelectedIndex(activity, actionBar);
                }
            }
            else
            {
#if APPCOMPAT
                actionBar.SetBindingMemberValue(AttachedMembersCompat.ActionBar.ItemsSourceGenerator, new ActionBarTabItemsSourceGenerator(actionBar, TabTemplate));
#else
                actionBar.SetBindingMemberValue(AttachedMembers.ActionBar.ItemsSourceGenerator, new ActionBarTabItemsSourceGenerator(actionBar, TabTemplate));
#endif
                setter.SetBinding(nameof(ItemsSource), ItemsSource, false);
            }
            setter.SetBinding(nameof(SelectedItem), SelectedItem, false);
            setter.Apply();
        }

        public static void Clear(Activity activity)
        {
            var actionBar = activity.GetActionBar(false);
            if (actionBar == null)
                return;
            for (int i = 0; i < actionBar.TabCount; i++)
                ActionBarTabTemplate.ClearTab(actionBar, actionBar.GetTabAt(i), false);
            actionBar.ClearBindings(true, true);
        }

        public static int? GetTabContentId(ActionBar actionBar)
        {
            int value;
            if (ServiceProvider.AttachedValueProvider.TryGetValue(actionBar, TabContentIdKey, out value))
                return value;
            return null;
        }

        private void TryRestoreSelectedIndex(Activity activity, ActionBar actionBar)
        {
            if (actionBar.GetNavigationMode() == ActionBarNavigationMode.Standard)
                return;
            bool result;
            if (bool.TryParse(RestoreTabSelectedIndex, out result) && !result)
                return;
            var activityView = activity as IActivityView;
            if (activityView == null)
                return;
            if (_activityViewOnSaveInstanceStateDelegate == null)
                _activityViewOnSaveInstanceStateDelegate = ActivityViewOnSaveInstanceState;
            activityView.Mediator.SaveInstanceState += _activityViewOnSaveInstanceStateDelegate;

            var bundle = activityView.Mediator.Bundle;
            if (bundle != null)
            {
                var i = bundle.GetInt(SelectedTabIndexKey, int.MinValue);
                if (i != int.MinValue && i != actionBar.SelectedNavigationIndex)
                    actionBar.SetSelectedNavigationItem(i);
            }
        }

        private static void ActivityViewOnSaveInstanceState(Activity sender, ValueEventArgs<Bundle> args)
        {
            var actionBar = sender.GetActionBar();
            if (actionBar.IsAlive())
            {
                var index = actionBar.SelectedNavigationIndex;
                args.Value.PutInt(SelectedTabIndexKey, index);
            }
        }

        #endregion
    }
}
