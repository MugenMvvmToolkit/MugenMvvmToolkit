#region Copyright

// ****************************************************************************
// <copyright file="ActionBarView.cs">
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
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Util;
using Android.Views;
using MugenMvvmToolkit.Attributes;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Models;
#if APPCOMPAT
using Android.Support.V7.App;
using MugenMvvmToolkit.AppCompat.Models;
using ActionBar = Android.Support.V7.App.ActionBar;

namespace MugenMvvmToolkit.AppCompat.Views
#else
using MugenMvvmToolkit.ActionBarSupport.Models;

namespace MugenMvvmToolkit.ActionBarSupport.Views
#endif
{
#if APPCOMPAT
    [TypeNameAlias("ActionBarCompat")]
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

        public ActionBarView(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            SetMinimumWidth(0);
            SetMinimumHeight(0);
            base.Visibility = ViewStates.Gone;
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

        #endregion

        #region Implementation of IHasActivityDependency

        public void OnAttached(Activity activity)
        {
            var activityView = activity as IActivityView;
            if (activityView == null)
            {
                Tracer.Warn("The IActivityView is null {0}", this);
                return;
            }
            activityView.Mediator.Destroyed += DestroyedHandler;
            var actionBar = activity.GetActionBar();
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
        }

        private static void ActivityViewOnDestroyed(Activity sender, EventArgs args)
        {
            ((IActivityView)sender).Mediator.Destroyed -= DestroyedHandler;
            ActionBarTemplate.Clear(sender);
        }

        #endregion

        #region Implementation of IManualBindings

        public IList<IDataBinding> SetBindings(IList<string> bindings)
        {
            _bindings = bindings;
            return Empty.Array<IDataBinding>();
        }

        #endregion

        #region Overrides of View

        public override ViewStates Visibility
        {
            get { return ViewStates.Gone; }
            set { }
        }

        #endregion
    }
}