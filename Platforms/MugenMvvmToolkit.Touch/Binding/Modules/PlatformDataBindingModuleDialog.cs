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
using Foundation;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Infrastructure;

namespace MugenMvvmToolkit.Binding.Modules
{
    public partial class PlatformDataBindingModule
    {
        #region Methods

        private static void RegisterDialogMembers(IBindingMemberProvider memberProvider)
        {
            memberProvider.Register(AttachedBindingMember.CreateMember<Element, object>(AttachedMemberConstants.Parent,
                (info, element) => element.Parent ?? BindingExtensions.AttachedParentMember.GetValue(element, null),
                (info, element, arg3) => BindingExtensions.AttachedParentMember.SetValue(element, arg3),
                (info, element, arg3) => BindingExtensions.AttachedParentMember.TryObserve(element, arg3)));

            IBindingMemberInfo member = memberProvider.GetBindingMember(typeof(EntryElement), "Changed", true, false);
            if (member != null)
                memberProvider.Register(AttachedBindingMember.CreateEvent<EntryElement>("ValueChanged",
                    (info, element, arg3) => member.TryObserve(element, arg3)));

            memberProvider.Register(AttachedBindingMember.CreateEvent<StringElement>("Tapped",
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