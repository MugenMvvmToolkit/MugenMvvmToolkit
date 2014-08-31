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
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Infrastructure.Mediators;
using MugenMvvmToolkit.Interfaces.ViewModels;

namespace MugenMvvmToolkit.Models
{
    public sealed class ActionBarTabTemplate
    {
        #region Nested types

        private sealed class TabListener : Java.Lang.Object, ActionBar.ITabListener
        {
            #region Fields

            private object _content;

            #endregion

            #region Methods

            public void Clear(ActionBar bar, ActionBar.Tab tab)
            {
                var fragment = _content as Fragment;
                if (fragment != null)
                {
                    var viewModel = BindingServiceProvider.ContextManager.GetBindingContext(fragment).Value as IViewModel;
                    if (viewModel != null)
                        viewModel.Settings.Metadata.Remove(MvvmFragmentMediator.StateNotNeeded);
                    var manager = fragment.FragmentManager;
                    if (manager != null)
                        manager.BeginTransaction()
                               .Remove(fragment)
                               .Commit();
                }

                tab.SetTabListener(null);
                BindingExtensions.AttachedParentMember.SetValue(tab, BindingExtensions.NullValue);
            }

            private void SetContent(ActionBar.Tab tab, FragmentTransaction ft, int? placeHolder, Activity activity, ActionBar bar)
            {
                if (placeHolder == null)
                {
                    Tracer.Error("The placeholder for tab {0} was not found.", tab);
                    return;
                }
                var layout = activity.FindViewById<ViewGroup>(placeHolder.Value);
                if (layout == null)
                {
                    Tracer.Warn("The ActionBarTabContentId with id = {0} is not found in activity {1}",
                        placeHolder.Value,
                        activity);
                    return;
                }
                if (_content == null)
                {
                    _content = PlatformDataBindingModule.ActionBarTabContentMember.GetValue(tab, null);
                    var viewModel = _content as IViewModel;
                    if (viewModel == null)
                    {
                        var fragmentClass = _content as string;
                        //If content is a string, trying to create a fragment.
                        if (!string.IsNullOrEmpty(fragmentClass))
                        {
                            var type = TypeCache<Fragment>.Instance.GetTypeByName(fragmentClass, true, false);
                            if (type != null)
                            {
                                var fragment = Fragment.Instantiate(bar.ThemedContext, Java.Lang.Class.FromType(type).Name);
                                _content = fragment;
                            }
                        }
                        else if (_content is int)
                            _content = activity.CreateBindableView((int)_content).Item1;
                    }
                    else
                        viewModel.Settings.Metadata.AddOrUpdate(MvvmFragmentMediator.StateNotNeeded, true);
                    _content = PlatformExtensions.GetContentView(layout, layout.Context, _content,
                        ValueTemplateManager.GetTemplateId(tab, AttachedMemberConstants.ContentTemplate),
                        ValueTemplateManager.GetDataTemplateSelector(tab, AttachedMemberConstants.ContentTemplateSelector));
                    layout.SetContentView(_content, ft, (@group, fragment, arg3) =>
                    {
                        if (fragment.IsDetached)
                            arg3.Attach(fragment);
                        else
                            arg3.Replace(@group.Id, fragment);
                    });
                }
                else
                    layout.SetContentView(_content, ft, (@group, fragment, arg3) => arg3.Attach(fragment));
            }

            #endregion

            #region Implementation of ITabListener

            public void OnTabReselected(ActionBar.Tab tab, FragmentTransaction ft)
            {
            }

            public void OnTabSelected(ActionBar.Tab tab, FragmentTransaction ft)
            {
                var bar = (ActionBar)BindingExtensions.AttachedParentMember.GetValue(tab, null);
                var placeHolder = Views.ActionBar.GetTabContentId(bar);
                var activity = bar.ThemedContext.GetActivity();
                SetContent(tab, ft, placeHolder, activity, bar);
                //Set selected item data context or tab
                var selectedItem = ActionBarTabItemsSourceGenerator.Get(bar) == null
                    ? tab
                    : BindingServiceProvider.ContextManager.GetBindingContext(tab).Value;
                PlatformDataBindingModule
                    .ActionBarSelectedItemMember
                    .SetValue(bar, selectedItem);
            }

            public void OnTabUnselected(ActionBar.Tab tab, FragmentTransaction ft)
            {
                var fragment = _content as Fragment;
                if (fragment != null)
                {
                    ft.Detach(fragment);
                    return;
                }
                var view = _content as View;
                if (view == null)
                    return;
                var viewGroup = view.Parent as ViewGroup;
                if (viewGroup != null)
                    viewGroup.RemoveView(view);
            }

            #endregion
        }

        #endregion

        #region Fields

        private const string ListenerKey = "!~tablistener";

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
            var listener = ServiceProvider.AttachedValueProvider.GetValue<TabListener>(tab, ListenerKey, false);
            if (listener == null)
                return;
            ServiceProvider.AttachedValueProvider.Clear(tab, ListenerKey);
            listener.Clear(bar, tab);
            BindingServiceProvider.BindingManager.ClearBindings(tab);
        }

        private ActionBar.Tab CreateTabInternal(ActionBar bar, object context, bool useContext)
        {
            ActionBar.Tab newTab = bar.NewTab();
            BindingExtensions.AttachedParentMember.SetValue(newTab, bar);
            var setter = new XmlPropertySetter<ActionBarTabTemplate, ActionBar.Tab>(newTab, bar.ThemedContext);
            if (useContext)
                BindingServiceProvider.ContextManager.GetBindingContext(newTab).Value = context;
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
            var tabListener = new TabListener();
            ServiceProvider.AttachedValueProvider.SetValue(newTab, ListenerKey, tabListener);
            newTab.SetTabListener(tabListener);
            return newTab;
        }

        #endregion
    }
}