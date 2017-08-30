#region Copyright

// ****************************************************************************
// <copyright file="ActionBarTabTemplate.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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

using System.Linq;
using Android.App;
using Android.Views;
using MugenMvvmToolkit.Android.Binding.Infrastructure;
using MugenMvvmToolkit.Android.Infrastructure;
using MugenMvvmToolkit.Android.Models;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces.ViewModels;
#if APPCOMPAT
using MugenMvvmToolkit.Android.Binding;
using Fragment = Android.Support.V4.App.Fragment;
using FragmentTransaction = Android.Support.V4.App.FragmentTransaction;
using ActionBar = Android.Support.V7.App.ActionBar;

namespace MugenMvvmToolkit.Android.AppCompat.Models
#else

namespace MugenMvvmToolkit.Android.Binding.Models
#endif
{
    public sealed class ActionBarTabTemplate
    {
        #region Nested types

        private sealed class TabListener : Java.Lang.Object, ActionBar.ITabListener
        {
            #region Fields

            private object _content;
            private readonly DataTemplateProvider _contentTemplateProvider;
            private bool _cleared;

            #endregion

            #region Constructors

            public TabListener(DataTemplateProvider contentTemplateProvider)
            {
                _contentTemplateProvider = contentTemplateProvider;
            }

            #endregion

            #region Methods

            public void Clear(ActionBar bar, ActionBar.Tab tab)
            {
                var fragment = _content as Fragment;
                if (fragment != null)
                {
                    (fragment.DataContext() as IViewModel)?.Settings.Metadata.Remove(ViewModelConstants.StateNotNeeded);
                    fragment.FragmentManager
                        ?.BeginTransaction()
                        .Remove(fragment)
                        .CommitAllowingStateLoss();
                }
                tab.SetBindingMemberValue(AttachedMembersBase.Object.Parent, value: null);
                _cleared = true;
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
#if APPCOMPAT
                    _content = tab.GetBindingMemberValue(AttachedMembersCompat.ActionBarTab.Content);
#else
                    _content = tab.GetBindingMemberValue(AttachedMembers.ActionBarTab.Content);
#endif
                    LayoutInflaterResult result = null;
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
                        {
                            result = activity.GetBindableLayoutInflater().InflateEx((int)_content, null, false);
                            _content = result.View;
                        }
                    }
                    else
                        viewModel.Settings.Metadata.AddOrUpdate(ViewModelConstants.StateNotNeeded, true);
                    _content = AndroidToolkitExtensions.GetContentView(layout, layout.Context, _content,
                        _contentTemplateProvider.GetTemplateId(), _contentTemplateProvider.GetDataTemplateSelector());
                    if (BindingServiceProvider.BindingManager.GetBindings(tab, AttachedMembersBase.Object.DataContext).Any())
                        _content.SetBindingMemberValue(AttachedMembersBase.Object.Parent, tab);
                    layout.SetContentView(_content, ft, (group, fragment, arg3) =>
                    {
                        if (fragment.IsDetached)
                            arg3.Attach(fragment);
                        else
                            arg3.Replace(group.Id, fragment);
                    });
                    result?.ApplyBindings();
                }
                else
                    layout.SetContentView(_content, ft, (group, fragment, arg3) => arg3.Attach(fragment));
            }

            #endregion

            #region Implementation of ITabListener

            public void OnTabReselected(ActionBar.Tab tab, FragmentTransaction ft)
            {
            }

            public void OnTabSelected(ActionBar.Tab tab, FragmentTransaction ft)
            {
                if (_cleared)
                    return;
                var bar = (ActionBar)tab.GetBindingMemberValue(AttachedMembersBase.Object.Parent);
                var placeHolder = ActionBarTemplate.GetTabContentId(bar);
                var activity = bar.ThemedContext.GetActivity();
                SetContent(tab, ft, placeHolder, activity, bar);
                //Set selected item data context or tab
                var selectedItem = bar.GetBindingMemberValue(ItemsSourceGeneratorBase.MemberDescriptor) == null ? tab : tab.DataContext();
#if APPCOMPAT
                bar.SetBindingMemberValue(AttachedMembersCompat.ActionBar.SelectedItem, selectedItem);
#else
                bar.SetBindingMemberValue(AttachedMembers.ActionBar.SelectedItem, selectedItem);
#endif
            }

            public void OnTabUnselected(ActionBar.Tab tab, FragmentTransaction ft)
            {
                if (_cleared)
                    return;
                var fragment = _content as Fragment;
                if (fragment != null)
                {
                    ft.Detach(fragment);
                    return;
                }
                var view = _content as View;
                (view?.Parent as ViewGroup)?.RemoveView(view);
            }

            #endregion
        }

        #endregion

        #region Fields

        private const string ListenerKey = "!~tablistener";

        #endregion

        #region Properties

        public string DataContext { get; set; }

        public string Bind { get; set; }

        public string Content { get; set; }

        public string ContentTemplate { get; set; }

        public string ContentTemplateSelector { get; set; }

        public string ContentDescription { get; set; }

        public string CustomView { get; set; }

        public string Icon { get; set; }

        public string Text { get; set; }

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

        public static void ClearTab(ActionBar bar, ActionBar.Tab tab, bool removeFragment)
        {
            var listener = ServiceProvider.AttachedValueProvider.GetValue<TabListener>(tab, ListenerKey, false);
            if (listener == null)
                return;
            BindingServiceProvider.BindingManager.ClearBindings(tab);
            if (removeFragment)
                listener.Clear(bar, tab);
            tab.ClearBindings(true, true);
        }

        private ActionBar.Tab CreateTabInternal(ActionBar bar, object context, bool useContext)
        {
            var newTab = bar.NewTab();
            newTab.SetBindingMemberValue(AttachedMembersBase.Object.Parent, bar);

            var setter = new XmlPropertySetter(newTab, bar.ThemedContext);
            if (useContext)
                newTab.SetDataContext(context);
            else
                setter.SetProperty(nameof(DataContext), DataContext);
            if (!string.IsNullOrEmpty(Bind))
                setter.Bind(newTab, Bind);
            setter.SetBinding(nameof(ContentTemplateSelector), ContentTemplateSelector, false);
            setter.SetProperty(nameof(ContentTemplate), ContentTemplate);
            setter.SetProperty(nameof(Content), Content);
            setter.SetStringProperty(nameof(ContentDescription), ContentDescription);
            setter.SetProperty(nameof(CustomView), CustomView);
            setter.SetProperty(nameof(Icon), Icon);
            setter.SetProperty(nameof(Text), Text);
            setter.SetProperty(nameof(Tag), Tag);
            setter.Apply();

            var tabListener = new TabListener(new DataTemplateProvider(bar, AttachedMemberConstants.ContentTemplate,
                AttachedMemberConstants.ContentTemplateSelector));
            ServiceProvider.AttachedValueProvider.SetValue(newTab, ListenerKey, tabListener);
            newTab.SetTabListener(tabListener);
            return newTab;
        }

        #endregion
    }
}
