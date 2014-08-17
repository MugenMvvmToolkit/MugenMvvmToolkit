#region Copyright
// ****************************************************************************
// <copyright file="ActionBarTabTemplate.cs">
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
using System.Xml.Serialization;
using Android.App;
using Android.Support.V4.App;
using Android.Support.V7.App;
using Android.Views;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Core;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Infrastructure.Mediators;
using MugenMvvmToolkit.Interfaces.ViewModels;

namespace MugenMvvmToolkit.Models
{
    public sealed class ActionBarTabTemplate : Java.Lang.Object, ActionBar.ITabListener
    {
        #region Fields

        private const string ContentInternalKey = "!$contint";

        #endregion

        #region Properties

        [XmlAttribute("DATACONTEXT")]
        public string DataContext { get; set; }

        [XmlAttribute("CONTENT")]
        public string Content { get; set; }

        [XmlAttribute("CONTENTTEMPLATE")]
        public string ContentTemplate { get; set; }

        [XmlAttribute("CONTENTTEMPLATESELECTOR")]
        public string ContentTemplateSelector { get; set; }

        [XmlAttribute("CONTENTDESCRIPTION")]
        public string ContentDescription { get; set; }

        [XmlAttribute("CUSTOMVIEW")]
        public string CustomView { get; set; }

        [XmlAttribute("ICON")]
        public string Icon { get; set; }

        [XmlAttribute("TEXT")]
        public string Text { get; set; }

        [XmlAttribute("TAG")]
        public string Tag { get; set; }

        #endregion

        #region Methods

        public ActionBar.Tab CreateTab(ActionBar bar)
        {
            return CreateTabInternal(bar, null, false);
        }

        public ActionBar.Tab CreateTab(ActionBar bar, object dataContext)
        {
            return CreateTabInternal(bar, dataContext, true);
        }

        public void ClearTab(ActionBar bar, ActionBar.Tab tab)
        {
            var fragment = ServiceProvider.AttachedValueProvider.GetValue<object>(tab, ContentInternalKey, false) as Fragment;
            if (fragment != null)
            {
                var viewModel = BindingProvider.Instance.ContextManager.GetBindingContext(fragment).Value as IViewModel;
                if (viewModel != null)
                    viewModel.Settings.Metadata.Remove(MvvmFragmentMediator.StateNotNeeded);
                fragment.FragmentManager
                    .BeginTransaction()
                    .Remove(fragment)
                    .Commit();
            }

            tab.SetTabListener(null);
            AttachedMembersModule
                .ActionBarTabParentMember
                .SetValue(tab, BindingExtensions.NullValue);
        }

        private ActionBar.Tab CreateTabInternal(ActionBar bar, object context, bool useContext)
        {
            ActionBar.Tab newTab = bar.NewTab();
            AttachedMembersModule.ActionBarTabParentMember.SetValue(newTab, new object[] { bar });
            var setter = new XmlPropertySetter<ActionBarTabTemplate, ActionBar.Tab>(newTab, bar.ThemedContext);
            if (useContext)
                BindingProvider.Instance.ContextManager.GetBindingContext(newTab).Value = context;
            else
                setter.SetProperty(template => template.DataContext, DataContext);
            setter.SetBinding(template => template.ContentTemplateSelector, ContentTemplateSelector, false);
            setter.SetProperty(template => template.ContentTemplate, ContentTemplate);
            setter.SetProperty(template => template.Content, Content);
            setter.SetStringProperty(template => template.ContentDescription, ContentDescription);
            setter.SetProperty(template => template.CustomView, CustomView);
            setter.SetProperty(template => template.Icon, Icon);
            setter.SetProperty(template => template.Text, Text);
            setter.SetProperty(template => template.Tag, Tag);
            newTab.SetTabListener(this);
            return newTab;
        }

        #endregion

        #region Implementation of ITabListener

        void ActionBar.ITabListener.OnTabReselected(ActionBar.Tab tab, FragmentTransaction ft)
        {
        }

        void ActionBar.ITabListener.OnTabSelected(ActionBar.Tab tab, FragmentTransaction ft)
        {
            var bar = AttachedMembersModule.ActionBarTabParentMember.GetValue(tab, null);
            var placeHolder = AttachedMembersModule.ActionBarTabContentIdMember.GetValue(bar, null);
            var activity = (Activity)bar.ThemedContext;
            if (placeHolder == null)
            {
                Tracer.Error("The placeholder for tab {0} was not found.", tab);
                return;
            }
            var content = AttachedMembersModule.ActionBarTabContentMember.GetValue(tab, null);
            var viewModel = content as IViewModel;
            if (viewModel != null)
                viewModel.Settings.Metadata.AddOrUpdate(MvvmFragmentMediator.StateNotNeeded, true);

            var fragmentClass = content as string;
            //If content is a string, trying to create a fragment.
            if (!string.IsNullOrEmpty(fragmentClass))
            {
                var type = TypeCache<Fragment>.Instance.GetTypeByName(fragmentClass, true, false);
                if (type != null)
                {
                    var fragment = Fragment.Instantiate(bar.ThemedContext, Java.Lang.Class.FromType(type).Name);
                    AttachedMembersModule.ActionBarTabContentMember.SetValue(tab, new object[] { fragment });
                    content = fragment;
                }
            }
            else if (content is int)
            {
                content = activity.CreateBindableView((int)content).Item1;
                AttachedMembersModule.ActionBarTabContentMember.SetValue(tab, new[] { content });
            }

            var layout = activity.FindViewById<ViewGroup>(placeHolder.Value);
            if (layout == null)
            {
                Tracer.Warn("The ActionBarTabContentId with id = {0} is not found in activity {1}", placeHolder.Value,
                    activity);
                content = null;
            }
            else
                content = layout.SetContentView(content,
                    ValueTemplateManager.GetTemplateId(tab, AttachedMemberConstants.ContentTemplate),
                    ValueTemplateManager.GetDataTemplateSelector(tab, AttachedMemberConstants.ContentTemplateSelector), ft);

            //Set selected item data context or tab
            AttachedMembersModule
                .ActionBarSelectedItemMember
                .SetValue(bar, ActionBarTabItemsSourceGenerator.Get(bar) == null
                    ? new object[] { tab }
                    : new[] { BindingProvider.Instance.ContextManager.GetBindingContext(tab).Value });

            ServiceProvider.AttachedValueProvider.SetValue(tab, ContentInternalKey, content);
        }

        void ActionBar.ITabListener.OnTabUnselected(ActionBar.Tab tab, FragmentTransaction ft)
        {
            var fragment = ServiceProvider.AttachedValueProvider.GetValue<object>(tab, ContentInternalKey, false) as Fragment;
            if (fragment != null)
                ft.Remove(fragment);
        }

        #endregion
    }
}