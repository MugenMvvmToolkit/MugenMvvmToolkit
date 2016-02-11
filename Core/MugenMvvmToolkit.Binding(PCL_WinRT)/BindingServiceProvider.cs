#region Copyright

// ****************************************************************************
// <copyright file="BindingServiceProvider.cs">
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
using System.Globalization;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Behaviors;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Infrastructure;

namespace MugenMvvmToolkit.Binding
{
    public static class BindingServiceProvider
    {
        #region Fields

        public const int DataContextMemberPriority = int.MaxValue - 10;
        public const int TemplateMemberPriority = 3;

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
        private static readonly HashSet<string> DataContextMemberAliasesField;
        private static readonly Dictionary<string, IBindingBehavior> BindingModeToBehaviorField;
        private static readonly Dictionary<string, IBindingPath> BindingPathCache;
        private static Func<string, IBindingPath> _bindingPathFactory;
        private static Func<Type, string, IBindingMemberInfo> _updateEventFinder;
        private static Func<CultureInfo> _bindingCultureInfo;

        #endregion

        #region Constructors

        static BindingServiceProvider()
        {
            BindingModeToBehaviorField = new Dictionary<string, IBindingBehavior>(StringComparer.OrdinalIgnoreCase)
            {
                {"Default", null},
                {"TwoWay", new TwoWayBindingMode()},
                {"OneWay", new OneWayBindingMode()},
                {"OneTime", new OneTimeBindingMode()},
                {"OneWayToSource", new OneWayToSourceBindingMode()},
                {"None", NoneBindingMode.Instance}
            };
            MemberPriorities = new Dictionary<string, int>
            {
                {AttachedMemberConstants.DataContext, DataContextMemberPriority},
                {AttachedMemberConstants.ItemTemplate, TemplateMemberPriority},
                {AttachedMemberConstants.ItemTemplateSelector, TemplateMemberPriority},
                {AttachedMemberConstants.ContentTemplate, TemplateMemberPriority},
                {AttachedMemberConstants.ContentTemplateSelector, TemplateMemberPriority}
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
            BindingPathCache = new Dictionary<string, IBindingPath>(StringComparer.Ordinal);
            SetDefaultValues();
            ViewManager.GetDataContext = BindingExtensions.DataContext;
            ViewManager.SetDataContext = BindingExtensions.SetDataContext;
            BindingExceptionHandler = BindingExceptionHandlerImpl;
        }

        #endregion

        #region Properties

        public static bool DisableConverterAutoRegistration { get; set; }

        public static bool DisableDataTemplateSelectorAutoRegistration { get; set; }

        public static bool HasStablePathDefault { get; set; }

        public static bool ObservablePathDefault { get; set; }

        public static Dictionary<string, IBindingBehavior> BindingModeToBehavior => BindingModeToBehaviorField;

        public static List<string> FakeMemberPrefixes => FakeMemberPrefixesField;

        [NotNull]
        public static Dictionary<string, int> BindingMemberPriorities => MemberPriorities;

        [NotNull]
        public static HashSet<string> DataContextMemberAliases => DataContextMemberAliasesField;

        [NotNull]
        public static Func<IBindingMemberInfo, Type, object, object> ValueConverter
        {
            get { return _valueConverter; }
            set { _valueConverter = value ?? ((member, type, o) => o); }
        }

        [NotNull]
        public static Func<Type, string, IBindingMemberInfo> UpdateEventFinder
        {
            get { return _updateEventFinder; }
            set { _updateEventFinder = value ?? FindUpdateEvent; }
        }

        [NotNull]
        public static Func<string, IBindingPath> BindingPathFactory
        {
            get { return _bindingPathFactory; }
            set
            {
                Should.PropertyNotBeNull(value);
                _bindingPathFactory = value;
            }
        }

        [NotNull]
        public static IBindingProvider BindingProvider
        {
            get { return _bindingProvider; }
            set
            {
                Should.PropertyNotBeNull(value);
                _bindingProvider = value;
            }
        }

        [NotNull]
        public static IBindingManager BindingManager
        {
            get { return _bindingManager; }
            set
            {
                Should.PropertyNotBeNull(value);
                _bindingManager = value;
            }
        }

        [NotNull]
        public static IBindingMemberProvider MemberProvider
        {
            get { return _memberProvider; }
            set
            {
                Should.PropertyNotBeNull(value);
                _memberProvider = value;
            }
        }

        [NotNull]
        public static IObserverProvider ObserverProvider
        {
            get { return _observerProvider; }
            set
            {
                Should.PropertyNotBeNull(value);
                _observerProvider = value;
            }
        }

        [NotNull]
        public static IBindingContextManager ContextManager
        {
            get { return _contextManager; }
            set
            {
                Should.PropertyNotBeNull(value);
                _contextManager = value;
            }
        }

        [NotNull]
        public static IBindingResourceResolver ResourceResolver
        {
            get { return _resourceResolver; }
            set
            {
                Should.PropertyNotBeNull(value);
                _resourceResolver = value;
            }
        }

        [NotNull]
        public static IVisualTreeManager VisualTreeManager
        {
            get { return _visualTreeManager; }
            set
            {
                Should.PropertyNotBeNull(value);
                _visualTreeManager = value;
            }
        }

        [NotNull]
        public static IWeakEventManager WeakEventManager
        {
            get { return _weakEventManager; }
            set
            {
                Should.PropertyNotBeNull(value);
                _weakEventManager = value;
            }
        }

        [CanBeNull]
        public static IBindingErrorProvider ErrorProvider { get; set; }

        public static Func<CultureInfo> BindingCultureInfo
        {
            get { return _bindingCultureInfo; }
            set { _bindingCultureInfo = value ?? (() => CultureInfo.CurrentCulture); }
        }

        [CanBeNull]
        public static Action<IDataBinding, BindingEventArgs> BindingExceptionHandler { get; set; }

        #endregion

        #region Methods

        public static void RaiseBindingException(IDataBinding binding, BindingEventArgs args)
        {
            BindingExceptionHandler?.Invoke(binding, args);
        }

        internal static void SetDefaultValues()
        {
            BindingCultureInfo = null;
            _updateEventFinder = FindUpdateEvent;
            _bindingPathFactory = BindingPathFactoryImpl;
            _valueConverter = BindingReflectionExtensions.Convert;
            _resourceResolver = new BindingResourceResolver();
            _memberProvider = new BindingMemberProvider();
            _visualTreeManager = new VisualTreeManager();
            _weakEventManager = new WeakEventManager();
            _bindingManager = new BindingManager();
            _bindingProvider = new BindingProvider();
            _observerProvider = new ObserverProvider();
            _contextManager = new BindingContextManager();
            ObservablePathDefault = true;
        }

        private static IBindingPath BindingPathFactoryImpl(string path)
        {
            lock (BindingPathCache)
            {
                IBindingPath value;
                if (!BindingPathCache.TryGetValue(path, out value))
                {
                    value = new BindingPath(path);
                    BindingPathCache[path] = value;
                }
                return value;
            }
        }

        private static IBindingMemberInfo FindUpdateEvent(Type type, string memberName)
        {
            IBindingMemberInfo member = MemberProvider.GetBindingMember(type, memberName + AttachedMemberConstants.ChangedEventPostfix, false, false);
            if (member == null || member.MemberType != BindingMemberType.Event)
                member = MemberProvider.GetBindingMember(type, memberName + "Change", false, false);
            if (member == null || member.MemberType != BindingMemberType.Event)
                return null;
            return member;
        }

        private static void BindingExceptionHandlerImpl(IDataBinding dataBinding, BindingEventArgs bindingEventArgs)
        {
            if (bindingEventArgs.Exception != null)
                Tracer.Error(bindingEventArgs.Exception.Message);
        }

        #endregion
    }
}
