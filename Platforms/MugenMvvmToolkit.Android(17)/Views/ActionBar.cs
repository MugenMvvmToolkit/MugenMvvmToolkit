#region Copyright
// ****************************************************************************
// <copyright file="ActionBarView.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Models;
#if API8SUPPORT
using ActionBarEx = Android.Support.V7.App.ActionBar;
#else
using ActionBarEx = Android.App.ActionBar;
#endif

namespace MugenMvvmToolkit.Views
{
    public sealed class ActionBar : View, IManualBindings
    {
        #region Fields

        private static readonly XmlSerializer Serializer;
        private const string TabContentIdKey = "!@tabcontentId";

        private readonly int _resourceId;
        private readonly int _tabContentId;
        private IList<string> _bindings;

        #endregion

        #region Constructors

        static ActionBar()
        {
            Serializer = new XmlSerializer(typeof(ActionBarTemplate), string.Empty);
        }

        public ActionBar(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            SetMinimumWidth(0);
            SetMinimumHeight(0);
            base.Visibility = ViewStates.Gone;
            base.Id = Resource.Id.ActionBarView;
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

        public void Apply(Activity activity)
        {
            if (_resourceId == int.MinValue)
                return;
            if (activity == null)
            {
                Tracer.Warn("The activity is null {0}", this);
                return;
            }
            var actionBar = activity.GetActionBar();
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

            if (_bindings == null)
                return;
            for (int i = 0; i < _bindings.Count; i++)
                BindingServiceProvider.BindingProvider.CreateBindingsFromString(actionBar, _bindings[i], null);
        }

        public static int? GetTabContentId(ActionBarEx actionBar)
        {
            int value;
            if (ServiceProvider.AttachedValueProvider.TryGetValue(actionBar, TabContentIdKey, out value))
                return value;
            return null;
        }

        #endregion

        #region Implementation of IManualBindings

        public IList<IDataBinding> SetBindings(IList<string> bindings)
        {
            _bindings = bindings;
            return EmptyValue<IDataBinding>.ListInstance;
        }

        #endregion

        #region Overrides of View

        public override int Id
        {
            get { return Resource.Id.ActionBarView; }
            set { }
        }

        public override ViewStates Visibility
        {
            get { return ViewStates.Gone; }
            set { }
        }

        #endregion
    }
}