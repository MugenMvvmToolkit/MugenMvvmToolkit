#region Copyright

// ****************************************************************************
// <copyright file="LayoutInflaterFactoryWrapper.cs">
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

        private static readonly Field FactoryField;
        private static readonly Field Factory2Field;
        private static readonly Field PrivateFactoryField;
        private static readonly MethodInfo JavaCastMethod;
        private static readonly Dictionary<string, Class> ViewNameToClass;

        private readonly LayoutInflater.IFactory _factory;
        private readonly LayoutInflater.IFactory2 _factory2;
        private readonly LayoutInflater.IFactory2 _privateFactory;
        private readonly IViewFactory _viewFactory;

        #endregion

        #region Constructors

        static LayoutInflaterFactoryWrapper()
        {
            ViewNameToClass = new Dictionary<string, Class>(StringComparer.Ordinal);
            JavaCastMethod = typeof(JavaObjectExtensions).GetMethod("JavaCast", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(IJavaObject) }, null);
            var inflaterClass = Class.FromType(typeof(LayoutInflater));
            FactoryField = TryGetField("mFactory", inflaterClass);
            if (PlatformExtensions.IsApiGreaterThan10)
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
            if (PlatformExtensions.IsApiGreaterThan10)
            {
                if (inflater.Factory is LayoutInflaterFactoryWrapper && inflater.Factory2 is LayoutInflaterFactoryWrapper)
                    return;
            }
            else
            {
                if (inflater.Factory is LayoutInflaterFactoryWrapper)
                    return;
            }
            if (factory == null && !ServiceProvider.TryGet(out factory))
                factory = new ViewFactory();
            var factoryWrapper = new LayoutInflaterFactoryWrapper(inflater, factory);
            FactoryField.Set(inflater, factoryWrapper);
            if (Factory2Field != null)
                Factory2Field.Set(inflater, factoryWrapper);
        }

        private View OnViewCreated(View view, string name, Context context, IAttributeSet attrs, bool isLastFactory)
        {
            if (name == "fragment")
                return view;
            ViewResult viewResult = default(ViewResult);
            if (view == null)
            {
                if (!isLastFactory)
                    return null;
                var type = TypeCache<View>.Instance.GetTypeByName(name, true, false);
                if (type == null)
                    return null;
                viewResult = _viewFactory.Create(type, context, attrs);
            }
            else
            {
                var @class = GetClass(name);
                if (@class != null && @class.IsInstance(view))
                {
                    var type = TypeCache<View>.Instance.GetTypeByName(name, true, false);
                    if (type != null && !type.IsInstanceOfType(view))
                        view = (View)JavaCastMethod.MakeGenericMethod(type).Invoke(null, new object[] { view });
                    viewResult = _viewFactory.Initialize(view, attrs);
                }
            }

            Func<View, string, Context, IAttributeSet, View> viewCreated = PlatformExtensions.ViewCreated;
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

        private static Class GetClass(string name)
        {
            Class value;
            if (!ViewNameToClass.TryGetValue(name, out value))
            {
                var type = TypeCache<View>.Instance.GetTypeByName(name, true, false);
                if (type != null)
                    value = Class.FromType(type);
                ViewNameToClass[name] = value;
            }
            return value;
        }

        private View CreateViewFactory2(View parent, string name, Context context, IAttributeSet attrs)
        {
            Class @class = null;
            return CreateView(_factory2, parent, name, context, attrs, ref @class) ?? CreateView(_privateFactory, parent, name, context, attrs, ref @class);
        }

        private static View CreateView(LayoutInflater.IFactory2 factory2, View parent, string name, Context context, IAttributeSet attrs, ref Class @class)
        {
            if (factory2 == null)
                return null;
            if (name.IndexOf(".", StringComparison.Ordinal) < 0)
            {
                if (@class == null)
                    @class = GetClass(name);
                if (@class != null)
                {
                    var view = factory2.OnCreateView(parent, @class.Name, context, attrs);
                    if (view != null)
                        return view;
                }
            }
            return factory2.OnCreateView(parent, name, context, attrs);
        }

        #endregion

        #region Implementation of interfaces

        public View OnCreateView(string name, Context context, IAttributeSet attrs)
        {
            View view = null;
            var factory = _factory;
            if (factory != null)
            {
                view = factory.OnCreateView(name, context, attrs);
                if (view == null && name.IndexOf(".", StringComparison.Ordinal) < 0)
                {
                    var @class = GetClass(name);
                    if (@class != null)
                        view = factory.OnCreateView(@class.Name, context, attrs);
                }
            }
            return OnViewCreated(view, name, context, attrs, PlatformExtensions.IsApiLessThanOrEqualTo10);
        }

        public View OnCreateView(View parent, string name, Context context, IAttributeSet attrs)
        {
            return OnViewCreated(CreateViewFactory2(parent, name, context, attrs), name, context, attrs, true);
        }

        #endregion
    }
}