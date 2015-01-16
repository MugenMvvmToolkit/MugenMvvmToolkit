#region Copyright

// ****************************************************************************
// <copyright file="BindingServiceProvider.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Infrastructure;

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
        private static Func<IBindingMemberInfo, Type, object, object> _valueConverter;
        private static readonly Dictionary<string, int> MemberPriorities;
        private static readonly List<string> FakeMemberPrefixesField;
        private static readonly ICollection<string> DataContextMemberAliasesField;
        private static Func<string, IBindingPath> _bindingPathFactory;

        #endregion

        #region Constructors

        static BindingServiceProvider()
        {
            MemberPriorities = new Dictionary<string, int>
            {
                {AttachedMemberConstants.DataContext, int.MaxValue - 1},
                {AttachedMemberConstants.ItemTemplate, 1},
                {AttachedMemberConstants.ItemTemplateSelector, 1},
                {AttachedMemberConstants.ContentTemplate, 1},
                {AttachedMemberConstants.ContentTemplateSelector, 1},
            };
            FakeMemberPrefixesField = new List<string>
            {
                "_Fake",
                "Fake"
            };
            DataContextMemberAliasesField = new HashSet<string>(StringComparer.Ordinal)
            {
                AttachedMemberConstants.DataContext
            };
            SetDefaultValues();
            ServiceProvider.InitializeDesignTimeManager();
            ViewManager.GetDataContext = o => ContextManager.GetBindingContext(o).Value;
            ViewManager.SetDataContext = (o, o1) => ContextManager.GetBindingContext(o).Value = o1;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the list that contains the prefixes of fake members.
        /// </summary>
        public static List<string> FakeMemberPrefixes
        {
            get { return FakeMemberPrefixesField; }
        }

        /// <summary>
        ///     Gets the dictionary that contains the priority of binding members.
        /// </summary>
        [NotNull]
        public static IDictionary<string, int> BindingMemberPriorities
        {
            get { return MemberPriorities; }
        }

        /// <summary>
        ///     Gets the collection of possible data context member aliases.
        /// </summary>
        [NotNull]
        public static ICollection<string> DataContextMemberAliases
        {
            get { return DataContextMemberAliasesField; }
        }

        /// <summary>
        ///     Gets or sets the delegate that allows to convert binding values.
        /// </summary>
        [NotNull]
        public static Func<IBindingMemberInfo, Type, object, object> ValueConverter
        {
            get { return _valueConverter; }
            set { _valueConverter = value ?? ((member, type, o) => o); }
        }

        /// <summary>
        ///     Gets or sets the factory that creates an instance of <see cref="IBindingPath" /> for the specified string.
        /// </summary>
        [NotNull]
        public static Func<string, IBindingPath> BindingPathFactory
        {
            get { return _bindingPathFactory; }
            set
            {
                Should.PropertyBeNotNull(value);
                _bindingPathFactory = value;
            }
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
                Should.PropertyBeNotNull(value);
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
                Should.PropertyBeNotNull(value);
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
                Should.PropertyBeNotNull(value);
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
                Should.PropertyBeNotNull(value);
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
                Should.PropertyBeNotNull(value);
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
                Should.PropertyBeNotNull(value);
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
                Should.PropertyBeNotNull(value);
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
                Should.PropertyBeNotNull(value);
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
            _bindingPathFactory = BindingPath.Create;
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