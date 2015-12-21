#region Copyright

// ****************************************************************************
// <copyright file="ActionBarTemplate.cs">
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

using System.Collections.Generic;
using System.Xml.Serialization;
using Android.App;
using Android.OS;
using MugenMvvmToolkit.Android.Binding;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Builders;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.Android.Binding.Infrastructure;
using MugenMvvmToolkit.Android.Interfaces.Views;
#if APPCOMPAT
using ActionBar = Android.Support.V7.App.ActionBar;
using ActionBarTabItemsSourceGenerator = MugenMvvmToolkit.Android.AppCompat.Infrastructure.ActionBarTabItemsSourceGenerator;

namespace MugenMvvmToolkit.Android.AppCompat.Models
#else

namespace MugenMvvmToolkit.Android.Binding.Models
#endif
{
    [XmlRoot("ACTIONBAR")]
    public sealed class ActionBarTemplate
    {
        #region Fields

        private const string SelectedTabIndexKey = "~@tabindex";

        #endregion

        #region Properties

        [XmlAttribute("DATACONTEXT")]
        public string DataContext { get; set; }

        [XmlAttribute("BIND")]
        public string Bind { get; set; }

        [XmlAttribute("BACKGROUNDDRAWABLE")]
        public string BackgroundDrawable { get; set; }

        [XmlAttribute("CUSTOMVIEW")]
        public string CustomView { get; set; }

        [XmlAttribute("DISPLAYHOMEASUPENABLED")]
        public string DisplayHomeAsUpEnabled { get; set; }

        [XmlAttribute("DISPLAYOPTIONS")]
        public string DisplayOptions { get; set; }

        [XmlAttribute("DISPLAYSHOWCUSTOMENABLED")]
        public string DisplayShowCustomEnabled { get; set; }

        [XmlAttribute("DISPLAYSHOWHOMEENABLED")]
        public string DisplayShowHomeEnabled { get; set; }

        [XmlAttribute("DISPLAYSHOWTITLEENABLED")]
        public string DisplayShowTitleEnabled { get; set; }

        [XmlAttribute("DISPLAYUSELOGOENABLED")]
        public string DisplayUseLogoEnabled { get; set; }

        [XmlAttribute("HOMEBUTTONENABLED")]
        public string HomeButtonEnabled { get; set; }

        [XmlAttribute("HOMEBUTTONCLICK")]
        public string HomeButtonClick { get; set; }

        [XmlAttribute("BACKBUTTONCLICK")]
        public string BackButtonClick
        {
            get { return HomeButtonClick; }
            set { HomeButtonClick = value; }
        }

        [XmlAttribute("ICON")]
        public string Icon { get; set; }

        [XmlAttribute("LOGO")]
        public string Logo { get; set; }

        [XmlAttribute("NAVIGATIONMODE")]
        public string NavigationMode { get; set; }

        [XmlAttribute("SPLITBACKGROUNDDRAWABLE")]
        public string SplitBackgroundDrawable { get; set; }

        [XmlAttribute("STACKEDBACKGROUNDDRAWABLE")]
        public string StackedBackgroundDrawable { get; set; }

        [XmlAttribute("ISSHOWING")]
        public string IsShowing { get; set; }

        [XmlAttribute("SUBTITLE")]
        public string Subtitle { get; set; }

        [XmlAttribute("TITLE")]
        public string Title { get; set; }

        [XmlAttribute("VISIBLE")]
        public string Visible { get; set; }

        [XmlAttribute("SELECTEDITEM")]
        public string SelectedItem { get; set; }

        [XmlAttribute("ITEMSSOURCE")]
        public string ItemsSource { get; set; }

        [XmlElement("RESTORETABSELECTEDINDEX")]
        public string RestoreTabSelectedIndex { get; set; }

        [XmlElement("TABTEMPLATE")]
        public ActionBarTabTemplate TabTemplate { get; set; }

        [XmlElement("TAB")]
        public List<ActionBarTabTemplate> Tabs { get; set; }

        [XmlAttribute("CONTEXTACTIONBARVISIBLE")]
        public string ContextActionBarVisible { get; set; }

        [XmlAttribute("CONTEXTACTIONBARTEMPLATE")]
        public string ContextActionBarTemplate { get; set; }

        #endregion

        #region Methods

        public void Apply(Activity activity)
        {
            PlatformExtensions.ValidateTemplate(ItemsSource, Tabs);
            var actionBar = activity.GetActionBar();

            var setter = new XmlPropertySetter<ActionBarTemplate, ActionBar>(actionBar, activity, new BindingSet());
            setter.SetEnumProperty<ActionBarNavigationMode>(nameof(NavigationMode), NavigationMode);
            setter.SetProperty(nameof(DataContext), DataContext);

            if (!string.IsNullOrEmpty(Bind))
                setter.BindingSet.BindFromExpression(actionBar, Bind);
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
                if (Tabs != null)
                {
                    ActionBar.Tab firstTab = null;
                    for (int index = 0; index < Tabs.Count; index++)
                    {
                        var tab = Tabs[index].CreateTab(actionBar);
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
            activityView.Mediator.SaveInstanceState += ActivityViewOnSaveInstanceState;

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
            if (actionBar == null)
                return;
            var index = actionBar.SelectedNavigationIndex;
            args.Value.PutInt(SelectedTabIndexKey, index);
        }

        #endregion
    }
}
