#region Copyright
// ****************************************************************************
// <copyright file="ActionBarTemplate.cs">
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
using System.Collections.Generic;
using System.Xml.Serialization;
using Android.App;
using Android.Support.V7.App;
using MugenMvvmToolkit.Infrastructure;

namespace MugenMvvmToolkit.Models
{
    [XmlRoot("ACTIONBAR")]
    public sealed class ActionBarTemplate
    {
        #region Properties

        [XmlAttribute("TABCONTENTID")]
        public string TabContentId { get; set; }

        [XmlAttribute("DATACONTEXT")]
        public string DataContext { get; set; }

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
            var setter = new XmlPropertySetter<ActionBarTemplate, ActionBar>(actionBar, activity);
            setter.SetEnumProperty<ActionBarNavigationMode>(template => template.NavigationMode, NavigationMode);
            setter.SetProperty(template => template.TabContentId, TabContentId);
            setter.SetProperty(template => template.DataContext, DataContext);
            if (string.IsNullOrEmpty(ItemsSource))
            {
                if (Tabs != null)
                {
                    for (int index = 0; index < Tabs.Count; index++)
                    {
                        var tab = Tabs[index].CreateTab(actionBar);
                        actionBar.AddTab(tab, index);
                    }
                }
            }
            else
            {
                AttachedMembersModule.ActionBarTabItemsSourceGeneratorMember.SetValue(actionBar, new object[]
                {
                    new ActionBarTabItemsSourceGenerator(actionBar, TabTemplate)
                });
                setter.SetBinding(template => template.ItemsSource, ItemsSource, false);
            }

            setter.SetProperty(template => template.ContextActionBarTemplate, ContextActionBarTemplate);
            setter.SetBinding(template => template.ContextActionBarVisible, ContextActionBarVisible, false);

            setter.SetBinding(template => template.SelectedItem, SelectedItem, false);
            setter.SetProperty(template => template.BackgroundDrawable, BackgroundDrawable);
            setter.SetProperty(template => template.CustomView, CustomView);
            setter.SetEnumProperty<ActionBarDisplayOptions>(template => template.DisplayOptions, DisplayOptions);
            setter.SetBoolProperty(template => template.DisplayHomeAsUpEnabled, DisplayHomeAsUpEnabled);
            setter.SetBoolProperty(template => template.DisplayShowCustomEnabled, DisplayShowCustomEnabled);
            setter.SetBoolProperty(template => template.DisplayShowHomeEnabled, DisplayShowHomeEnabled);
            setter.SetBoolProperty(template => template.DisplayShowTitleEnabled, DisplayShowTitleEnabled);
            setter.SetBoolProperty(template => template.DisplayUseLogoEnabled, DisplayUseLogoEnabled);
            setter.SetBoolProperty(template => template.HomeButtonEnabled, HomeButtonEnabled);
            setter.SetProperty(template => template.Icon, Icon);
            setter.SetProperty(template => template.Logo, Logo);
            setter.SetProperty(template => template.SplitBackgroundDrawable, SplitBackgroundDrawable);
            setter.SetProperty(template => template.StackedBackgroundDrawable, StackedBackgroundDrawable);
            setter.SetBoolProperty(template => template.IsShowing, IsShowing);
            setter.SetStringProperty(template => template.Subtitle, Subtitle);
            setter.SetStringProperty(template => template.Title, Title);
            setter.SetBoolProperty(template => template.Visible, Visible);
        }

        #endregion
    }
}