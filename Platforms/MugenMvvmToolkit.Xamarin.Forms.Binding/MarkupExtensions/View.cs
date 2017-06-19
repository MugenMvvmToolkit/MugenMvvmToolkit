#region Copyright

// ****************************************************************************
// <copyright file="View.cs">
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Xamarin.Forms.Binding;
using Xamarin.Forms;

// ReSharper disable once CheckNamespace
namespace MugenMvvmToolkit.Xamarin.Forms.MarkupExtensions
{
    public static class View
    {
        #region Constructors

        static View()
        {
            var nativeViewAccessors = new Dictionary<Type, PropertyInfo>();
            GetNativeViewHandler = element =>
            {
                var type = element?.GetType();
                if (type == null)
                    return null;
                PropertyInfo propertyInfo;
                if (!nativeViewAccessors.TryGetValue(type, out propertyInfo))
                {
                    propertyInfo = element?.GetType()?.GetPropertyEx("NativeView", MemberFlags.Instance | MemberFlags.Public);
                    nativeViewAccessors[type] = propertyInfo;
                }
                return propertyInfo?.GetValue(element);
            };
        }

        #endregion

        #region Methods

        public static string GetBind(BindableObject view)
        {
            return (string)view.GetValue(BindProperty);
        }

        public static void SetBind(BindableObject view, string value)
        {
            view.SetValue(BindProperty, value);
        }

        public static bool GetHasNativeView(BindableObject view)
        {
            return (bool)view.GetValue(HasNativeViewProperty);
        }

        public static void SetHasNativeView(BindableObject view, bool value)
        {
            view.SetValue(HasNativeViewProperty, value);
        }

        private static void OnHasNativeViewChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var element = bindable as Element;
            if (!(newValue is bool) || element == null)
                return;
            if ((bool)newValue)
                element.ChildAdded += OnChildAdded;
            else
                element.ChildAdded -= OnChildAdded;
        }

        private static void OnChildAdded(object sender, ElementEventArgs args)
        {
            var nativeView = GetNativeViewHandler?.Invoke(args.Element);
            nativeView?.SetBindingMemberValue(AttachedMembers.Object.Parent, args.Element);
        }

        private static void OnBindPropertyChanged(object bindable, object oldValue, object newValue)
        {
            var bindings = (string)newValue;
            if (string.IsNullOrWhiteSpace(bindings))
                return;
            var nativeView = GetNativeViewHandler?.Invoke((BindableObject)bindable);
            if (nativeView != null)
            {
                nativeView.SetBindingMemberValue(AttachedMembers.Object.Parent, bindable);
                bindable = nativeView;
            }
            if (XamarinFormsToolkitExtensions.IsDesignMode)
            {
                XamarinFormsDataBindingExtensions.InitializeFromDesignContext();
                var list = BindingServiceProvider.BindingProvider.CreateBindingsFromStringWithBindings(bindable, bindings);
                foreach (var binding in list.OfType<InvalidDataBinding>())
                    throw binding.Exception;
            }
            else
                BindingServiceProvider.BindingProvider.CreateBindingsFromString(bindable, bindings);
        }

        #endregion

        #region Attached properties

        public static Func<BindableObject, object> GetNativeViewHandler { get; set; }

        public static readonly BindableProperty BindProperty = BindableProperty.CreateAttached("Bind", typeof(string),
            typeof(BindableObject), null, propertyChanged: OnBindPropertyChanged);

        public static readonly BindableProperty HasNativeViewProperty = BindableProperty.CreateAttached("HasNativeView", typeof(bool),
            typeof(BindableObject), Empty.FalseObject, propertyChanged: OnHasNativeViewChanged);

        #endregion
    }
}