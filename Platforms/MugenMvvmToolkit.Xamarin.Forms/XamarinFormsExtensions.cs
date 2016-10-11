#region Copyright

// ****************************************************************************
// <copyright file="XamarinFormsExtensions.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Models;
using Xamarin.Forms;

namespace MugenMvvmToolkit.Xamarin.Forms
{
    public static class XamarinFormsExtensions
    {
        #region Fields

        private const string NavParamKey = "@~`NavParam";
        private static readonly Dictionary<Type, IBindingMemberInfo> TypeToContentMember;

        #endregion

        #region Constructors

        static XamarinFormsExtensions()
        {
            TypeToContentMember = new Dictionary<Type, IBindingMemberInfo>();
        }

        #endregion

        #region Events

        public static event EventHandler<Page, CancelEventArgs> BackButtonPressed;

        public static Func<Page, Action> SendBackButtonPressed { get; set; }

        #endregion

        #region Methods

        public static bool HandleBackButtonPressed([NotNull] this Page page, Func<bool> baseOnBackButtonPressed = null)
        {
            Should.NotBeNull(page, nameof(page));
            var handler = BackButtonPressed;
            if (handler == null)
                return baseOnBackButtonPressed != null && baseOnBackButtonPressed();
            var args = new CancelEventArgs(false);
            handler(page, args);
            if (args.Cancel)
                return true;
            return baseOnBackButtonPressed != null && baseOnBackButtonPressed();
        }

        public static void SetNavigationParameter([NotNull] this Page controller, object value)
        {
            Should.NotBeNull(controller, nameof(controller));
            if (value == null)
                ServiceProvider.AttachedValueProvider.Clear(controller, NavParamKey);
            else
                ServiceProvider.AttachedValueProvider.SetValue(controller, NavParamKey, value);
        }

        public static object GetNavigationParameter([CanBeNull] this Page controller)
        {
            if (controller == null)
                return null;
            return ServiceProvider.AttachedValueProvider.GetValue<object>(controller, NavParamKey, false);
        }

        public static void ClearBindingsRecursively([CanBeNull] this BindableObject item, bool clearDataContext, bool clearAttachedValues)
        {
            if (item == null)
                return;
            var contentMember = GetContentMember(item.GetType());
            if (contentMember != null)
            {
                var content = contentMember.GetValue(item, null);
                if (!(content is string))
                {
                    var enumerable = content as IEnumerable;
                    if (enumerable == null)
                        ClearBindingsRecursively(content as BindableObject, clearDataContext, clearAttachedValues);
                    else
                    {
                        foreach (object child in enumerable)
                        {
                            var bindableObject = child as BindableObject;
                            if (child == null || bindableObject == null)
                                break;
                            bindableObject.ClearBindingsRecursively(clearDataContext, clearAttachedValues);
                        }
                    }
                }
            }
            item.ClearBindings(clearDataContext, clearAttachedValues);
        }

        internal static bool IsAssignableFrom([NotNull] this Type typeFrom, [NotNull] Type typeTo)
        {
            Should.NotBeNull(typeFrom, nameof(typeFrom));
            Should.NotBeNull(typeTo, nameof(typeTo));
            return typeFrom.GetTypeInfo().IsAssignableFrom(typeTo.GetTypeInfo());
        }

        internal static PlatformInfo GetPlatformInfo()
        {
            return new PlatformInfo(Device.OnPlatform(PlatformType.XamarinFormsiOS, PlatformType.XamarinFormsAndroid, PlatformType.XamarinFormsWinPhone), new Version(0, 0));
        }

        internal static void AsEventHandler<TArg>(this Action action, object sender, TArg arg)
        {
            action();
        }

        private static IBindingMemberInfo GetContentMember(Type type)
        {
            lock (TypeToContentMember)
            {
                IBindingMemberInfo info;
                if (!TypeToContentMember.TryGetValue(type, out info))
                {
                    var attribute = type
                        .GetTypeInfo()
                        .GetCustomAttribute<ContentPropertyAttribute>(true);
                    if (attribute != null)
                        info = BindingServiceProvider
                            .MemberProvider
                            .GetBindingMember(type, attribute.Name, true, false);
                    TypeToContentMember[type] = info;
                }
                return info;
            }
        }

        #endregion
    }
}
