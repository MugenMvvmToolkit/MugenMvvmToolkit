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
using Android.App;
using Android.Views;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Modules
{
    public partial class PlatformDataBindingModule
    {
        #region Nested types

        private abstract class LayoutObserver : DisposableObject
        {
            #region Fields

            private WeakReference _viewReference;

            #endregion

            #region Constructors

            protected LayoutObserver(View view)
            {
                _viewReference = ServiceProvider.WeakReferenceFactory(view, true);
                if (view.ViewTreeObserver.IsAlive)
                    view.ViewTreeObserver.GlobalLayout += OnGlobalLayoutChanged;
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

            private void OnGlobalLayoutChanged(object sender, EventArgs eventArgs)
            {
                if (IsDisposed)
                    return;
                var view = GetView();
                if (view == null)
                    Dispose();
                else
                    OnGlobalLayoutChangedInternal(view, sender, eventArgs);
            }

            protected abstract void OnGlobalLayoutChangedInternal(View view, object sender, EventArgs eventArgs);

            #endregion

            #region Overrides of DisposableObjectBase

            protected override bool TraceFinalized
            {
                get { return false; }
            }

            protected override void OnDispose(bool disposing)
            {
                base.OnDispose(disposing);
                if (disposing)
                {
                    try
                    {
                        var view = (View)_viewReference.Target;
                        if (view != null && view.ViewTreeObserver.IsAlive)
                            view.ViewTreeObserver.GlobalLayout -= OnGlobalLayoutChanged;
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

            public VisiblityObserver(View view, IEventListener handler)
                : base(view)
            {
                _listenerRef = handler.ToWeakWrapper();
                _oldValue = view.Visibility;
            }

            #endregion

            #region Overrides of LayoutObserver

            protected override void OnGlobalLayoutChangedInternal(View view, object sender, EventArgs eventArgs)
            {
                ViewStates visibility = view.Visibility;
                if (_oldValue == visibility)
                    return;
                _oldValue = visibility;
                if (!_listenerRef.EventListener.TryHandle(view, eventArgs))
                    Dispose();
            }

            #endregion
        }

        #endregion

        #region Methods

        private static void RegisterViewMembers(IBindingMemberProvider memberProvider)
        {
            //Activity
            memberProvider.Register(AttachedBindingMember.CreateMember<Activity, object>(AttachedMemberConstants.FindByNameMethod, ActivityFindByNameMember));

            //View            
            memberProvider.Register(AttachedBindingMember.CreateMember<View, object>(AttachedMemberConstants.FindByNameMethod, ViewFindByNameMember));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty<View, int>(AttachedMemberNames.MenuTemplate, MenuTemplateChanged));
            memberProvider.Register(AttachedBindingMember.CreateMember<View, object>(AttachedMemberConstants.Parent, GetViewParentValue, null, ObserveViewParent));
            memberProvider.Register(AttachedBindingMember.CreateMember<View, bool>(AttachedMemberConstants.Focused,
                    (info, view) => view.IsFocused, null, "FocusChange"));
            memberProvider.Register(AttachedBindingMember.CreateMember<View, bool>(AttachedMemberConstants.Enabled,
                    (info, view) => view.Enabled, (info, view, value) => view.Enabled = value));
            memberProvider.Register(AttachedBindingMember.CreateMember<View, ViewStates>("Visibility",
                (info, view) => view.Visibility, (info, view, value) => view.Visibility = value,
                ObserveViewVisibility));
            memberProvider.Register(AttachedBindingMember.CreateMember<View, bool>("Visible",
                (info, view) => view.Visibility == ViewStates.Visible,
                (info, view, value) => view.Visibility = value ? ViewStates.Visible : ViewStates.Gone,
                ObserveViewVisibility));
            memberProvider.Register(AttachedBindingMember.CreateMember<View, bool>("Hidden",
                (info, view) => view.Visibility != ViewStates.Visible,
                (info, view, value) => view.Visibility = value ? ViewStates.Gone : ViewStates.Visible,
                ObserveViewVisibility));
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
            if (view.Id == Android.Resource.Id.Content)
                return view.Context.GetActivity();
            return ParentObserver.GetOrAdd(view).Parent;
        }

        private static object ActivityFindByNameMember(IBindingMemberInfo bindingMemberInfo, Activity target, object[] arg3)
        {
            return ViewFindByNameMember(bindingMemberInfo, target.FindViewById(Android.Resource.Id.Content), arg3);
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