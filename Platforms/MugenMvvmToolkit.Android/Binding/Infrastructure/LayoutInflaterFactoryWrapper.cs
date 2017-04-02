#region Copyright

// ****************************************************************************
// <copyright file="LayoutInflaterFactoryWrapper.cs">
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
using System.Reflection;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Java.Interop;
using Java.Lang;
using Java.Lang.Reflect;
using MugenMvvmToolkit.Android.Binding.Interfaces;
using MugenMvvmToolkit.Android.Infrastructure;
using MugenMvvmToolkit.Android.Interfaces;
using MugenMvvmToolkit.Android.Models;
using MugenMvvmToolkit.Binding;
using Exception = System.Exception;
using Object = Java.Lang.Object;

namespace MugenMvvmToolkit.Android.Binding.Infrastructure
{
    public sealed class LayoutInflaterFactoryWrapper : Object, LayoutInflater.IFactory2
    {
        #region Fields

        private readonly LayoutInflater.IFactory _factory;
        private readonly LayoutInflater.IFactory2 _factory2;
        private readonly LayoutInflater.IFactory2 _privateFactory;
        private readonly IViewFactory _viewFactory;

        private static WeakReference _lastCreatedView;
        private static readonly Field FactoryField;
        private static readonly Field Factory2Field;
        private static readonly Field PrivateFactoryField;
        private static readonly MethodInfo JavaCastMethod;
        private static readonly Dictionary<string, string> ViewNameToClass;

        #endregion

        #region Constructors

        static LayoutInflaterFactoryWrapper()
        {
            _lastCreatedView = Empty.WeakReference;
            ViewNameToClass = new Dictionary<string, string>(StringComparer.Ordinal);
            JavaCastMethod = typeof(JavaObjectExtensions).GetMethod("JavaCast", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(IJavaObject) }, null);
            var inflaterClass = Class.FromType(typeof(LayoutInflater));
            FactoryField = TryGetField("mFactory", inflaterClass);
            if (AndroidToolkitExtensions.IsApiGreaterThan10)
            {
                Factory2Field = TryGetField("mFactory2", inflaterClass);
                PrivateFactoryField = TryGetField("mPrivateFactory", inflaterClass);
            }
        }

        private LayoutInflaterFactoryWrapper(LayoutInflater inflater, IViewFactory viewFactory)
        {
            Should.NotBeNull(inflater, nameof(inflater));
            Should.NotBeNull(viewFactory, nameof(viewFactory));
            _viewFactory = viewFactory;
            _factory = inflater.Factory;
            if (Factory2Field != null)
                _factory2 = inflater.Factory2;
            if (PrivateFactoryField != null)
                _privateFactory = JavaObjectExtensions.JavaCast<LayoutInflater.IFactory2>(PrivateFactoryField.Get(inflater));
        }

        private LayoutInflaterFactoryWrapper(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
        {
            _viewFactory = new ViewFactory();
        }

        #endregion

        #region Methods

        public static void SetFactory(LayoutInflater inflater, IViewFactory factory)
        {
            LayoutInflaterFactoryWrapper factoryWrapper = null;
            if (!HasFactory(inflater))
            {
                factoryWrapper = GetWrapper(inflater, factory);
                if (FactoryField == null)
                    inflater.Factory = factoryWrapper;
                else
                    FactoryField.Set(inflater, factoryWrapper);
            }
            if (AndroidToolkitExtensions.IsApiGreaterThan10 && !HasFactory2(inflater))
            {
                if (factoryWrapper == null)
                    factoryWrapper = GetWrapper(inflater, factory);
                if (Factory2Field == null)
                    inflater.Factory2 = factoryWrapper;
                else
                    Factory2Field.Set(inflater, factoryWrapper);
            }
        }

        private static bool HasFactory(LayoutInflater inflater)
        {
            return inflater.Factory is LayoutInflaterFactoryWrapper;
        }

        private static bool HasFactory2(LayoutInflater inflater)
        {
            return inflater.Factory2 is LayoutInflaterFactoryWrapper;
        }

        private static LayoutInflaterFactoryWrapper GetWrapper(LayoutInflater inflater, IViewFactory factory)
        {
            if (factory == null && !ServiceProvider.TryGet(out factory))
                factory = new ViewFactory();
            return new LayoutInflaterFactoryWrapper(inflater, factory);
        }

        private View OnViewCreated(View view, string name, Context context, IAttributeSet attrs)
        {
            if (name == "fragment" || view != null && _lastCreatedView.Target == view)
                return view;
            ViewResult viewResult;
            if (view == null)
            {
                var type = TypeCache<View>.Instance.GetTypeByName(name, true, false);
                if (type == null)
                    return null;
                viewResult = _viewFactory.Create(type, context, attrs);
                view = viewResult.View;
            }
            else
            {
                var type = TypeCache<View>.Instance.GetTypeByName(name, true, false);
                if (type != null && !type.IsInstanceOfType(view))
                {
                    try
                    {
                        view = (View)JavaCastMethod.MakeGenericMethod(type).Invoke(null, new object[] { view });
                    }
                    catch
                    {
                        ;
                    }
                }
                viewResult = _viewFactory.Initialize(view, attrs);
            }

            var viewCreated = AndroidToolkitExtensions.ViewCreated;
            if (viewCreated != null)
                view = viewCreated(view, name, context, attrs);

            if (!viewResult.IsEmpty)
            {
                view = viewResult.View;
                var bind = viewResult.Bind;
                if (!string.IsNullOrEmpty(bind))
                {
                    var manualBindings = view as IManualBindings;
                    if (manualBindings == null)
                        BindingServiceProvider.BindingProvider.CreateBindingsFromString(view, bind);
                    else
                        manualBindings.SetBindings(bind);
                }
            }

            var viewGroup = view as ViewGroup;
            if (viewGroup != null && !viewGroup.IsDisableHierarchyListener())
                viewGroup.SetOnHierarchyChangeListener(GlobalViewParentListener.Instance);

            _lastCreatedView = ServiceProvider.WeakReferenceFactory(view);
            return view;
        }

        private static Field TryGetField(string fieldName, Class clazz)
        {
            try
            {
                var field = clazz.GetDeclaredField(fieldName);
                field.Accessible = true;
                return field;
            }
            catch (Exception e)
            {
                Tracer.Error(e.Flatten(true));
                return null;
            }
        }

        private static string GetClassName(string name)
        {
            string value;
            if (!ViewNameToClass.TryGetValue(name, out value))
            {
                var type = TypeCache<View>.Instance.GetTypeByName(name, true, false);
                if (type != null)
                    value = Class.FromType(type).Name;
                ViewNameToClass[name] = value;
            }
            return value;
        }

        #endregion

        #region Implementation of interfaces

        public View OnCreateView(string name, Context context, IAttributeSet attrs)
        {
            if (_factory == null)
                return OnViewCreated(null, name, context, attrs);
            var v = _factory.OnCreateView(name, context, attrs);
            if (v == null && name.IndexOf('.') < 0)
            {
                var clazz = GetClassName(name);
                if (clazz != null)
                    v = _factory.OnCreateView(clazz, context, attrs);
            }
            return OnViewCreated(v, name, context, attrs);
        }

        public View OnCreateView(View parent, string name, Context context, IAttributeSet attrs)
        {
            View view = null;
            string clazz = null;
            if (_factory2 != null)
            {
                view = _factory2.OnCreateView(parent, name, context, attrs);
                if (view == null && name.IndexOf('.') < 0)
                {
                    clazz = GetClassName(name);
                    if (clazz != null)
                        view = _factory2.OnCreateView(parent, clazz, context, attrs);
                }
            }
            if (view == null && _privateFactory != null)
            {
                view = _privateFactory.OnCreateView(parent, name, context, attrs);
                if (view == null && clazz != null)
                    view = _privateFactory.OnCreateView(clazz, context, attrs);
            }
            return OnViewCreated(view, name, context, attrs);
        }

        #endregion
    }
}