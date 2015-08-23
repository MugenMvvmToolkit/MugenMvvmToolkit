#region Copyright

// ****************************************************************************
// <copyright file="PlatformDataBindingModuleDialog.cs">
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

using System;
using MonoTouch.Dialog;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.iOS.MonoTouch.Dialog;

namespace MugenMvvmToolkit.iOS.Binding.Modules
{
    public partial class PlatformDataBindingModule
    {
        #region Methods

        private static void RegisterDialogMembers(IBindingMemberProvider memberProvider)
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember<Element>(() => e => e.Caption);
            memberProvider.Register(AttachedBindingMember.CreateMember<Element, string>("Caption",
                (info, element) => element.Caption,
                (info, element, arg3) =>
                {
                    element.Caption = arg3;
                    element.Reload();
                }));
            memberProvider.Register(AttachedBindingMember.CreateMember<Element, object>(AttachedMemberConstants.ParentExplicit,
                (info, element) => element.Parent, null));

            BindingBuilderExtensions.RegisterDefaultBindingMember<EntryElement>(() => e => e.Value);
            IBindingMemberInfo member = memberProvider.GetBindingMember(typeof(EntryElement), "Changed", true, false);
            if (member != null)
                memberProvider.Register(AttachedBindingMember.CreateEvent<EntryElement>("ValueChanged",
                    (info, element, arg3) => member.TryObserve(element, arg3)));

            BindingBuilderExtensions.RegisterDefaultBindingMember<StringElement>(() => e => e.Value);
            memberProvider.Register(AttachedBindingMember.CreateMember<StringElement, string>("Value",
                (info, element) => element.Value,
                (info, element, arg3) =>
                {
                    element.Value = arg3;
                    element.Reload();
                }));
            memberProvider.Register(AttachedBindingMember.CreateEvent(AttachedMembers.StringElement.TappedEvent,
                (info, element, arg3) =>
                {
                    var weakWrapper = arg3.ToWeakWrapper();
                    IDisposable unsubscriber = null;
                    Action action = () =>
                    {
                        if (!weakWrapper.EventListener.TryHandle(weakWrapper.EventListener, EventArgs.Empty))
                            unsubscriber.Dispose();
                    };
                    unsubscriber = WeakActionToken.Create(element, action,
                        (stringElement, nsAction) => stringElement.Tapped -= nsAction);
                    element.Tapped += action;
                    return unsubscriber;
                }));
        }

        #endregion
    }
}