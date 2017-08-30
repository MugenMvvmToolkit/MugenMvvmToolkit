#region Copyright

// ****************************************************************************
// <copyright file="ViewFactory.cs">
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
using Android.Content;
using Android.Content.Res;
using Android.Runtime;
using Android.Util;
using Android.Views;
using MugenMvvmToolkit.Android.Binding;
using MugenMvvmToolkit.Android.Interfaces;
using MugenMvvmToolkit.Android.Models;
using MugenMvvmToolkit.Binding;

namespace MugenMvvmToolkit.Android.Infrastructure
{
    public class ViewFactory : IViewFactory
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public ViewFactory()
        {
        }

        #endregion

        #region Methods

        protected virtual ViewResult GetViewResult(View view, Context context, IAttributeSet attrs)
        {
            var type = view.GetType();
            var attributes = context.ObtainStyledAttributes(attrs, Resource.Styleable.Binding);
            try
            {
                if (attributes.IndexCount == 0)
                    return new ViewResult(view, null, null, null, null, null, null, null, null);
                var bind = ReadStringAttributeValue(attributes, Resource.Styleable.Binding_Bind, null, null, null);
                var itemTemplateId = ReadAttributeValueId(attributes, Resource.Styleable.Binding_ItemTemplate, view, type, AttachedMembers.ViewGroup.ItemTemplate);
                var dropDownItemTemplate = ReadAttributeValueId(attributes, Resource.Styleable.Binding_DropDownItemTemplate, view, type, AttachedMembers.AdapterView.DropDownItemTemplate);
                var contentTemplate = ReadAttributeValueId(attributes, Resource.Styleable.Binding_ContentTemplate, view, type, AttachedMembers.ViewGroup.ContentTemplate);
                var menuTemplate = ReadAttributeValueId(attributes, Resource.Styleable.Binding_MenuTemplate, view, type, AttachedMembers.View.MenuTemplate);
                var popupMenuTemplate = ReadAttributeValueId(attributes, Resource.Styleable.Binding_PopupMenuTemplate, view, type, AttachedMembers.View.PopupMenuTemplate);
                var popupMenuEvent = ReadStringAttributeValue(attributes, Resource.Styleable.Binding_PopupMenuEvent, view, type, AttachedMembers.View.PopupMenuEvent);
                var placementTargetPath = ReadStringAttributeValue(attributes, Resource.Styleable.Binding_PlacementTargetPath, view, type, AttachedMembers.View.PopupMenuPlacementTargetPath);
                return new ViewResult(view, bind, itemTemplateId, dropDownItemTemplate, contentTemplate, menuTemplate, popupMenuTemplate, popupMenuEvent, placementTargetPath);

            }
            finally
            {
                attributes.Recycle();
                attributes.Dispose();
            }
        }

        internal static string ReadStringAttributeValue(Context context, IAttributeSet attrs, int[] groupId, int index, View view, Type viewType, string attachedPropertyName)
        {
            var typedArray = context.Theme.ObtainStyledAttributes(attrs, groupId, 0, 0);
            try
            {
                return ReadStringAttributeValue(typedArray, index, view, viewType, attachedPropertyName);
            }
            finally
            {
                typedArray.Recycle();
                typedArray.Dispose();
            }
        }

        private static string ReadStringAttributeValue(TypedArray typedArray, int index, View view, Type viewType, string attachedPropertyName)
        {
            var st = typedArray.GetString(index);
            if (attachedPropertyName != null && !string.IsNullOrEmpty(st))
                BindingServiceProvider.MemberProvider.GetBindingMember(viewType, attachedPropertyName, false, false)?.SetSingleValue(view, st);
            return st;
        }

        private static int? ReadAttributeValueId(TypedArray typedArray, int requiredAttributeId, View view, Type viewType, string attachedPropertyName)
        {
            var result = typedArray.GetResourceId(requiredAttributeId, int.MinValue);
            if (result == int.MinValue)
                return null;
            if (attachedPropertyName != null)
                BindingServiceProvider.MemberProvider.GetBindingMember(viewType, attachedPropertyName, false, false)?.SetSingleValue(view, result);
            return result;
        }

        #endregion

        #region Implementation of IViewFactory

        public virtual ViewResult Create(string name, Context context, IAttributeSet attrs)
        {
            Should.NotBeNull(name, nameof(name));
            var type = TypeCache<View>.Instance.GetTypeByName(name, true, true);
            return Create(type, context, attrs);
        }

        public virtual ViewResult Create(Type type, Context context, IAttributeSet attrs)
        {
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(context, nameof(context));
            Should.NotBeNull(attrs, nameof(attrs));
            var view = type.CreateView(context, attrs);
            return Initialize(view, attrs);
        }

        public virtual ViewResult Initialize(View view, IAttributeSet attrs)
        {
            Should.NotBeNull(view, nameof(view));
            Should.NotBeNull(attrs, nameof(attrs));
            return GetViewResult(view, view.Context, attrs);
        }

        #endregion
    }
}