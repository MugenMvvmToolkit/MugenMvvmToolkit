using System.Collections;
using Android.Graphics.Drawables;
using Android.Support.Design.Widget;
using Android.Widget;
using Java.Lang;
using MugenMvvmToolkit.Android.Binding;
using MugenMvvmToolkit.Android.Binding.Infrastructure;
using MugenMvvmToolkit.Android.Design.Infrastructure;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Infrastructure;

namespace MugenMvvmToolkit.Android.Design
{
    public static class AttachedMembersRegistration
    {
        #region Fields

        private static readonly IBindingMemberProvider MemberProvider = BindingServiceProvider.MemberProvider;

        #endregion

        #region Methods

        private static void TabLayoutSetSelectedItem(IBindingMemberInfo bindingMemberInfo, TabLayout tabLayout, object selectedItem)
        {
            for (var i = 0; i < tabLayout.TabCount; i++)
            {
                var tab = tabLayout.GetTabAt(i);
                if (tab.DataContext() == selectedItem)
                {
                    if (tab.Position != tabLayout.SelectedTabPosition)
                        tab.Select();
                    return;
                }
            }
        }

        private static object TabLayoutGetSelectedItem(IBindingMemberInfo bindingMemberInfo, TabLayout tabLayout)
        {
            var p = tabLayout.SelectedTabPosition;
            if (p < 0)
                return null;
            return tabLayout.GetTabAt(p).DataContext();
        }

        private static void TabLayoutItemsSourceChanged(TabLayout view, AttachedMemberChangedEventArgs<IEnumerable> args)
        {
            var generator = view.GetBindingMemberValue(AttachedMembers.ViewGroup.ItemsSourceGenerator);
            if (generator == null)
            {
                generator = new TabLayoutItemsSourceGenerator(view);
                view.SetBindingMemberValue(AttachedMembers.ViewGroup.ItemsSourceGenerator, generator);
            }
            generator.SetItemsSource(args.NewValue);
        }

        private static void NavigationViewMenuTemplateChanged(NavigationView view, AttachedMemberChangedEventArgs<object> args)
        {
            view.Menu.ApplyMenuTemplate(args.NewValue, view.Context, view);
        }

        public static void RegisterNavigationViewMembers()
        {
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.View.MenuTemplate.Override<NavigationView>(), NavigationViewMenuTemplateChanged));
        }

        public static void RegisterTabLayoutMembers()
        {
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ViewGroup.ItemsSource.Override<TabLayout>(), TabLayoutItemsSourceChanged));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembersDesign.TabLayout.RestoreSelectedIndex));
            MemberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembersDesign.TabLayout.SelectedItem,
                TabLayoutGetSelectedItem, TabLayoutSetSelectedItem, (info, layout, arg3) =>
                {
                    arg3 = arg3.ToWeakEventListener();
                    layout.TabSelected += arg3.Handle;
                    layout.TabUnselected += arg3.Handle;
                    return WeakActionToken.Create(layout, arg3, (tabLayout, listener) =>
                    {
                        tabLayout.TabSelected -= listener.Handle;
                        tabLayout.TabUnselected -= listener.Handle;
                    });
                }));
        }

        public static void RegisterTabLayoutTabMembers()
        {
            MemberProvider.Register(AttachedBindingMember.CreateNotifiableMember<TabLayout.Tab, string>(nameof(TabLayout.Tab.Text),
                (info, tab) => tab.Text,
                (info, tab, arg3) =>
                {
                    tab.SetText(arg3);
                    return true;
                }));
            MemberProvider.Register(AttachedBindingMember.CreateNotifiableMember<TabLayout.Tab, string>(nameof(TabLayout.Tab.ContentDescription),
                (info, tab) => tab.ContentDescription,
                (info, tab, arg3) =>
                {
                    tab.SetContentDescription(arg3);
                    return true;
                }));
            MemberProvider.Register(AttachedBindingMember.CreateNotifiableMember<TabLayout.Tab, object>(nameof(TabLayout.Tab.Icon),
                (info, tab) => tab.Icon,
                (info, tab, arg3) =>
                {
                    if (arg3 is int)
                        tab.SetIcon((int)arg3);
                    else
                        tab.SetIcon((Drawable)arg3);
                    return true;
                }));
            MemberProvider.Register(AttachedBindingMember.CreateNotifiableMember<TabLayout.Tab, Object>(nameof(TabLayout.Tab.Tag),
                (info, tab) => tab.Tag,
                (info, tab, arg3) =>
                {
                    tab.SetTag(arg3);
                    return true;
                }));
        }

        public static void RegisterTextInputLayoutMembers()
        {
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty<EditText, string>(AndroidBindingErrorProvider.ErrorPropertyName,
                (text, args) =>
                {
                    var layout = text.Parent as TextInputLayout;
                    if (layout == null)
                        text.Error = args.NewValue;
                    else
                    {
                        text.Error = null;
                        layout.Error = args.NewValue;
                    }
                }, getDefaultValue: (text, info) => text.Error));
        }

        public static void RegisterSnakbarMembers()
        {
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembersDesign.Activity.SnackbarView));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembersDesign.Activity.SnackbarViewSelector));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembersDesign.Activity.SnackbarTemplateSelector));
        }

        #endregion
    }
}