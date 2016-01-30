#region Copyright

// ****************************************************************************
// <copyright file="ActionBarView.cs">
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
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
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
using ActionBar = Android.Support.V7.App.ActionBar;

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

        private static readonly XmlSerializer Serializer;
        private static readonly EventHandler<Activity, EventArgs> DestroyedHandler;
        private const string TabContentIdKey = "!@tabcontentId";

        private readonly int _resourceId;
        private readonly int _tabContentId;
        private IList<string> _bindings;

        #endregion

        #region Constructors

        static ActionBarView()
        {
            DestroyedHandler = ActivityViewOnDestroyed;
            Serializer = new XmlSerializer(typeof(ActionBarTemplate), string.Empty);
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
            TypedArray typedArray = Context.ObtainStyledAttributes(attrs, Resource.Styleable.ActionBar);
            try
            {
                _resourceId = typedArray.GetResourceId(Resource.Styleable.ActionBar_ActionBarTemplate, int.MinValue);
                _tabContentId = typedArray.GetResourceId(Resource.Styleable.ActionBar_TabContentId, int.MinValue);
            }
            finally
            {
                typedArray.Recycle();
            }
        }

        #endregion

        #region Methods

        public static int? GetTabContentId(ActionBar actionBar)
        {
            int value;
            if (ServiceProvider.AttachedValueProvider.TryGetValue(actionBar, TabContentIdKey, out value))
                return value;
            return null;
        }

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
                    ServiceProvider.AttachedValueProvider.SetValue(actionBar, TabContentIdKey, _tabContentId);
                using (XmlReader reader = Context.Resources.GetLayout(_resourceId))
                {
                    //NOTE XDocument throws an error.
                    var document = new XmlDocument();
                    document.Load(reader);
                    using (var stringReader = new StringReader(PlatformExtensions.XmlTagsToUpper(document.InnerXml)))
                    {
                        var barTemplate = (ActionBarTemplate)Serializer.Deserialize(stringReader);
                        barTemplate.Apply(activity);
                    }
                }
            }

            if (_bindings == null)
                return;
            for (int i = 0; i < _bindings.Count; i++)
                BindingServiceProvider.BindingProvider.CreateBindingsFromString(actionBar, _bindings[i], null);
            this.RemoveFromParent();
            this.ClearBindingsRecursively(true, true);
        }

        public IList<IDataBinding> SetBindings(IList<string> bindings)
        {
            _bindings = bindings;
            return Empty.Array<IDataBinding>();
        }

        #endregion
    }
}
