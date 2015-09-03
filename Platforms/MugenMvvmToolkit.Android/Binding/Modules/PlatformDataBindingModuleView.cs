#region Copyright

// ****************************************************************************
// <copyright file="PlatformDataBindingModuleView.cs">
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
using Android.OS;
using Android.Runtime;
using Android.Views;
using MugenMvvmToolkit.Android.Binding.Models;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;

namespace MugenMvvmToolkit.Android.Binding.Modules
{
    public partial class PlatformDataBindingModule
    {
        #region Nested types

        private abstract class LayoutObserver : Java.Lang.Object, View.IOnLayoutChangeListener, ViewTreeObserver.IOnGlobalLayoutListener
        {
            #region Fields

            private WeakReference _viewReference;

            #endregion

            #region Constructors

            protected LayoutObserver(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
            {
            }

            protected LayoutObserver(View view, bool treeObserver)
            {
                _viewReference = ServiceProvider.WeakReferenceFactory(view);
                if (!treeObserver && Build.VERSION.SdkInt >= BuildVersionCodes.Honeycomb)
                    view.AddOnLayoutChangeListener(this);
                else
                {
                    var viewTreeObserver = view.ViewTreeObserver;
                    if (viewTreeObserver.IsAlive)
                        viewTreeObserver.AddOnGlobalLayoutListener(this);
                }
            }

            #endregion

            #region Methods

            protected View GetView()
            {
                var viewReference = _viewReference;
                if (viewReference == null)
                    return null;
                return (View)viewReference.Target;
            }

            private void Raise()
            {
                if (_viewReference == null)
                    return;
                var view = GetView();
                if (view.IsAlive())
                    OnGlobalLayoutChangedInternal(view);
                else
                    Dispose();
            }

            protected abstract void OnGlobalLayoutChangedInternal(View view);

            #endregion

            #region Implementation of interfaces

            public void OnLayoutChange(View v, int left, int top, int right, int bottom, int oldLeft, int oldTop,
                int oldRight,
                int oldBottom)
            {
                Raise();
            }

            public void OnGlobalLayout()
            {
                Raise();
            }

            #endregion

            #region Overrides

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    try
                    {
                        var view = GetView();
                        if (view.IsAlive())
                        {
                            if (Build.VERSION.SdkInt >= BuildVersionCodes.Honeycomb)
                                view.RemoveOnLayoutChangeListener(this);
                            else
                            {
                                var viewTreeObserver = view.ViewTreeObserver;
                                if (viewTreeObserver.IsAlive)
                                    viewTreeObserver.RemoveOnGlobalLayoutListener(this);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Tracer.Warn(e.Flatten());
                    }
                    finally
                    {
                        _viewReference = null;
                    }
                }
                base.Dispose(disposing);
            }

            #endregion

        }

        private sealed class SizeObserver : LayoutObserver
        {
            #region Fields

            private readonly WeakEventListenerWrapper _listenerRef;

            #endregion

            #region Constructors

            public SizeObserver(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
            {
            }

            public SizeObserver(View view, IEventListener handler)
                : base(view, false)
            {
                _listenerRef = handler.ToWeakWrapper();
            }

            #endregion

            #region Overrides of LayoutObserver

            protected override void OnGlobalLayoutChangedInternal(View view)
            {
                if (!_listenerRef.EventListener.TryHandle(view, EventArgs.Empty))
                    Dispose();
            }

            #endregion
        }

        private sealed class VisiblityObserver : LayoutObserver
        {
            #region Fields

            private readonly WeakEventListenerWrapper _listenerRef;
            private ViewStates _oldValue;

            #endregion

            #region Constructors

            public VisiblityObserver(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
            {
            }

            public VisiblityObserver(View view, IEventListener handler)
                : base(view, true)
            {
                _listenerRef = handler.ToWeakWrapper();
                _oldValue = view.Visibility;
            }

            #endregion

            #region Overrides of LayoutObserver

            protected override void OnGlobalLayoutChangedInternal(View view)
            {
                ViewStates visibility = view.Visibility;
                if (_oldValue == visibility)
                    return;
                _oldValue = visibility;
                if (!_listenerRef.EventListener.TryHandle(view, EventArgs.Empty))
                    Dispose();
            }

            #endregion
        }

        #endregion

        #region Methods

        private static void RegisterViewMembers(IBindingMemberProvider memberProvider)
        {
            //View
            memberProvider.Register(AttachedBindingMember.CreateMember<View, object>(AttachedMemberConstants.FindByNameMethod, ViewFindByNameMember));
            memberProvider.Register(AttachedBindingMember.CreateMember<View, object>(AttachedMemberConstants.ParentExplicit, GetViewParentValue, SetViewParentValue, ObserveViewParent));
            memberProvider.Register(AttachedBindingMember.CreateMember<View, bool>(AttachedMemberConstants.Focused,
                    (info, view) => view.IsFocused, (info, view, arg3) =>
                    {
                        if (arg3)
                            view.RequestFocus();
                        else
                            view.ClearFocus();
                    }, "FocusChange"));
            memberProvider.Register(AttachedBindingMember.CreateMember<View, bool>(AttachedMemberConstants.Enabled,
                    (info, view) => view.Enabled, (info, view, value) => view.Enabled = value));
            memberProvider.Register(AttachedBindingMember.CreateMember<View, ViewStates>("Visibility",
                (info, view) => view.Visibility, (info, view, value) => view.Visibility = value,
                ObserveViewVisibility));
            memberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembers.View.Visible,
                (info, view) => view.Visibility == ViewStates.Visible,
                (info, view, value) => view.Visibility = value ? ViewStates.Visible : ViewStates.Gone,
                ObserveViewVisibility));
            memberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembers.View.Hidden,
                (info, view) => view.Visibility != ViewStates.Visible,
                (info, view, value) => view.Visibility = value ? ViewStates.Gone : ViewStates.Visible,
                ObserveViewVisibility));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty<View, object>(AttachedMembers.Toolbar.MenuTemplate.Path));
            memberProvider.Register(AttachedBindingMember.CreateEvent<View>("WidthChanged", (info, o, arg3) => new SizeObserver(o, arg3)));
            memberProvider.Register(AttachedBindingMember.CreateEvent<View>("HeightChanged", (info, o, arg3) => new SizeObserver(o, arg3)));
        }

        private static IDisposable ObserveViewVisibility(IBindingMemberInfo bindingMemberInfo, View view, IEventListener arg3)
        {
            return new VisiblityObserver(view, arg3);
        }

        private static IDisposable ObserveViewParent(IBindingMemberInfo bindingMemberInfo, View view, IEventListener arg3)
        {
            return ParentObserver.GetOrAdd(view).AddWithUnsubscriber(arg3);
        }

        private static object GetViewParentValue(IBindingMemberInfo arg1, View view)
        {
            return ParentObserver.GetOrAdd(view).Parent;
        }

        private static void SetViewParentValue(IBindingMemberInfo bindingMemberInfo, View view, object arg3)
        {
            ParentObserver.GetOrAdd(view).Parent = arg3;
        }

        private static object ViewFindByNameMember(IBindingMemberInfo bindingMemberInfo, View target, object[] arg3)
        {
            if (target == null)
                return null;
            var name = arg3[0].ToStringSafe();
            var result = target.FindViewWithTag(name);
            if (result == null)
            {
                var id = target.Resources.GetIdentifier(name, "id", target.Context.PackageName);
                if (id != 0)
                    result = target.FindViewById(id);
            }
            return result;
        }

        #endregion
    }
}