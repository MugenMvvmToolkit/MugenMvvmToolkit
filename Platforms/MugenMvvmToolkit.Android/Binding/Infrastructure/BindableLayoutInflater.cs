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
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using JetBrains.Annotations;
using MugenMvvmToolkit.Android.Binding.Interfaces;
using MugenMvvmToolkit.Android.DataConstants;
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

        #endregion

        #region Constructors

        public BindableLayoutInflater(IViewFactory factory, LayoutInflater original)
            : this(factory, original, original.Context)
        {
        }

        public BindableLayoutInflater(IViewFactory factory, Context context)
            : base(context)
        {
            Should.NotBeNull(factory, "factory");
            _viewFactory = factory;
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            Initialize();
        }

        protected BindableLayoutInflater(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            Initialize();
        }

        protected BindableLayoutInflater([NotNull] IViewFactory factory, LayoutInflater original, Context newContext)
            : base(original, newContext)
        {
            Should.NotBeNull(factory, "factory");
            _viewFactory = factory;
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            Initialize();
        }

        #endregion

        #region Properties

        [CanBeNull]
        public virtual IFactory NestedFactory { get; set; }

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
            if (_ignoreViewTypes.Contains(name))
            {
                IFactory factory = NestedFactory;
                if (factory == null)
                    return null;
                return factory.OnCreateView(name, context, attrs);
            }

            ViewResult viewResult = _viewFactory.Create(name, Context, attrs);
            View view = viewResult.View;
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
