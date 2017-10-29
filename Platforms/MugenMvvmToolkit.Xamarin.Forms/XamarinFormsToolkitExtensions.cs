#region Copyright

// ****************************************************************************
// <copyright file="XamarinFormsToolkitExtensions.cs">
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
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using Xamarin.Forms;

namespace MugenMvvmToolkit.Xamarin.Forms
{
    public static class XamarinFormsToolkitExtensions
    {
        #region Fields

        private const string NavParamKey = nameof(NavParamKey);
        private const string NavContextKey = nameof(NavContextKey);
        private const string NavContextBackKey = NavContextKey + "Back";
        private const string NavBringToFrontKey = nameof(NavBringToFrontKey);

        #endregion

        #region Constructors

        static XamarinFormsToolkitExtensions()
        {
            BindingCultureInfo = () => CultureInfo.CurrentCulture;
        }

        #endregion

        #region Properties

        public static bool IsDesignMode => Application.Current == null || ToolkitServiceProvider.IsDesignModeRaw.GetValueOrDefault();

        public static Func<CultureInfo> BindingCultureInfo { get; set; }

        public static Func<MemberInfo, Type, object, object> ValueConverter { get; set; }

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

        public static void SetNavigationParameter([NotNull] this Page page, string value)
        {
            Should.NotBeNull(page, nameof(page));
            if (value == null)
                ToolkitServiceProvider.AttachedValueProvider.Clear(page, NavParamKey);
            else
                ToolkitServiceProvider.AttachedValueProvider.SetValue(page, NavParamKey, value);
        }

        public static string GetNavigationParameter([CanBeNull] this Page page)
        {
            if (page == null)
                return null;
            return ToolkitServiceProvider.AttachedValueProvider.GetValue<string>(page, NavParamKey, false);
        }

        internal static bool IsAssignableFrom([NotNull] this Type typeFrom, [NotNull] Type typeTo)
        {
            Should.NotBeNull(typeFrom, nameof(typeFrom));
            Should.NotBeNull(typeTo, nameof(typeTo));
            return typeFrom.GetTypeInfo().IsAssignableFrom(typeTo.GetTypeInfo());
        }

        internal static void AsEventHandler<TArg>(this Action action, object sender, TArg arg)
        {
            action();
        }

        internal static void SetBringToFront([NotNull] this Page page, bool value)
        {
            Should.NotBeNull(page, nameof(page));
            ToolkitServiceProvider.AttachedValueProvider.SetValue(page, NavBringToFrontKey, Empty.BooleanToObject(value));
        }

        internal static bool GetBringToFront(this Page page)
        {
            if (page == null)
                return false;
            var value = ToolkitServiceProvider.AttachedValueProvider.GetValue<bool>(page, NavBringToFrontKey, false);
            ToolkitServiceProvider.AttachedValueProvider.Clear(page, NavBringToFrontKey);
            return value;
        }

        internal static void SetNavigationContext([NotNull] this Page page, IDataContext value, bool isBack)
        {
            Should.NotBeNull(page, nameof(page));
            ToolkitServiceProvider.AttachedValueProvider.SetValue(page, isBack ? NavContextBackKey : NavContextKey, value);
        }

        internal static IDataContext GetNavigationContext(this Page page, bool isBack, bool remove)
        {
            if (page == null)
                return null;
            var key = isBack ? NavContextBackKey : NavContextKey;
            var dataContext = ToolkitServiceProvider.AttachedValueProvider.GetValue<IDataContext>(page, key, false);
            if (dataContext != null && remove)
                ToolkitServiceProvider.AttachedValueProvider.Clear(page, key);
            return dataContext;
        }

        #endregion

        #region Events

        public static event EventHandler<Page, CancelEventArgs> BackButtonPressed;

        public static Func<object, Action> SendBackButtonPressed { get; set; }

        #endregion
    }
}