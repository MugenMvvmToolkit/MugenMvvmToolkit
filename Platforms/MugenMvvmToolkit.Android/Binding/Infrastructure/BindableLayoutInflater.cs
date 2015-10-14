#region Copyright

// ****************************************************************************
// <copyright file="BindableLayoutInflater.cs">
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
using System.Reflection;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Java.Interop;
using JetBrains.Annotations;
using MugenMvvmToolkit.Android.Binding.Interfaces;
using MugenMvvmToolkit.Android.DataConstants;
using MugenMvvmToolkit.Android.Infrastructure;
using MugenMvvmToolkit.Android.Interfaces;
using MugenMvvmToolkit.Android.Models;
using MugenMvvmToolkit.Binding;

namespace MugenMvvmToolkit.Android.Binding.Infrastructure
{
    public class BindableLayoutInflater : LayoutInflater, LayoutInflater.IFactory
    {
        #region Fields

        private IViewFactory _viewFactory;
        private readonly HashSet<string> _ignoreViewTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "fragment",
            "android.preference.PreferenceFrameLayout"
        };
        private static readonly MethodInfo JavaCastMethod;
        private static readonly Dictionary<string, string> ViewNameToFullName;

        #endregion

        #region Constructors

        static BindableLayoutInflater()
        {
            JavaCastMethod = typeof(JavaObjectExtensions).GetMethod("JavaCast", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(IJavaObject) }, null);
            ViewNameToFullName = new Dictionary<string, string>(StringComparer.Ordinal);
        }

        public BindableLayoutInflater(IViewFactory factory, Context context)
            : base(context)
        {
            Should.NotBeNull(factory, "factory");
            _viewFactory = factory;
            Initialize();
        }

        public BindableLayoutInflater([NotNull] IViewFactory factory, LayoutInflater original, Context newContext)
            : base(original, newContext)
        {
            Should.NotBeNull(factory, "factory");
            Should.NotBeNull(original, "original");
            _viewFactory = factory;
            var bindableLayoutInflater = original as BindableLayoutInflater;
            if (bindableLayoutInflater == null)
            {
                NestedFactory = original.Factory;
                if (PlatformExtensions.IsApiGreaterThan10)
                    NestedFactory2 = original.Factory2;
            }
            else
            {
                NestedFactory = bindableLayoutInflater.NestedFactory;
                if (PlatformExtensions.IsApiGreaterThan10)
                    NestedFactory2 = bindableLayoutInflater.NestedFactory2;
            }
            Initialize();
        }

        protected BindableLayoutInflater(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        #endregion

        #region Properties

        [CanBeNull]
        public virtual IFactory NestedFactory { get; set; }

        [CanBeNull]
        public virtual IFactory2 NestedFactory2 { get; set; }

        [NotNull]
        public virtual IViewFactory ViewFactory
        {
            get { return _viewFactory; }
            set
            {
                Should.PropertyNotBeNull(value);
                _viewFactory = value;
            }
        }

        public HashSet<string> IgnoreViewTypes
        {
            get { return _ignoreViewTypes; }
        }

        #endregion

        #region Overrides of LayoutInflater

        public override LayoutInflater CloneInContext(Context newContext)
        {
            return new BindableLayoutInflater(ViewFactory, this, newContext);
        }

        #endregion

        #region Methods

        View IFactory.OnCreateView(string name, Context context, IAttributeSet attrs)
        {
            return OnCreateViewInternal(name, context, attrs);
        }

        private string GetFullName(string name)
        {
            if (_ignoreViewTypes.Contains(name) || name.IndexOf('.') >= 0)
                return null;
            string value;
            if (!ViewNameToFullName.TryGetValue(name, out value))
            {
                var type = TypeCache<View>.Instance.GetTypeByName(name, true, false);
                if (type != null)
                {
                    var clazz = Java.Lang.Class.FromType(type);
                    value = clazz.CanonicalName;
                }
                ViewNameToFullName[name] = value;
            }
            return value;
        }

        protected virtual void Initialize()
        {
            try
            {
                Factory = this;
            }
            catch (Exception e)
            {
                Tracer.Error(e.Flatten(false));
            }
        }

        protected virtual View OnCreateViewInternal(string name, Context context, IAttributeSet attrs)
        {
            View view = null;
            var fullName = GetFullName(name);
            if (PlatformExtensions.IsApiGreaterThan10 && NestedFactory2 != null)
            {
                if (fullName != null)
                    view = NestedFactory2.OnCreateView(fullName, context, attrs);
                if (view == null)
                    view = NestedFactory2.OnCreateView(name, context, attrs);
            }
            else if (NestedFactory != null)
            {
                if (fullName != null)
                    view = NestedFactory.OnCreateView(fullName, context, attrs);
                if (view == null)
                    view = NestedFactory.OnCreateView(name, context, attrs);
            }
            if (_ignoreViewTypes.Contains(name))
                return view;

            ViewResult viewResult;
            if (view == null)
            {
                viewResult = _viewFactory.Create(name, Context, attrs);
                view = viewResult.View;
            }
            else
            {
                var type = TypeCache<View>.Instance.GetTypeByName(name, true, false);
                if (type != null)
                {
                    var clazz = Java.Lang.Class.FromType(type);
                    if (clazz.IsInstance(view) && !type.IsInstanceOfType(view))
                        view = (View)JavaCastMethod.MakeGenericMethod(type).InvokeEx(null, view);
                }
                viewResult = _viewFactory.Initialize(view, attrs);
            }
            IList<string> bindings = viewResult.DataContext.GetData(ViewFactoryConstants.Bindings);
            if (bindings != null)
            {
                var manualBindings = view as IManualBindings;
                if (manualBindings == null)
                {
                    foreach (string binding in bindings)
                    {
                        BindingServiceProvider
                            .BindingProvider
                            .CreateBindingsFromString(view, binding);
                    }
                }
                else
                    manualBindings.SetBindings(bindings);
            }
            Func<View, string, Context, IAttributeSet, View> viewCreated = PlatformExtensions.ViewCreated;
            if (viewCreated == null)
                return view;
            return viewCreated(view, name, Context, attrs);
        }

        #endregion
    }
}
