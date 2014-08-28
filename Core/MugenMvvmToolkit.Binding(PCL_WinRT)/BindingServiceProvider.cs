#region Copyright
// ****************************************************************************
// <copyright file="BindingServiceProvider.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Core;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Utils;

namespace MugenMvvmToolkit.Binding
{
    /// <summary>
    ///     Represents the service locator for data binding infrastructure.
    /// </summary>
    public static class BindingServiceProvider
    {
        #region Fields

        private static IBindingProvider _bindingProvider;
        private static IBindingManager _bindingManager;
        private static IBindingMemberProvider _memberProvider;
        private static IObserverProvider _observerProvider;
        private static IBindingContextManager _contextManager;
        private static IVisualTreeManager _visualTreeManager;
        private static IBindingResourceResolver _resourceResolver;
        private static IWeakEventManager _weakEventManager;
        private static Func<Type, object, object> _valueConverter;

        #endregion

        #region Constructors

        static BindingServiceProvider()
        {
            SetDefaultValues();
            MvvmUtils.InitializeDesignTimeManager();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the delegate that allows to convert binding values.
        /// </summary>
        [NotNull]
        public static Func<Type, object, object> ValueConverter
        {
            get { return _valueConverter; }
            set { _valueConverter = value ?? BindingReflectionExtensions.Convert; }
        }

        /// <summary>
        ///     Gets or sets the <see cref="IBindingProvider" />.
        /// </summary>
        [NotNull]
        public static IBindingProvider BindingProvider
        {
            get { return _bindingProvider; }
            set
            {
                Should.PropertyBeNotNull(value, "BindingProvider");
                _bindingProvider = value;
            }
        }

        /// <summary>
        ///     Gets or sets the <see cref="IBindingManager" />.
        /// </summary>
        [NotNull]
        public static IBindingManager BindingManager
        {
            get { return _bindingManager; }
            set
            {
                Should.PropertyBeNotNull(value, "BindingManager");
                _bindingManager = value;
            }
        }

        /// <summary>
        ///     Gets or sets  the <see cref="IBindingMemberProvider" />.
        /// </summary>
        [NotNull]
        public static IBindingMemberProvider MemberProvider
        {
            get { return _memberProvider; }
            set
            {
                Should.PropertyBeNotNull(value, "MemberProvider");
                _memberProvider = value;
            }
        }

        /// <summary>
        ///     Gets or sets the <see cref="IObserverProvider" />.
        /// </summary>
        [NotNull]
        public static IObserverProvider ObserverProvider
        {
            get { return _observerProvider; }
            set
            {
                Should.PropertyBeNotNull(value, "ObserverProvider");
                _observerProvider = value;
            }
        }

        /// <summary>
        ///     Gets or sets the <see cref="IBindingContextManager" />.
        /// </summary>
        [NotNull]
        public static IBindingContextManager ContextManager
        {
            get { return _contextManager; }
            set
            {
                Should.PropertyBeNotNull(value, "ContextManager");
                _contextManager = value;
            }
        }

        /// <summary>
        ///     Gets or sets the <see cref="IBindingResourceResolver" />.
        /// </summary>
        [NotNull]
        public static IBindingResourceResolver ResourceResolver
        {
            get { return _resourceResolver; }
            set
            {
                Should.PropertyBeNotNull(value, "ResourceResolver");
                _resourceResolver = value;
            }
        }

        /// <summary>
        ///     Gets or sets the default <see cref="IVisualTreeManager" />.
        /// </summary>
        [NotNull]
        public static IVisualTreeManager VisualTreeManager
        {
            get { return _visualTreeManager; }
            set
            {
                Should.PropertyBeNotNull(value, "VisualTreeManager");
                _visualTreeManager = value;
            }
        }

        /// <summary>
        ///     Gets or sets the <see cref="IWeakEventManager" />.
        /// </summary>
        [NotNull]
        public static IWeakEventManager WeakEventManager
        {
            get { return _weakEventManager; }
            set
            {
                Should.PropertyBeNotNull(value, "WeakEventManager");
                _weakEventManager = value;
            }
        }

        /// <summary>
        ///     Gets or sets the <see cref="IBindingErrorProvider" />.
        /// </summary>
        [CanBeNull]
        public static IBindingErrorProvider ErrorProvider { get; set; }

        #endregion

        #region Methods

        internal static void SetDefaultValues()
        {
            _valueConverter = BindingReflectionExtensions.Convert;
            _resourceResolver = new BindingResourceResolver();
            _memberProvider = new BindingMemberProvider();
            _visualTreeManager = new VisualTreeManager();
            _weakEventManager = new WeakEventManager();
            _bindingManager = new BindingManager();
            _bindingProvider = new BindingProvider();
            _observerProvider = new ObserverProvider();
            _contextManager = new BindingContextManager();
        }

        #endregion
    }
}