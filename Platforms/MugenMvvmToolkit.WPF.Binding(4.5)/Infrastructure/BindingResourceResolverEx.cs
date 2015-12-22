#region Copyright

// ****************************************************************************
// <copyright file="BindingResourceResolverEx.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
#if WPF
using MugenMvvmToolkit.WPF.Binding.Converters;
using System.Windows;
using System.Windows.Data;

namespace MugenMvvmToolkit.WPF.Binding.Infrastructure
#elif XAMARIN_FORMS
using Xamarin.Forms;
using MugenMvvmToolkit.Xamarin.Forms.Binding.Converters;

namespace MugenMvvmToolkit.Xamarin.Forms.Binding.Infrastructure
#elif SILVERLIGHT
using MugenMvvmToolkit.Silverlight.Binding.Converters;
using System.Windows;
using System.Windows.Data;

namespace MugenMvvmToolkit.Silverlight.Binding.Infrastructure
#elif WINDOWSCOMMON
using MugenMvvmToolkit.WinRT.Binding.Converters;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace MugenMvvmToolkit.WinRT.Binding.Infrastructure
#elif WINDOWS_PHONE
using MugenMvvmToolkit.WinPhone.Binding.Converters;
using System.Windows;
using System.Windows.Data;

namespace MugenMvvmToolkit.WinPhone.Binding.Infrastructure
#endif
{
    public class BindingResourceResolverEx : BindingResourceResolver
    {
        #region Nested types

        private sealed class XamlResourceWrapper : ISourceValue, IEventListener
        {
            #region Fields

            private object _value;
            private EventHandler<ISourceValue, EventArgs> _handler;
            private readonly string _key;
            private readonly ISourceValue _globalResource;
            private readonly IDisposable _unsubscriber;
            private readonly WeakReference _reference;

            #endregion

            #region Constructors

            public XamlResourceWrapper(object target, string key, IBindingMemberInfo rootMember, ISourceValue globalResource)
            {
                _key = key;
                _value = BindingConstants.UnsetValue;
                _reference = ServiceProvider.WeakReferenceFactory(target);
                _unsubscriber = rootMember.TryObserve(target, this);
                _globalResource = globalResource;
            }

            #endregion

            #region Implementation of interfaces

            public object Value
            {
                get
                {
                    if (ReferenceEquals(_value, BindingConstants.UnsetValue))
                        return _globalResource.Value;
                    return _value;
                }
                private set
                {
                    if (Equals(_value, value))
                        return;
                    _value = value;
                    var handler = _handler;
                    if (handler != null)
                        handler(this, EventArgs.Empty);
                }
            }

            public bool IsAlive => _reference.Target != null || _globalResource.IsAlive;

            public bool IsWeak => true;

            public bool TryHandle(object sender, object message)
            {
                var target = _reference.Target;
                if (target == null)
                {
                    if (_unsubscriber != null)
                        _unsubscriber.Dispose();
                    _handler = null;
                    return false;
                }
                Value = TryFindResource(target, _key) ?? BindingConstants.UnsetValue;
                return true;
            }

            public event EventHandler<ISourceValue, EventArgs> ValueChanged
            {
                add
                {
                    _handler += value;
                    _globalResource.ValueChanged += value;
                }
                remove
                {
                    _handler -= value;
                    _globalResource.ValueChanged -= value;
                }
            }

            #endregion
        }

        #endregion

        #region Constructors

        public BindingResourceResolverEx()
        {
        }

        public BindingResourceResolverEx([NotNull] BindingResourceResolver resolver)
            : base(resolver)
        {
        }

        #endregion

        #region Overrides of BindingResourceResolver

        public override IBindingValueConverter ResolveConverter(string name, IDataContext context, bool throwOnError)
        {
            var result = base.ResolveConverter(name, context, false);
            if (result != null)
                return result;

            var item = TryFindResource(TryGetTarget(context), name);
            if (item != null)
            {
                var valueConverter = item as IBindingValueConverter;
                if (valueConverter != null)
                    return valueConverter;
                var converter = item as IValueConverter;
                if (converter != null)
                    return new ValueConverterWrapper(converter);
            }
            return base.ResolveConverter(name, context, throwOnError);
        }


        protected override ISourceValue ResolveObjectInternal(object target, string name, IDataContext context, out bool keepValue)
        {
            var value = base.ResolveObjectInternal(target, name, context, out keepValue);
            if (value != null)
                return value;

            var globalRes = GetOrAddDynamicResource(name, false);
            if (globalRes.Value != null)
            {
                keepValue = false;
                return globalRes;
            }

            keepValue = true;
            var item = TryFindResource(target, name);
            if (item != null)
                return new BindingResourceObject(item, true);

            var rootMember = BindingServiceProvider.VisualTreeManager.GetRootMember(target.GetType());
            if (rootMember != null)
                return new XamlResourceWrapper(target, name, rootMember, globalRes);
            return null;
        }

        #endregion

        #region Methods

#if !XAMARIN_FORMS
        private static FrameworkElement FindFirstFrameworkElement(object target)
        {
            var currentElement = target as FrameworkElement;
            while (target != null && currentElement == null)
            {
                target = BindingServiceProvider.VisualTreeManager.FindParent(target);
                currentElement = target as FrameworkElement;
            }
            return currentElement;
        }
#endif
        protected static object TryFindResource([CanBeNull] object target, [NotNull] string resourceKey)
        {
#if XAMARIN_FORMS
            var currentElement = target as VisualElement;
            if (currentElement == null)
                return null;
            while (currentElement != null)
            {
                if (currentElement.Resources != null && currentElement.Resources.ContainsKey(resourceKey))
                    return currentElement.Resources[resourceKey];
                currentElement = currentElement.Parent as VisualElement;
            }
            var application = Application.Current;
            if (application != null && application.Resources != null && application.Resources.ContainsKey(resourceKey))
                return application.Resources[resourceKey];
            return null;
#else
            var application = Application.Current;
            var currentElement = FindFirstFrameworkElement(target);
#if WPF
            if (currentElement == null)
            {
                if (application == null)
                    return null;
                return application.TryFindResource(resourceKey);
            }
            return currentElement.TryFindResource(resourceKey);
#else
            while (currentElement != null)
            {
                if (currentElement.Resources != null && currentElement.Resources.Contains(resourceKey))
                    return currentElement.Resources[resourceKey];
                currentElement = currentElement.Parent as FrameworkElement;
            }
            if (application != null && application.Resources.Contains(resourceKey))
                return application.Resources[resourceKey];
            return null;
#endif
#endif
        }

        #endregion
    }
}
