#region Copyright
// ****************************************************************************
// <copyright file="PlatformDataBindingModule.cs">
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
using System;
using System.Linq;
using System.Reflection;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Converters;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using Xamarin.Forms;

namespace MugenMvvmToolkit
{
    public class PlatformDataBindingModule : DataBindingModule
    {
        #region Methods

        private static void Register(IBindingMemberProvider memberProvider)
        {
            //Element
            memberProvider.Register(AttachedBindingMember
                .CreateMember<Element, object>(AttachedMemberConstants.Parent, GetParentValue, null, ObserveParentMember));
            memberProvider.Register(typeof(Element), "BindingContext", BindingMemberProvider.BindingContextMember, true);

            //VisualElement
            var visibleMember = memberProvider.GetBindingMember(typeof(VisualElement),
                ToolkitExtensions.GetMemberName<VisualElement>(element => element.IsVisible), true, false);
            if (visibleMember != null)
            {
                memberProvider.Register(typeof(VisualElement), "Visible", visibleMember, true);
                memberProvider.Register(AttachedBindingMember.CreateMember<VisualElement, bool>("Hidden",
                    (info, element) => !element.IsVisible, (info, element, arg3) => element.IsVisible = !arg3,
                    (info, element, arg3) => visibleMember.TryObserve(element, arg3)));
            }
            memberProvider.Register(AttachedBindingMember
                .CreateMember<VisualElement, object>(AttachedMemberConstants.FindByNameMethod, FindByNameMemberImpl));

            memberProvider.Register(AttachedBindingMember.CreateMember<VisualElement, bool>(AttachedMemberConstants.Focused, (info, element) => element.IsFocused,
                (info, element, arg3) =>
                {
                    if (arg3)
                        element.Focus();
                    else
                        element.Unfocus();
                }, (info, element, arg3) => BindingServiceProvider.WeakEventManager.Subscribe(element, "IsFocused", arg3)));

            var enabledMember = memberProvider.GetBindingMember(typeof(VisualElement),
                ToolkitExtensions.GetMemberName<VisualElement>(element => element.IsEnabled), true, false);
            if (enabledMember != null)
                memberProvider.Register(typeof(VisualElement), AttachedMemberConstants.Enabled, enabledMember, true);

            //Toolbar item
            enabledMember = memberProvider.GetBindingMember(typeof(ToolbarItem), "IsEnabled", true, false);
            if (enabledMember != null)
                memberProvider.Register(typeof(ToolbarItem), AttachedMemberConstants.Enabled, enabledMember, true);
        }

        private static object FindByNameMemberImpl(IBindingMemberInfo bindingMemberInfo, VisualElement target, object[] arg3)
        {
            var name = (string)arg3[0];
            return target.FindByName<object>(name);
        }

        private static object GetParentValue(IBindingMemberInfo bindingMemberInfo, Element target)
        {
            return ParentObserver.GetOrAdd(target).Parent;
        }

        private static IDisposable ObserveParentMember(IBindingMemberInfo bindingMemberInfo, Element o, IEventListener arg3)
        {
            return ParentObserver.GetOrAdd(o).AddWithUnsubscriber(arg3);
        }

        #endregion

        #region Overrides of DataBindingModule

        /// <summary>
        ///    Occurs on load the current module.
        /// </summary>
        protected override void OnLoaded(IModuleContext context)
        {
            base.OnLoaded(context);
            Register(BindingServiceProvider.MemberProvider);
        }

        /// <summary>
        /// Tries to register type.
        /// </summary>
        protected override void RegisterType(Type type)
        {
            base.RegisterType(type);

            var typeInfo = type.GetTypeInfo();
            if (!typeof(IValueConverter).GetTypeInfo().IsAssignableFrom(typeInfo) || !type.IsPublicNonAbstractClass())
                return;
            var constructor = typeInfo.DeclaredConstructors.FirstOrDefault(info => !info.IsStatic && info.GetParameters().Length == 0);
            if (constructor == null || !constructor.IsPublic)
                return;
            var converter = (IValueConverter)constructor.InvokeEx();
            string name = RemoveTail(RemoveTail(type.Name, "ValueConverter"), "Converter");
            if (BindingServiceProvider.ResourceResolver.TryAddConverter(name,
                new ValueConverterWrapper(converter.Convert, converter.ConvertBack)))
                Tracer.Info("The {0} converter is registered.", type);
        }

        /// <summary>
        ///     Gets the <see cref="IBindingContextManager" /> that will be used by default.
        /// </summary>
        protected override IBindingContextManager GetBindingContextManager()
        {
            return new BindingContextManagerEx();
        }

        /// <summary>
        ///     Gets the <see cref="IBindingResourceResolver" /> that will be used by default.
        /// </summary>
        protected override IBindingResourceResolver GetBindingResourceResolver()
        {
            var resolver = BindingServiceProvider.ResourceResolver as BindingResourceResolver;
            return resolver == null
                ? new BindingResourceResolverEx()
                : new BindingResourceResolverEx(resolver);
        }

        #endregion
    }
}