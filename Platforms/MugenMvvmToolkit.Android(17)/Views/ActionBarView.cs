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

namespace MugenMvvmToolkit.Views
{
    public sealed class ActionBarView : View, IManualBindings
    {
        #region Fields

        private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(ActionBarTemplate), string.Empty);
        private const string TabContentIdKey = "!@tabcontentId";

        private readonly int _resourceId;
        private readonly int _tabContentId;
        private Activity _activity;

        #endregion

        #region Constructors

        public ActionBarView(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            SetMinimumWidth(0);
            SetMinimumHeight(0);
            base.Visibility = ViewStates.Gone;
            _activity = context.GetActivity();
            base.Id = Resource.Id.ActionBarView;
            TypedArray typedArray = Context.ObtainStyledAttributes(attrs, Resource.Styleable.ActionBarView);
            try
            {
                _resourceId = typedArray.GetResourceId(Resource.Styleable.ActionBarView_ActionBarTemplate, int.MinValue);
                _tabContentId = typedArray.GetResourceId(Resource.Styleable.ActionBarView_TabContentId, int.MinValue);
            }
            finally
            {
                typedArray.Recycle();
            }
        }

        #endregion

        #region Methods

        public void Apply()
        {
            if (_resourceId == int.MinValue)
                return;
            if (_activity == null)
            {
                Tracer.Warn("The activity is null {0}", this);
                return;
            }
            var actionBar = _activity.GetActionBar();
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
                    barTemplate.Apply(_activity);
                }
            }
        }

        public static int? GetTabContentId(ActionBar actionBar)
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
            if (bindings == null || _activity == null)
                return EmptyValue<IDataBinding>.ListInstance;
            var actionBar = _activity.GetActionBar();
            var dataBindings = new List<IDataBinding>();
            foreach (string binding in bindings)
                dataBindings.AddRange(BindingServiceProvider.BindingProvider.CreateBindingsFromString(actionBar, binding, null));
            return dataBindings;
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

        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();
            if (_activity == null)
                _activity = Context.GetActivity();
            Apply();
        }

        #endregion
    }
}