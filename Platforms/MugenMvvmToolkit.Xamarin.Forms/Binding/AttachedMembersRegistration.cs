#region Copyright

// ****************************************************************************
// <copyright file="AttachedMembersRegistration.cs">
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

using System;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Xamarin.Forms.Binding.Models;
using Xamarin.Forms;

namespace MugenMvvmToolkit.Xamarin.Forms.Binding
{
    public static class AttachedMembersRegistration
    {
        #region Properties

        private static IBindingMemberProvider MemberProvider => BindingServiceProvider.MemberProvider;

        #endregion

        #region Methods

        public static void RegisterElementMembers()
        {
            MemberProvider.Register(AttachedBindingMember
                .CreateMember<Element, object>(AttachedMemberConstants.Parent, GetParentValue, SetParentValue, ObserveParentMember));
            MemberProvider.Register(typeof(Element), nameof(Element.BindingContext), BindingMemberProvider.BindingContextMember, true);
        }

        public static void RegisterVisualElementMembers()
        {
            var visibleMember = MemberProvider.GetBindingMember(typeof(VisualElement), nameof(VisualElement.IsVisible), true, false);
            if (visibleMember != null)
            {
                MemberProvider.Register(typeof(VisualElement), AttachedMembers.VisualElement.Visible, visibleMember, true);
                MemberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembers.VisualElement.Hidden,
                    (info, element) => !element.IsVisible, (info, element, arg3) => element.IsVisible = !arg3,
                    (info, element, arg3) => visibleMember.TryObserve(element, arg3)));
            }
            MemberProvider.Register(AttachedBindingMember
                .CreateMember<VisualElement, object>(AttachedMemberConstants.FindByNameMethod, FindByNameMemberImpl));

            MemberProvider.Register(AttachedBindingMember.CreateMember<VisualElement, bool>(AttachedMemberConstants.Focused, (info, element) => element.IsFocused,
                (info, element, arg3) =>
                {
                    if (arg3)
                        element.Focus();
                    else
                        element.Unfocus();
                }, (info, element, arg3) => BindingServiceProvider.WeakEventManager.Subscribe(element, nameof(VisualElement.IsFocused), arg3)));

            var enabledMember = MemberProvider.GetBindingMember(typeof(VisualElement), nameof(VisualElement.IsEnabled), true, false);
            if (enabledMember != null)
                MemberProvider.Register(typeof(VisualElement), AttachedMemberConstants.Enabled, enabledMember, true);
        }

        public static void RegisterToolbarItemMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember<ToolbarItem>(nameof(ToolbarItem.Clicked));
            var enabledMember = MemberProvider.GetBindingMember(typeof(ToolbarItem), "IsEnabled", true, false);
            if (enabledMember != null)
                MemberProvider.Register(typeof(ToolbarItem), AttachedMemberConstants.Enabled, enabledMember, true);
        }

        public static void RegisterEntryMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember<Entry>(nameof(Entry.Text));
        }

        public static void RegisterLabelMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember<Label>(nameof(Label.Text));
        }

        public static void RegisterButtonMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember<Button>(nameof(Button.Clicked));
        }

        public static void RegisterListViewMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember<ListView>(nameof(ListView.ItemsSource));
        }

        public static void RegisterProgressBarMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember<ProgressBar>(nameof(ProgressBar.Progress));
        }

        private static object FindByNameMemberImpl(IBindingMemberInfo bindingMemberInfo, VisualElement target, object[] arg3)
        {
            var name = (string) arg3[0];
            return target.FindByName<object>(name);
        }

        private static object GetParentValue(IBindingMemberInfo bindingMemberInfo, Element target)
        {
            return ParentObserver.GetOrAdd(target).Parent;
        }

        private static void SetParentValue(IBindingMemberInfo bindingMemberInfo, Element element, object arg3)
        {
            ParentObserver.GetOrAdd(element).Parent = arg3;
        }

        private static IDisposable ObserveParentMember(IBindingMemberInfo bindingMemberInfo, Element o, IEventListener arg3)
        {
            return ParentObserver.GetOrAdd(o).AddWithUnsubscriber(arg3);
        }

        #endregion
    }
}