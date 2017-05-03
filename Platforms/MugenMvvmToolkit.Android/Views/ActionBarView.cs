#region Copyright

// ****************************************************************************
// <copyright file="ActionBarView.cs">
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
using System.Xml;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Runtime;
using Android.Util;
using Android.Views;
using MugenMvvmToolkit.Android.Attributes;
using MugenMvvmToolkit.Android.Binding.Interfaces;
using MugenMvvmToolkit.Android.Interfaces.Views;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Models;
#if APPCOMPAT
using MugenMvvmToolkit.Android.AppCompat.Models;

namespace MugenMvvmToolkit.Android.AppCompat.Views
#else
using MugenMvvmToolkit.Android.Binding.Models;

namespace MugenMvvmToolkit.Android.Views
#endif
{
#if APPCOMPAT
    [TypeNameAlias("ActionBarCompat")]
    [Register("mugenmvvmtoolkit.android.appcompat.views.ActionBarView")]
#else
    [Register("mugenmvvmtoolkit.android.views.ActionBarView")]
#endif
    [TypeNameAlias("ActionBar")]
    public sealed class ActionBarView : View, IManualBindings, IHasActivityDependency
    {
        #region Fields

        private static readonly EventHandler<Activity, EventArgs> DestroyedHandler;
        private static readonly Dictionary<int, object> TemplateCache;

        private readonly int _resourceId;
        private readonly int _tabContentId;
        private string _bind;

        #endregion

        #region Constructors

        static ActionBarView()
        {
            DestroyedHandler = ActivityViewOnDestroyed;
            TemplateCache = new Dictionary<int, object>();
        }

        private ActionBarView(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public ActionBarView(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            SetMinimumWidth(0);
            SetMinimumHeight(0);
            Visibility = ViewStates.Gone;
            TypedArray typedArray = Context.ObtainStyledAttributes(attrs, Resource.Styleable.Binding);
            try
            {
                _resourceId = typedArray.GetResourceId(Resource.Styleable.Binding_ActionBarTemplate, int.MinValue);
                _tabContentId = typedArray.GetResourceId(Resource.Styleable.Binding_TabContentId, int.MinValue);
            }
            finally
            {
                typedArray.Recycle();
                typedArray.Dispose();
            }
        }

        #endregion

        #region Methods

        private static void ActivityViewOnDestroyed(Activity sender, EventArgs args)
        {
            ((IActivityView)sender).Mediator.Destroyed -= DestroyedHandler;
            ActionBarTemplate.Clear(sender);
        }

        #endregion

        #region Implementation of interfaces

        public void OnAttached(Activity activity)
        {
            var actionBar = activity.GetActionBar();
            if (actionBar == null)
            {
                Tracer.Error("Cannot apply ActionBarView the ActionBar is null, activity {0}", activity.GetType().FullName);
                return;
            }

            var activityView = activity as IActivityView;
            if (activityView != null)
                activityView.Mediator.Destroyed += DestroyedHandler;

            if (_resourceId != int.MinValue)
            {
                if (_tabContentId != int.MinValue)
                    ServiceProvider.AttachedValueProvider.SetValue(actionBar, ActionBarTemplate.TabContentIdKey, _tabContentId);


                object templateObj;
                if (!TemplateCache.TryGetValue(_resourceId, out templateObj))
                {
                    using (XmlReader reader = Context.Resources.GetLayout(_resourceId))
                    {
                        templateObj = reader.Deserialize<ActionBarTemplate>();
                        TemplateCache[_resourceId] = templateObj;
                    }
                }
                ((ActionBarTemplate)templateObj).Apply(activity);
            }

            if (string.IsNullOrEmpty(_bind))
                return;
            BindingServiceProvider.BindingProvider.CreateBindingsFromString(actionBar, _bind, null);
            this.ClearBindingsRecursively(true, true, AndroidToolkitExtensions.AggressiveViewCleanup);
            this.RemoveFromParent();
        }

        public IList<IDataBinding> SetBindings(string bind)
        {
            _bind = bind;
            return Empty.Array<IDataBinding>();
        }

        #endregion
    }
}
