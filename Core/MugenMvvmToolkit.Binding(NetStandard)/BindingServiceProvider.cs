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
using MugenMvvmToolkit.Models;

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
        private static readonly Dictionary<string, int> MemberPriorities;
        private static readonly List<string> FakeMemberPrefixesField;
        private static readonly HashSet<string> DataContextMemberAliasesField;
        private static readonly Dictionary<string, IBindingBehavior> BindingModeToBehaviorField;
        private static readonly Dictionary<string, IBindingPath> BindingPathCache;
        private static Func<string, IBindingPath> _bindingPathFactory;
        private static Func<CultureInfo> _bindingCultureInfo;
        private static Func<Type, string, IBindingMemberInfo> _updateEventFinder;
        private static Func<IBindingMemberInfo, Type, object, object> _valueConverter;

        #endregion

        #region Constructors

        static BindingServiceProvider()
        {
            BindingModeToBehaviorField = new Dictionary<string, IBindingBehavior>();
            MemberPriorities = new Dictionary<string, int>
            {
                {AttachedMemberConstants.DataContext, DataContextMemberPriority},
                {AttachedMemberConstants.ItemTemplate, TemplateMemberPriority},
                {AttachedMemberConstants.ItemTemplateSelector, TemplateMemberPriority},
                {AttachedMemberConstants.ContentTemplate, TemplateMemberPriority},
                {AttachedMemberConstants.ContentTemplateSelector, TemplateMemberPriority},
                {AttachedMemberConstants.CommandParameter, TemplateMemberPriority}
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
            ViewManager.GetDataContext = BindingExtensions.DataContext;
            ViewManager.SetDataContext = BindingExtensions.SetDataContext;
            BindingExceptionHandler = BindingExceptionHandlerImpl;
            BindingDebugger = BindingDebuggerImpl;
            ObservablePathDefault = true;
            CompiledExpressionInvokerSupportCoalesceExpression = true;
        }

        #endregion

        #region Properties

        public static bool DisableConverterAutoRegistration { get; set; }

        public static bool DisableDataTemplateSelectorAutoRegistration { get; set; }

        public static bool HasStablePathDefault { get; set; }

        public static bool ObservablePathDefault { get; set; }

        public static bool OptionalBindingDefault { get; set; }

        public static bool CompiledExpressionInvokerSupportCoalesceExpression { get; set; }

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
            set
            {
                Should.PropertyNotBeNull(value);
                _valueConverter = value;
            }
        }

        [NotNull]
        public static Func<Type, string, IBindingMemberInfo> UpdateEventFinder
        {
            get { return _updateEventFinder; }
            set
            {
                Should.PropertyNotBeNull(value);
                _updateEventFinder = value;
            }
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

        [NotNull]
        public static Func<CultureInfo> BindingCultureInfo
        {
            get { return _bindingCultureInfo; }
            set
            {
                Should.PropertyNotBeNull(value);
                _bindingCultureInfo = value;
            }
        }

        [CanBeNull]
        public static Action<IDataBinding, BindingEventArgs> BindingExceptionHandler { get; set; }

        [CanBeNull]
        public static Action<object, string, string, object[]> BindingDebugger { get; set; }

        #endregion

        #region Methods

        public static void DebugBinding(object sender, string tag, string message, object[] args = null)
        {
            BindingDebugger?.Invoke(sender, tag, message, args);
        }

        public static void RaiseBindingException(IDataBinding binding, BindingEventArgs args)
        {
            BindingExceptionHandler?.Invoke(binding, args);
        }

        public static void Initialize(IBindingProvider bindingProvider = null, IBindingManager bindingManager = null,
            IBindingResourceResolver resourceResolver = null, IBindingMemberProvider memberProvider = null, IVisualTreeManager visualTreeManager = null,
            IWeakEventManager weakEventManager = null, IObserverProvider observerProvider = null, IBindingContextManager contextManager = null, IBindingErrorProvider errorProvider = null,
            Func<IBindingMemberInfo, Type, object, object> converter = null, Func<string, IBindingPath> bindingPathFactory = null,
            Func<Type, string, IBindingMemberInfo> findUpdateEvent = null, Func<CultureInfo> bindingCultureInfo = null, IDictionary<string, IBindingBehavior> bindingModeBehaviors = null)
        {
            ValueConverter = converter ?? ValueConverterDefaultImpl;
            BindingProvider = bindingProvider ?? new BindingProvider();
            BindingManager = bindingManager ?? new BindingManager();
            ResourceResolver = resourceResolver ?? new BindingResourceResolver();
            MemberProvider = memberProvider ?? new BindingMemberProvider();
            VisualTreeManager = visualTreeManager ?? new VisualTreeManager();
            WeakEventManager = weakEventManager ?? new WeakEventManager();
            ObserverProvider = observerProvider ?? new ObserverProvider();
            ContextManager = contextManager ?? new BindingContextManager();
            BindingPathFactory = bindingPathFactory ?? BindingPathFactoryDefaultImpl;
            UpdateEventFinder = findUpdateEvent ?? FindUpdateEventDefaultImpl;
            BindingCultureInfo = bindingCultureInfo ?? BindingCultureInfoDefaultImpl;
            ErrorProvider = errorProvider;
            if (bindingModeBehaviors == null)
                InitializeDefaultBindingModeBehaviors();
            else
            {
                foreach (var behavior in bindingModeBehaviors)
                    BindingModeToBehavior[behavior.Key] = behavior.Value;
            }
        }

        public static void InitializeFromDesignContext()
        {
            if (_bindingProvider == null)
            {
                var methodInfo = typeof(BindingServiceProvider).GetMethodEx(nameof(SetDefaultValues), MemberFlags.Static | MemberFlags.NonPublic | MemberFlags.Public);
                if (methodInfo != null)
                    methodInfo.Invoke(null, null);
            }
        }

        internal static void SetDefaultValues()
        {
            Initialize();
        }

        public static CultureInfo BindingCultureInfoDefaultImpl()
        {
            return CultureInfo.CurrentCulture;
        }

        public static void InitializeDefaultBindingModeBehaviors()
        {
            BindingModeToBehavior["Default"] = null;
            BindingModeToBehavior["TwoWay"] = new TwoWayBindingMode();
            BindingModeToBehavior["OneWay"] = new OneWayBindingMode();
            BindingModeToBehavior["OneTime"] = new OneTimeBindingMode();
            BindingModeToBehavior["OneWayToSource"] = new OneWayToSourceBindingMode();
            BindingModeToBehavior["None"] = NoneBindingMode.Instance;
        }

        public static IBindingMemberInfo FindUpdateEventDefaultImpl(Type type, string memberName)
        {
            IBindingMemberInfo member = MemberProvider.GetBindingMember(type, memberName + AttachedMemberConstants.ChangedEventPostfix, false, false);
            if (member == null || member.MemberType != BindingMemberType.Event)
                member = MemberProvider.GetBindingMember(type, memberName + "Change", false, false);
            if (member == null || member.MemberType != BindingMemberType.Event)
                return null;
            return member;
        }

        public static IBindingPath BindingPathFactoryDefaultImpl(string path)
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

        private static object ValueConverterDefaultImpl(IBindingMemberInfo bindingMember, Type type, object value)
        {
            return value;
        }

        private static void BindingDebuggerImpl(object sender, string tag, string message, object[] args = null)
        {
            Tracer.Error($"{tag}: {message}");
        }

        private static void BindingExceptionHandlerImpl(IDataBinding dataBinding, BindingEventArgs bindingEventArgs)
        {
            if (bindingEventArgs.Exception != null)
                Tracer.Error(bindingEventArgs.Exception.Message);
        }

        #endregion
    }
}
