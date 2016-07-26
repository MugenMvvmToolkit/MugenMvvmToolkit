#region Copyright

// ****************************************************************************
// <copyright file="ViewFactory.cs">
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
using Android.Content;
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
        #region Methods

        protected virtual ViewResult GetViewResult(View view, Context context, IAttributeSet attrs)
        {
            var type = view.GetType();
            var bind = ReadStringAttributeValue(context, attrs, Resource.Styleable.Binding, Resource.Styleable.Binding_Bind, null, null, null);
            var itemTemplateId = ReadAttributeValueId(context, attrs, Resource.Styleable.ItemsControl, Resource.Styleable.ItemsControl_ItemTemplate, view, type,
                AttachedMembers.ViewGroup.ItemTemplate);
            var dropDownItemTemplate = ReadAttributeValueId(context, attrs, Resource.Styleable.ItemsControl, Resource.Styleable.ItemsControl_DropDownItemTemplate, view, type,
                AttachedMembers.AdapterView.DropDownItemTemplate);
            var contentTemplate = ReadAttributeValueId(context, attrs, Resource.Styleable.Control, Resource.Styleable.Control_ContentTemplate, view, type,
                AttachedMembers.ViewGroup.ContentTemplate);
            var menuTemplate = ReadAttributeValueId(context, attrs, Resource.Styleable.Menu, Resource.Styleable.Menu_MenuTemplate, view, type,
                AttachedMembers.Toolbar.MenuTemplate);
            var popupMenuTemplate = ReadAttributeValueId(context, attrs, Resource.Styleable.Menu, Resource.Styleable.Menu_PopupMenuTemplate, view, type,
                AttachedMembers.View.PopupMenuTemplate);
            var popupMenuEvent = ReadStringAttributeValue(context, attrs, Resource.Styleable.Menu, Resource.Styleable.Menu_PopupMenuEvent, view, type,
                AttachedMembers.View.PopupMenuEvent);
            var placementTargetPath = ReadStringAttributeValue(context, attrs, Resource.Styleable.Menu, Resource.Styleable.Menu_PlacementTargetPath, view, type,
                AttachedMembers.View.PopupMenuPlacementTargetPath);
            return new ViewResult(view, bind, itemTemplateId, dropDownItemTemplate, contentTemplate, menuTemplate, popupMenuTemplate, popupMenuEvent, placementTargetPath);
        }

        internal static string ReadStringAttributeValue(Context context, IAttributeSet attrs, int[] groupId, int index, View view, Type viewType, string attachedPropertyName)
        {
            var typedArray = context.Theme.ObtainStyledAttributes(attrs, groupId, 0, 0);
            try
            {
                var st = typedArray.GetString(index);
                if (attachedPropertyName != null && !string.IsNullOrEmpty(st))
                    BindingServiceProvider.MemberProvider.GetBindingMember(viewType, attachedPropertyName, false, false)?.SetSingleValue(view, st);
                return st;
            }
            finally
            {
                typedArray.Recycle();
                typedArray.Dispose();
            }
        }

        private static int? ReadAttributeValueId(Context context, IAttributeSet attrs, int[] groupId, int requiredAttributeId, View view, Type viewType, string attachedPropertyName)
        {
            var typedArray = context.Theme.ObtainStyledAttributes(attrs, groupId, 0, 0);
            try
            {
                var result = typedArray.GetResourceId(requiredAttributeId, int.MinValue);
                if (result == int.MinValue)
                    return null;
                if (attachedPropertyName != null)
                    BindingServiceProvider.MemberProvider.GetBindingMember(viewType, attachedPropertyName, false, false)?.SetSingleValue(view, result);
                return result;
            }
            finally
            {
                typedArray.Recycle();
                typedArray.Dispose();
            }
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