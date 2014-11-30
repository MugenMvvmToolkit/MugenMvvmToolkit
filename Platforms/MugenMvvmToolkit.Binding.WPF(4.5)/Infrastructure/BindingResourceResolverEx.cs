#region Copyright
// ****************************************************************************
// <copyright file="BindingResourceResolverEx.cs">
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

using System;
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
#if WINDOWSCOMMON || NETFX_CORE
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#elif XAMARIN_FORMS
using Xamarin.Forms;
#else
using System.Windows;
using System.Windows.Data;
#endif
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Converters;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    /// <summary>
    ///     Represents the dynamic resource resolver supports {StaticResources}.
    /// </summary>
    public class BindingResourceResolverEx : BindingResourceResolver
    {
        #region Nested types

        private sealed class XamlUnresolvedResource : IBindingResourceObject, IEventListener
        {
            #region Fields

            private object _value;
            private readonly string _key;
            private readonly IDisposable _unsubscriber;
            private readonly WeakReference _reference;

            #endregion

            #region Constructors

            public XamlUnresolvedResource(object target, string key, IBindingMemberInfo rootMember)
            {
                _key = key;
                _value = BindingConstants.UnsetValue;
                _reference = ServiceProvider.WeakReferenceFactory(target, true);
                _unsubscriber = rootMember.TryObserve(target, this);
            }

            #endregion

            #region Implementation of interfaces

            public Type Type
            {
                get
                {
                    var value = _value;
                    if (ReferenceEquals(value, BindingConstants.UnsetValue))
                        return typeof(object);
                    return value.GetType();
                }
            }

            public object Value
            {
                get
                {
                    if (ReferenceEquals(_value, BindingConstants.UnsetValue))
                        Tracer.Warn("The XAML resource with key '{0}' cannot be found.", _key);
                    return _value;
                }
                private set
                {
                    if (Equals(_value, value))
                        return;
                    _value = value;
                    var handler = ValueChanged;
                    if (handler != null)
                        handler(this, EventArgs.Empty);
                }
            }

            public bool IsAlive
            {
                get { return _reference.Target != null; }
            }

            public bool IsWeak
            {
                get { return true; }
            }

            public bool TryHandle(object sender, object message)
            {
                var target = _reference.Target;
                if (target == null)
                {
                    if (_unsubscriber != null)
                        _unsubscriber.Dispose();
                    return false;
                }
                Value = TryFindResource(target, _key) ?? BindingConstants.UnsetValue;
                return true;
            }

            public event EventHandler<ISourceValue, EventArgs> ValueChanged;

            #endregion
        }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingResourceResolverEx" /> class.
        /// </summary>
        public BindingResourceResolverEx()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingResourceResolverEx" /> class.
        /// </summary>
        public BindingResourceResolverEx([NotNull] BindingResourceResolver resolver)
            : base(resolver)
        {
        }

        #endregion

        #region Overrides of BindingResourceResolver

        /// <summary>
        ///     Gets an instance of <see cref="IBindingValueConverter" /> by the specified name.
        /// </summary>
        /// <param name="name">The specified name.</param>
        /// <param name="context">The specified data context, if any.</param>
        /// <param name="throwOnError">
        ///     true to throw an exception if the type cannot be found; false to return null. Specifying
        ///     false also suppresses some other exception conditions, but not all of them.
        /// </param>
        /// <returns>An instance of <see cref="IBindingValueConverter" />.</returns>
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
                    return new ValueConverterWrapper(converter.Convert, converter.ConvertBack);
            }
            return base.ResolveConverter(name, context, throwOnError);
        }

        /// <summary>
        ///     Gets an instance of <see cref="IBindingResourceObject" /> by the specified name.
        /// </summary>
        /// <param name="name">The specified name.</param>
        /// <param name="context">The specified data context, if any.</param>
        /// <param name="throwOnError">
        ///     true to throw an exception if the type cannot be found; false to return null. Specifying
        ///     false also suppresses some other exception conditions, but not all of them.
        /// </param>
        /// <returns>An instance of <see cref="IBindingResourceMethod" />.</returns>
        public override IBindingResourceObject ResolveObject(string name, IDataContext context, bool throwOnError)
        {
            var result = base.ResolveObject(name, context, false);
            if (result != null)
                return result;

            var target = TryGetTarget(context);
            var item = TryFindResource(target, name);
            if (item != null)
                return new BindingResourceObject(item);

            if (throwOnError)
            {
                if (target != null)
                {
                    var rootMember = BindingServiceProvider.VisualTreeManager.GetRootMember(target.GetType());
                    if (rootMember != null)
                        return new XamlUnresolvedResource(target, name, rootMember);
                }
            }
            return base.ResolveObject(name, context, throwOnError);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Tries to find a resource by key.
        /// </summary>
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
            return null;
#else
            var application = Application.Current;
            var currentElement = target as FrameworkElement;
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