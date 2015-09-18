#region Copyright

// ****************************************************************************
// <copyright file="BindingExtensions.cs">
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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Attributes;
using MugenMvvmToolkit.Binding.Builders;
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Accessors;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Parse.Nodes;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

// ReSharper disable once CheckNamespace

namespace MugenMvvmToolkit.Binding
{
    /// <summary>
    ///     Represents the binding extensions.
    /// </summary>
    public static class BindingExtensions
    {
        #region Nested types

        private sealed class BindingContextWrapper : IBindingContext, IEventListener
        {
            #region Fields

            private readonly IBindingContext _innerContext;
            private IBindingContext _parentContext;
            //NOTE to keep observer reference.
            // ReSharper disable once NotAccessedField.Local
            private IDisposable _parentListener;

            #endregion

            #region Constructors

            public BindingContextWrapper(object target)
            {
                IBindingMemberInfo parentMember =
                    BindingServiceProvider.VisualTreeManager.GetParentMember(target.GetType());
                if (parentMember != null)
                    _parentListener = parentMember.TryObserve(target, this);
                Update(target);
                _innerContext = BindingServiceProvider.ContextManager.GetBindingContext(target);
            }

            #endregion

            #region Implementation of interfaces

            public object Source
            {
                get { return _innerContext; }
            }

            public object Value
            {
                get
                {
                    if (_parentContext == null)
                        return null;
                    return _parentContext.Value;
                }
                set { _innerContext.Value = value; }
            }

            public bool IsAlive
            {
                get { return _innerContext == null || _innerContext.IsAlive; }
            }

            public event EventHandler<ISourceValue, EventArgs> ValueChanged;

            public bool IsWeak
            {
                get { return true; }
            }

            public bool TryHandle(object sender, object message)
            {
                if (!(sender is IBindingContext) && _innerContext != null)
                {
                    lock (this)
                        Update(_innerContext.Source);
                }
                RaiseValueChanged();
                return true;
            }

            #endregion

            #region Methods

            private void Update(object source)
            {
                if (_parentContext != null)
                    WeakEventManager.RemoveBindingContextListener(_parentContext, this);
                if (source == null)
                    _parentContext = null;
                else
                {
                    _parentContext = GetParentBindingContext(source);
                    if (_parentContext != null)
                        WeakEventManager.AddBindingContextListener(_parentContext, this, false);
                }
            }

            private static IBindingContext GetParentBindingContext(object target)
            {
                object parent = BindingServiceProvider.VisualTreeManager.FindParent(target);
                if (parent == null)
                    return null;
                return BindingServiceProvider.ContextManager.GetBindingContext(parent);
            }

            private void RaiseValueChanged()
            {
                EventHandler<ISourceValue, EventArgs> handler = ValueChanged;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }

            #endregion
        }

        private sealed class ParentSourceValue : ISourceValue, IEventListener
        {
            #region Fields

            private readonly bool _isElementSource;
            private readonly IRelativeSourceExpressionNode _node;
            private readonly IDisposable _subscriber;
            private readonly WeakReference _targetReference;
            private bool _hasParent;
            private WeakReference _value;

            #endregion

            #region Constructors

            public ParentSourceValue(object target, IRelativeSourceExpressionNode node)
            {
                _node = node;
                _isElementSource = _node.Type == RelativeSourceExpressionNode.ElementSourceType;
                _targetReference = ServiceProvider.WeakReferenceFactory(target);
                _value = Empty.WeakReference;
                IBindingMemberInfo rootMember = BindingServiceProvider.VisualTreeManager.GetRootMember(target.GetType());
                if (rootMember != null)
                    _subscriber = rootMember.TryObserve(target, this);
                TryHandle(null, null);
            }

            #endregion

            #region Implementation of interfaces

            public bool IsWeak
            {
                get { return true; }
            }

            public bool TryHandle(object sender, object message)
            {
                object target = _targetReference.Target;
                if (target == null)
                {
                    Value = null;
                    if (_subscriber != null)
                        _subscriber.Dispose();
                    return false;
                }

                IVisualTreeManager treeManager = BindingServiceProvider.VisualTreeManager;
                _hasParent = treeManager.FindParent(target) != null;
                Value = _isElementSource
                    ? treeManager.FindByName(target, _node.ElementName)
                    : treeManager.FindRelativeSource(target, _node.Type, _node.Level);
                return true;
            }

            public object Value
            {
                get
                {
                    object target = _value.Target;
                    if (_hasParent && target == null)
                    {
                        if (_isElementSource)
                            Tracer.Warn(BindingExceptionManager.ElementSourceNotFoundFormat2, _targetReference.Target,
                                _node.ElementName);
                        else
                            Tracer.Warn(BindingExceptionManager.RelativeSourceNotFoundFormat3, _targetReference.Target,
                                _node.Type, _node.Level);
                    }
                    return target ?? BindingConstants.UnsetValue;
                }
                private set
                {
                    if (Equals(value, _value.Target))
                        return;
                    _value = ToolkitExtensions.GetWeakReferenceOrDefault(value, Empty.WeakReference, false);
                    EventHandler<ISourceValue, EventArgs> handler = ValueChanged;
                    if (handler != null)
                        handler(this, EventArgs.Empty);
                }
            }

            public bool IsAlive
            {
                get { return _targetReference.Target != null; }
            }

            public event EventHandler<ISourceValue, EventArgs> ValueChanged;

            #endregion
        }

        private sealed class WeakEventListener : IEventListener
        {
            #region Fields

            private WeakReference _listenerRef;

            #endregion

            #region Constructors

            public WeakEventListener()
            {
            }

            public WeakEventListener(IEventListener listener)
            {
                _listenerRef = ToolkitExtensions.GetWeakReference(listener);
            }

            #endregion

            #region Implementation of IEventListener

            public bool IsAlive
            {
                get
                {
                    WeakReference listenerRef = _listenerRef;
                    return listenerRef != null && listenerRef.Target != null;
                }
            }

            public bool IsWeak
            {
                get { return true; }
            }

            public bool TryHandle(object sender, object message)
            {
                WeakReference reference = _listenerRef;
                if (reference == null)
                    return false;
                var listener = (IEventListener)reference.Target;
                if (listener == null)
                {
                    _listenerRef = null;
                    return false;
                }
                return listener.TryHandle(sender, message);
            }

            #endregion
        }

        #endregion

        #region Fields

        /// <summary>
        ///     Gets the array with single null value.
        /// </summary>
        public static readonly object[] NullValue;

        internal static readonly IEventListener EmptyListener;
        internal static readonly Func<IDataContext, string, IObserver> CreteBindingSourceDel;

        private static readonly Func<string, string, string> MergePathDelegate;
        private static readonly Dictionary<Delegate, string> DelegateToPathCache;

        #endregion

        #region Constructors

        static BindingExtensions()
        {
            CreteBindingSourceDel = (context, s) => CreateBindingSource(context, s, null);
            EmptyListener = new WeakEventListener();
            DelegateToPathCache = new Dictionary<Delegate, string>(ReferenceEqualityComparer.Instance);
            NullValue = new object[] { null };
            MergePathDelegate = MergePath;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Attempts to subscribe to the event.
        /// </summary>
        public static IDisposable TrySubscribe([NotNull] this IWeakEventManager eventManager, [NotNull] object target,
            string eventName, IEventListener listener,
            IDataContext context = null)
        {
            Should.NotBeNull(eventManager, "eventManager");
            Should.NotBeNull(target, "target");
            EventInfo @event = target.GetType().GetEventEx(eventName, MemberFlags.Instance | MemberFlags.Public);
            if (@event == null)
                throw BindingExceptionManager.MissingEvent(target, eventName);
            return eventManager.TrySubscribe(target, @event, listener, context);
        }

        /// <summary>
        ///     Handles the message.
        /// </summary>
        /// <param name="listener">The specified event listener.</param>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="message">Information about event.</param>
        public static void Handle<TSender, TMessage>(this IEventListener listener, TSender sender, TMessage message)
        {
            Should.NotBeNull(listener, "listener");
            listener.TryHandle(sender, message);
        }

        /// <summary>
        ///     Converts the specified <see cref="IEventListener" /> to a weak <see cref="IEventListener" /> if listener is not
        ///     weak.
        /// </summary>
        public static IEventListener ToWeakEventListener(this IEventListener listener)
        {
            if (listener.IsWeak)
                return listener;
            return new WeakEventListener(listener);
        }

        /// <summary>
        ///     Converts the specified <see cref="IEventListener" /> to a <see cref="WeakReference" /> if listener is not weak.
        /// </summary>
        public static WeakEventListenerWrapper ToWeakWrapper(this IEventListener target)
        {
            return new WeakEventListenerWrapper(target);
        }

        /// <summary>
        ///     Registers the specified member.
        /// </summary>
        public static void Register<TTarget, TType>([NotNull] this IBindingMemberProvider memberProvider,
            [NotNull] IAttachedBindingMemberInfo<TTarget, TType> member, bool rewrite = true)
            where TTarget : class
        {
            memberProvider.Register(member.Path, member, rewrite);
        }

        /// <summary>
        ///     Registers the specified member.
        /// </summary>
        public static void Register<TTarget, TType>([NotNull] this IBindingMemberProvider memberProvider, string path,
            [NotNull] IAttachedBindingMemberInfo<TTarget, TType> member, bool rewrite = true)
            where TTarget : class
        {
            Should.NotBeNull(memberProvider, "memberProvider");
            memberProvider.Register(typeof(TTarget), path, member, rewrite);
        }

        /// <summary>
        ///     Adds the specified object to resources.
        /// </summary>
        public static BindingResourceObject AddObject([NotNull] this IBindingResourceResolver resolver, [NotNull] string name,
            object value, bool rewrite = true)
        {
            Should.NotBeNull(resolver, "resolver");
            var resourceObject = new BindingResourceObject(value);
            resolver.AddObject(name, resourceObject, rewrite);
            return resourceObject;
        }

        /// <summary>
        ///     Adds the specified type to resources.
        /// </summary>
        public static void AddType([NotNull] this IBindingResourceResolver resolver, [NotNull] Type type,
            bool rewrite = true)
        {
            Should.NotBeNull(resolver, "resolver");
            resolver.AddType(type.Name, type, rewrite);
            resolver.AddType(type.FullName, type, rewrite);
        }

        /// <summary>
        ///     Adds the specified converter to resources.
        /// </summary>
        public static void AddConverter([NotNull] this IBindingResourceResolver resolver,
            [NotNull] IBindingValueConverter converter, Type type = null, bool rewrite = true)
        {
            Should.NotBeNull(resolver, "resolver");
            Should.NotBeNull(converter, "converter");
            if (type == null)
                type = converter.GetType();
            string name = RemoveTail(RemoveTail(RemoveTail(type.Name, "BindingValueConverter"), "ValueConverter"),
                "Converter");
            resolver.AddConverter(name, converter, rewrite);
            if (name != type.Name)
                resolver.AddConverter(type.Name, converter, rewrite);
        }

        /// <summary>
        ///     Adds the specified method to resources.
        /// </summary>
        public static void AddMethod<TArg1, TResult>([NotNull] this IBindingResourceResolver resolver,
            [NotNull] string name, [NotNull] Func<TArg1, IDataContext, TResult> method, bool rewrite = true)
        {
            Should.NotBeNull(resolver, "resolver");
            resolver.AddMethod(name,
                new BindingResourceMethod(method.AsResourceMethodDelegate, typeof(TResult)), rewrite);
        }

        /// <summary>
        ///     Adds the specified method to resources.
        /// </summary>
        public static void AddMethod<TArg1, TArg2, TResult>([NotNull] this IBindingResourceResolver resolver,
            [NotNull] string name, [NotNull] Func<TArg1, TArg2, IDataContext, TResult> method, bool rewrite = true)
        {
            Should.NotBeNull(resolver, "resolver");
            resolver.AddMethod(name,
                new BindingResourceMethod(method.AsResourceMethodDelegate, typeof(TResult)), rewrite);
        }

        /// <summary>
        ///     Gets the binding context for the specified item.
        /// </summary>
        public static IBindingContext GetBindingContext([NotNull] this IBindingContextManager contextManager,
            [NotNull] object target, [NotNull] string targetPath)
        {
            Should.NotBeNull(contextManager, "contextManager");
            if (BindingServiceProvider.DataContextMemberAliases.Contains(targetPath))
                return new BindingContextWrapper(target);
            return contextManager.GetBindingContext(target);
        }

        /// <summary>
        ///     Gets the member names from the specified expression.
        /// </summary>
        [Pure]
        public static string GetMemberPath(Func<LambdaExpression> getExpression, string separator = ".")
        {
            Should.NotBeNull(getExpression, "getExpression");
            if (getExpression.HasClosure())
            {
                LambdaExpression expression = getExpression();
                expression.TraceClosureWarn();
                return GetMemberPath(expression, separator);
            }
            lock (DelegateToPathCache)
            {
                string value;
                if (!DelegateToPathCache.TryGetValue(getExpression, out value))
                {
                    value = GetMemberPath(getExpression(), separator);
                    DelegateToPathCache[getExpression] = value;
                }
                return value;
            }
        }

        /// <summary>
        ///     Gets the value of binding member.
        /// </summary>
        public static object GetValue([CanBeNull] this BindingActionValue actionValue, object[] args)
        {
            if (actionValue == null)
                return BindingConstants.UnsetValue;
            object source = actionValue.MemberSource.Target;
            if (source == null)
                return BindingConstants.UnsetValue;
            try
            {
                return actionValue.Member.GetValue(source, args);
            }
            catch (Exception exception)
            {
                Tracer.Error(exception.Flatten(false));
                return BindingConstants.UnsetValue;
            }
        }

        /// <summary>
        ///     Tries to set value.
        /// </summary>
        public static bool TrySetValue<TResult>([CanBeNull] this BindingActionValue actionValue, object[] args,
            out TResult result)
        {
            result = default(TResult);
            if (actionValue == null)
                return false;
            object source = actionValue.MemberSource.Target;
            if (actionValue.MemberSource.Target == null)
                return false;
            try
            {
                object value = actionValue.Member.SetValue(source, args);
                if (value is TResult)
                    result = (TResult)value;
            }
            catch (Exception exception)
            {
                Tracer.Error(exception.Flatten(false));
                return false;
            }
            return true;
        }

        public static object GetValueFromPath(object src, string strPath, int firstMemberIndex = 0)
        {
            IBindingPath path = BindingServiceProvider.BindingPathFactory(strPath);
            for (int index = firstMemberIndex; index < path.Parts.Count; index++)
            {
                string item = path.Parts[index];
                if (src == null)
                    return null;
                IBindingMemberInfo member = BindingServiceProvider
                    .MemberProvider
                    .GetBindingMember(src.GetType(), item, false, true);
                src = member.GetValue(src, null);
            }
            return src;
        }

        public static string MergePath([CanBeNull] string left, [CanBeNull] string right)
        {
            if (string.IsNullOrEmpty(right))
                return left;
            if (string.IsNullOrEmpty(left))
                left = right;
            else
            {
                if (right[0] == '[')
                    left += right;
                else
                    left += "." + right;
            }
            return left;
        }

        [NotNull]
        public static string MergePath([CanBeNull] IList<string> items)
        {
            if (items == null || items.Count == 0)
                return string.Empty;
            return items.Aggregate(MergePathDelegate);
        }

        [BindingSyntaxMember]
        public static TValue GetBindingMemberValue<TSource, TValue>([NotNull] this TSource source, BindingMemberDescriptor<TSource, TValue> member) where TSource : class
        {
            return source.GetBindingMemberValue(member, Empty.Array<object>());
        }

        public static TValue GetBindingMemberValue<TSource, TValue>([NotNull] this TSource source, BindingMemberDescriptor<TSource, TValue> member, params object[] args)
            where TSource : class
        {
            TValue value;
            source.TryGetBindingMemberValue(member, args, true, out value);
            return value;
        }

        public static bool TryGetBindingMemberValue<TSource, TValue>([CanBeNull] this TSource source, BindingMemberDescriptor<TSource, TValue> member, out TValue value)
            where TSource : class
        {
            return source.TryGetBindingMemberValue(member, out value, Empty.Array<object>());
        }

        public static bool TryGetBindingMemberValue<TSource, TValue>([CanBeNull] this TSource source, BindingMemberDescriptor<TSource, TValue> member, out TValue value, params object[] args) where TSource : class
        {
            return source.TryGetBindingMemberValue(member.Path, args, false, out value);
        }

        public static object SetBindingMemberValue<TSource, TValue>(this TSource source, BindingMemberDescriptor<TSource, TValue> member, TValue value)
            where TSource : class
        {
            return source.SetBindingMemberValue(member, new object[] { value });
        }

        public static object SetBindingMemberValue<TSource, TValue>(this TSource source, BindingMemberDescriptor<TSource, TValue> member, object[] args)
            where TSource : class
        {
            Should.NotBeNull(source, "source");
            return BindingServiceProvider
                .MemberProvider
                .GetBindingMember(source.GetType(), member.Path, false, true)
                .SetValue(source, args);
        }

        public static bool TryRaiseAttachedEvent<TSource, TValue>([CanBeNull] this TSource source, BindingMemberDescriptor<TSource, TValue> member, object message = null)
            where TSource : class
        {
            if (source == null)
                return false;
            var eventMember = BindingServiceProvider
                .MemberProvider
                .GetBindingMember(source.GetType(), member.Path, false, false) as INotifiableAttachedBindingMemberInfo;
            if (eventMember == null)
                return false;
            return eventMember.TryRaise(source, message ?? EventArgs.Empty);
        }

        /// <summary>
        ///     Gets the current data context for the specified item.
        /// </summary>
        [BindingSyntaxMember]
        public static T DataContext<T>(this object item)
        {
            return (T)item.DataContext();
        }

        /// <summary>
        ///     Gets the current data context for the specified item.
        /// </summary>
        [BindingSyntaxMember]
        public static object DataContext<TSource>(this TSource source)
            where TSource : class
        {
            return BindingServiceProvider.ContextManager.GetBindingContext(source).Value;
        }

        public static void SetDataContext<TSource>(this TSource source, object value)
            where TSource : class
        {
            BindingServiceProvider.ContextManager.GetBindingContext(source).Value = value;
        }

        public static IList<IDataBinding> SetBindings<T>([NotNull] this T item, [NotNull] string bindingExpression,
            IList<object> sources = null)
            where T : class
        {
            return BindingServiceProvider.BindingProvider.CreateBindingsFromString(item, bindingExpression, sources);
        }

        public static T SetBindings<T, TBindingSet>([NotNull] this T item, [NotNull] TBindingSet bindingSet,
            [NotNull] string bindings, object[] sources = null)
            where T : class
            where TBindingSet : BindingSet
        {
            Should.NotBeNull(item, "item");
            Should.NotBeNull(bindingSet, "bindingSet");
            Should.NotBeNull(bindings, "bindings");
            bindingSet.BindFromExpression(item, bindings, sources);
            return item;
        }

        public static T SetBindings<T, TBindingSet>([NotNull] this T item, [NotNull] TBindingSet bindingSet,
            [NotNull] Action<TBindingSet, T> setBinding)
            where T : class
            where TBindingSet : BindingSet
        {
            Should.NotBeNull(item, "item");
            Should.NotBeNull(bindingSet, "bindingSet");
            Should.NotBeNull(setBinding, "setBinding");
            setBinding(bindingSet, item);
            return item;
        }

        public static void ClearBindings<T>([CanBeNull] this T item, bool clearDataContext, bool clearAttachedValues)
            where T : class
        {
            if (item == null)
                return;
            try
            {
                BindingServiceProvider.BindingManager.ClearBindings(item);
                if (clearDataContext && BindingServiceProvider.ContextManager.HasBindingContext(item))
                    BindingServiceProvider.ContextManager.GetBindingContext(item).Value = null;
                if (clearAttachedValues)
                    ServiceProvider.AttachedValueProvider.Clear(item);
            }
            catch (Exception e)
            {
                Tracer.Error(e.Flatten(true));
            }
        }

        /// <summary>
        ///     Creates an instance of <see cref="IObserver" /> using the relative source value.
        /// </summary>
        public static IObserver CreateBindingSource(IRelativeSourceExpressionNode node, [NotNull] object target,
            string pathEx)
        {
            if (target == null)
                throw BindingExceptionManager.InvalidBindingTarget(node.Path);
            string path = node.Path ?? String.Empty;
            if (!String.IsNullOrEmpty(pathEx))
                path = MergePath(path, pathEx);

            if (node.Type != RelativeSourceExpressionNode.SelfType)
            {
                if (node.Type == RelativeSourceExpressionNode.ContextSourceType)
                    target = BindingServiceProvider.ContextManager.GetBindingContext(target);
                else
                    target = new ParentSourceValue(target, node);
            }
            return CreateBindingSource(null, path, target, false);
        }

        public static Exception DuplicateLambdaParameter(string parameterName)
        {
            return BindingExceptionManager.DuplicateLambdaParameter(parameterName);
        }

        /// <summary>
        ///     Gets the value that indicates that all members are available.
        /// </summary>
        public static bool IsAllMembersAvailable(this IBindingSourceAccessor accessor)
        {
            Should.NotBeNull(accessor, "accessor");
            var s = accessor as ISingleBindingSourceAccessor;
            if (s != null)
                return s.Source.GetPathMembers(false).AllMembersAvailable;
            var sources = accessor.Sources;
            for (int i = 0; i < sources.Count; i++)
            {
                if (!sources[i].GetPathMembers(false).AllMembersAvailable)
                    return false;
            }
            return true;
        }

        /// <summary>
        ///     Creates an instance of <see cref="IObserver" /> using the DataContext or source value if any.
        /// </summary>
        internal static IObserver CreateBindingSource(IDataContext context, string path, object src,
            bool ignoreSrc = false)
        {
            if (src == null && (ignoreSrc || !context.TryGetData(BindingBuilderConstants.Source, out src)))
                src = BindingServiceProvider
                    .ContextManager
                    .GetBindingContext(context.GetData(BindingBuilderConstants.Target, true),
                        context.GetData(BindingBuilderConstants.TargetPath, true).Path);
            return BindingServiceProvider
                .ObserverProvider
                .Observe(src, BindingServiceProvider.BindingPathFactory(path), false);
        }

        internal static void CheckDuplicateLambdaParameter(ICollection<string> parameters)
        {
            if (parameters.Count == 0)
                return;
            var strings = new HashSet<string>();
            foreach (string parameter in parameters)
            {
                if (strings.Contains(parameter))
                    throw BindingExceptionManager.DuplicateLambdaParameter(parameter);
                strings.Add(parameter);
            }
        }

        internal static string TryGetMemberName(this IExpressionNode target, bool allowIndexer, bool allowDynamicMember,
            IList<IExpressionNode> nodes = null, List<string> members = null)
        {
            if (target == null)
                return null;
            if (members == null)
                members = new List<string>();
            while (target != null)
            {
                if (nodes != null)
                    nodes.Insert(0, target);
                var expressionNode = target as IMemberExpressionNode;
                if (expressionNode == null)
                {
                    if (target is ResourceExpressionNode)
                    {
                        if (!allowDynamicMember || members.Count == 0)
                            return null;
                        break;
                    }

                    if (!allowIndexer)
                        return null;
                    var indexExpressionNode = target as IIndexExpressionNode;
                    if (indexExpressionNode == null ||
                        indexExpressionNode.Arguments.Any(arg => arg.NodeType != ExpressionNodeType.Constant))
                        return null;
                    IEnumerable<string> args = indexExpressionNode
                        .Arguments
                        .Cast<IConstantExpressionNode>()
                        .Select(node => node.Value.ToStringValue());
                    members.Insert(0, string.Format("[{0}]", string.Join(",", args)).Trim());
                    target = indexExpressionNode.Object;
                }
                else
                {
                    string memberName = expressionNode.Member.Trim();
                    members.Insert(0, memberName);
                    target = expressionNode.Target;
                }
            }
            return MergePath(members);
        }

        internal static bool IsUnsetValueOrDoNothing(this object obj)
        {
            if (obj is DataConstant)
                return ReferenceEquals(obj, BindingConstants.UnsetValue) || ReferenceEquals(obj, BindingConstants.DoNothing);
            return false;
        }

        internal static bool IsUnsetValue(this object obj)
        {
            return ReferenceEquals(obj, BindingConstants.UnsetValue);
        }

        internal static bool IsDoNothing(this object obj)
        {
            return ReferenceEquals(obj, BindingConstants.DoNothing);
        }

        internal static T GetValueOrDefault<T>(this Func<IDataContext, T> getValue, IDataContext context,
            T value = default(T))
        {
            if (getValue == null)
                return value;
            return getValue(context);
        }

        [Pure]
        internal static bool TryGetMemberPath(Expression expression, string separator, bool throwOnError,
            out Expression lastExpression, out string path)
        {
            lastExpression = expression;
            path = null;
            if (expression == null)
                return false;
            var ret = new StringBuilder();
            bool error = false;
            while (lastExpression.NodeType != ExpressionType.Parameter)
            {
                if (error)
                {
                    if (throwOnError)
                        throw BindingExceptionManager.InvalidBindingMemberExpression();
                    path = ret.ToString();
                    return false;
                }

                // This happens when a value type gets boxed
                if (lastExpression.NodeType == ExpressionType.Convert ||
                    lastExpression.NodeType == ExpressionType.ConvertChecked)
                {
                    var ue = (UnaryExpression)lastExpression;
                    lastExpression = ue.Operand;
                    continue;
                }

                string memberName;
                var methodCallExpression = lastExpression as MethodCallExpression;
                if (methodCallExpression != null)
                {
                    var memberAttribute = methodCallExpression.Method
                        .GetAttributes()
                        .OfType<BindingSyntaxMemberAttribute>()
                        .FirstOrDefault();

                    if (memberAttribute != null)
                    {
                        if (methodCallExpression.Arguments.Count != 1 && methodCallExpression.Arguments.Count != 2)
                        {
                            error = true;
                            continue;
                        }
                        lastExpression = methodCallExpression.Arguments[0];
                        if (methodCallExpression.Arguments.Count == 2)
                        {
                            if (!methodCallExpression.Arguments[1].TryGetStaticValue(out memberName, false))
                            {
                                error = true;
                                continue;
                            }
                        }
                        else
                            memberName = memberAttribute.GetMemberName(methodCallExpression.Method);
                    }
                    else
                    {
                        if (methodCallExpression.Method.Name != "get_Item")
                        {
                            error = true;
                            continue;
                        }
                        var builder = new StringBuilder("[");
                        var args = methodCallExpression.Arguments;
                        for (int i = 0; i < args.Count; i++)
                        {
                            object value;
                            if (!args[i].TryGetStaticValue(out value, false))
                            {
                                error = true;
                                break;
                            }
                            if (i != 0)
                                builder.Append(",");
                            builder.Append(value.ToStringValue());
                        }
                        if (error)
                            continue;
                        builder.Append("]");
                        memberName = builder.ToString();
                        lastExpression = methodCallExpression.Object;
                    }
                }
                else if (lastExpression.NodeType != ExpressionType.MemberAccess)
                {
                    error = true;
                    continue;
                }
                else
                {
                    var me = (MemberExpression)lastExpression;
                    memberName = me.Member.Name;
                    lastExpression = me.Expression;
                }

                if (ret.Length != 0 && ret[0] != '[')
                    ret.Insert(0, separator);
                ret.Insert(0, memberName);
                if (lastExpression == null)
                    break;
            }
            path = ret.ToString();
            return true;
        }

        internal static object GetCurrentValue(this IObserver source)
        {
            IBindingPathMembers pathMembers = source.GetPathMembers(true);
            object value = pathMembers.LastMember.GetValue(pathMembers.PenultimateValue, null);
            if (value.IsUnsetValue())
                return null;
            var actionValue = value as BindingActionValue;
            if (actionValue == null)
                return value;
            return actionValue.GetValue(null);
        }

        internal static bool TryGetStaticValue<T>(this Expression expression, out T value, bool throwOnError)
        {
            try
            {
                object v;
                var exp = expression as ConstantExpression;
                if (exp == null)
                    v = ExpressionReflectionManager
                        .CreateLambdaExpression(expression, Empty.Array<ParameterExpression>())
                        .Compile()
                        .DynamicInvoke(Empty.Array<object>());
                else
                    v = exp.Value;
                value = (T)BindingServiceProvider.ValueConverter(BindingMemberInfo.Empty, typeof(T), v);
                return true;
            }
            catch (Exception e)
            {
                value = default(T);
                if (throwOnError)
                    throw ExceptionManager.ExpressionShouldBeStaticValue(expression, e);
                return false;
            }
        }

        internal static string RemoveBounds(this string st)
        {
            return st.Substring(1, st.Length - 2);
        }

        [Pure]
        private static string GetMemberPath(LambdaExpression expression, string separator = ".")
        {
            Expression lastExpression;
            string path;
            if (TryGetMemberPath(expression.Body, separator, true, out lastExpression, out path))
                return path;
            return null;
        }

        private static object AsResourceMethodDelegate<TArg1, TResult>(this Func<TArg1, IDataContext, TResult> method,
            IList<Type> types, IList<object> args, IDataContext context)
        {
            return method((TArg1)args[0], context);
        }

        private static object AsResourceMethodDelegate<TArg1, TArg2, TResult>(
            this Func<TArg1, TArg2, IDataContext, TResult> method, IList<Type> types, IList<object> args,
            IDataContext context)
        {
            return method((TArg1)args[0], (TArg2)args[1], context);
        }

        private static string RemoveTail(string name, string word)
        {
            if (name.EndsWith(word, StringComparison.OrdinalIgnoreCase))
                name = name.Substring(0, name.Length - word.Length);
            return name;
        }

        private static string ToStringValue(this object o)
        {
            if (o == null)
                return "null";
            if (o is string)
                return "\"" + o + "\"";
            return o.ToString();
        }

        private static bool TryGetBindingMemberValue<TSource, TValue>(this TSource source, BindingMemberDescriptor<TSource, TValue> member, object[] args, bool throwOnError, out TValue value)
            where TSource : class
        {
            if (throwOnError)
                Should.NotBeNull(source, "source");
            else if (source == null)
            {
                value = default(TValue);
                return false;
            }
            value = default(TValue);
            var info = BindingServiceProvider
                .MemberProvider
                .GetBindingMember(source.GetType(), member.Path, false, throwOnError);
            if (info == null)
                return false;
            var o = info.GetValue(source, args);
            if (throwOnError || o is TValue)
            {
                value = (TValue)o;
                return true;
            }
            return false;
        }

        #endregion
    }
}