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
using System.Diagnostics;
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
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes;
using MugenMvvmToolkit.Binding.Interfaces.Sources;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Parse.Nodes;
using MugenMvvmToolkit.Binding.Sources;
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
                var parentMember = BindingServiceProvider.VisualTreeManager.GetParentMember(target.GetType());
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

            public event EventHandler<ISourceValue, EventArgs> ValueChanged;

            #endregion

            #region Methods

            private void Update(object source)
            {
                if (_parentContext != null)
                {
                    var src = _parentContext.Source;
                    if (src != null)
                        WeakEventManager.GetBindingContextListener(src).Remove(this);
                }
                if (source == null)
                    _parentContext = null;
                else
                {
                    _parentContext = GetParentBindingContext(source);
                    if (_parentContext != null)
                    {
                        var src = _parentContext.Source;
                        if (src != null)
                            WeakEventManager.GetBindingContextListener(src).Add(this);
                    }
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
                var handler = ValueChanged;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }

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
                    var listenerRef = _listenerRef;
                    return listenerRef != null && listenerRef.Target != null;
                }
            }

            public bool IsWeak
            {
                get { return true; }
            }

            public bool TryHandle(object sender, object message)
            {
                var reference = _listenerRef;
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
        /// Gets the array with single null value.
        /// </summary>
        public static readonly object[] NullValue;

        /// <summary>
        /// Gets the attached parent member.
        /// </summary>
        public readonly static INotifiableAttachedBindingMemberInfo<object, object> AttachedParentMember;

        internal readonly static IEventListener EmptyListener;
        internal static readonly Func<IDataContext, string, IBindingSource> CreteBindingSourceFromSelfDel;
        internal static readonly Func<IDataContext, string, IBindingSource> CreteBindingSourceFromContextDel;
        internal static readonly Func<IDataContext, string, IBindingSource> CreteBindingSourceFromSourceDel;

        private static readonly Func<string, string, string> MergePathDelegate;
        private static readonly Dictionary<Delegate, string> DelegateToPathCache;

        #endregion

        #region Constructors

        static BindingExtensions()
        {
            CreteBindingSourceFromSelfDel = CreateBindingSourceSelf;
            CreteBindingSourceFromContextDel = CreateBindingSource;
            CreteBindingSourceFromSourceDel = (context, s) => CreateBindingSourceExplicit(context, s, null);
            EmptyListener = new WeakEventListener();
            DelegateToPathCache = new Dictionary<Delegate, string>(ReferenceEqualityComparer.Instance);
            AttachedParentMember = AttachedBindingMember.CreateAutoProperty<object, object>("#" + AttachedMemberConstants.Parent);
            NullValue = new object[] { null };
            MergePathDelegate = MergePath;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Attempts to subscribe to the event.        
        /// </summary>
        public static IDisposable TrySubscribe([NotNull] this IWeakEventManager eventManager, [NotNull] object target, string eventName, IEventListener listener,
            IDataContext context = null)
        {
            Should.NotBeNull(eventManager, "eventManager");
            Should.NotBeNull(target, "target");
            var @event = target.GetType().GetEventEx(eventName, MemberFlags.Instance | MemberFlags.Public);
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
        /// Converts the specified <see cref="IEventListener"/> to a weak <see cref="IEventListener"/> if listener is not weak.
        /// </summary>
        public static IEventListener ToWeakEventListener(this IEventListener listener)
        {
            if (listener.IsWeak)
                return listener;
            return new WeakEventListener(listener);
        }

        /// <summary>
        /// Converts the specified <see cref="IEventListener"/> to a <see cref="WeakReference"/> if listener is not weak.
        /// </summary>
        public static WeakEventListenerWrapper ToWeakWrapper(this IEventListener target)
        {
            return new WeakEventListenerWrapper(target);
        }

        /// <summary>
        ///     Registers the specified member.
        /// </summary>
        public static void Register<TTarget, TType>([NotNull]this IBindingMemberProvider memberProvider, [NotNull] IAttachedBindingMemberInfo<TTarget, TType> member, bool rewrite = true)
        {
            memberProvider.Register(member.Path, member, rewrite);
        }

        /// <summary>
        ///     Registers the specified member.
        /// </summary>
        public static void Register<TTarget, TType>([NotNull]this IBindingMemberProvider memberProvider, string path, [NotNull] IAttachedBindingMemberInfo<TTarget, TType> member, bool rewrite = true)
        {
            Should.NotBeNull(memberProvider, "memberProvider");
            memberProvider.Register(typeof(TTarget), path, member, rewrite);
        }

        /// <summary>
        ///     Adds the specified object to resources.
        /// </summary>
        public static void AddObject([NotNull]this IBindingResourceResolver resolver, [NotNull] string name, object value, bool rewrite = true)
        {
            Should.NotBeNull(resolver, "resolver");
            resolver.AddObject(name, new BindingResourceObject(value), rewrite);
        }

        /// <summary>
        ///     Adds the specified type to resources.
        /// </summary>
        public static void AddType([NotNull]this IBindingResourceResolver resolver, [NotNull] Type type,
            bool rewrite = true)
        {
            Should.NotBeNull(resolver, "resolver");
            resolver.AddType(type.Name, type, rewrite);
            resolver.AddType(type.FullName, type, rewrite);
        }

        /// <summary>
        ///     Adds the specified converter to resources.
        /// </summary>
        public static void AddConverter([NotNull]this IBindingResourceResolver resolver, [NotNull] IBindingValueConverter converter, Type type = null, bool rewrite = true)
        {
            Should.NotBeNull(resolver, "resolver");
            Should.NotBeNull(converter, "converter");
            if (type == null)
                type = converter.GetType();
            var name = RemoveTail(RemoveTail(RemoveTail(type.Name, "BindingValueConverter"), "ValueConverter"), "Converter");
            resolver.AddConverter(name, converter, rewrite);
            if (name != type.Name)
                resolver.AddConverter(type.Name, converter, rewrite);
        }

        /// <summary>
        ///     Adds the specified method to resources.
        /// </summary>
        public static void AddMethod<TArg1, TResult>([NotNull]this IBindingResourceResolver resolver, [NotNull] string name, [NotNull] Func<TArg1, IDataContext, TResult> method, bool rewrite = true)
        {
            Should.NotBeNull(resolver, "resolver");
            resolver.AddMethod(name,
                new BindingResourceMethod(method.AsResourceMethodDelegate, typeof(TResult)), rewrite);
        }

        /// <summary>
        ///     Adds the specified method to resources.
        /// </summary>
        public static void AddMethod<TArg1, TArg2, TResult>([NotNull]this IBindingResourceResolver resolver, [NotNull] string name, [NotNull] Func<TArg1, TArg2, IDataContext, TResult> method, bool rewrite = true)
        {
            Should.NotBeNull(resolver, "resolver");
            resolver.AddMethod(name,
                new BindingResourceMethod(method.AsResourceMethodDelegate, typeof(TResult)), rewrite);
        }

        /// <summary>
        ///     Gets the binding context for the specified item.
        /// </summary>
        public static IBindingContext GetBindingContext([NotNull] this IBindingContextManager contextManager, [NotNull] object target, [NotNull] string targetPath)
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
            if (getExpression.Target != null)
            {
                var expression = getExpression();
                if (Debugger.IsAttached)
                    Tracer.Warn("The expression '{0}' has closure, it can lead to poor performance", expression);
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
        ///     Gets the member names from the specified expression.
        /// </summary>        
        [Pure]
        public static string GetMemberPath(LambdaExpression expression, string separator = ".")
        {
            Expression lastExpression;
            string path;
            if (TryGetMemberPath(expression.Body, separator, true, out lastExpression, out path))
                return path;
            return null;
        }

        /// <summary>
        ///     Gets the value of binding member.
        /// </summary>
        public static object GetMemberValue([NotNull] this IBindingMemberProvider memberProvider, [NotNull] object item,
            [NotNull] string path, object[] args = null)
        {
            Should.NotBeNull(memberProvider, "memberProvider");
            Should.NotBeNull(item, "item");
            return memberProvider.GetBindingMember(item.GetType(), path, false, true).GetValue(item, args);
        }

        /// <summary>
        ///     Sets the value of binding member.
        /// </summary>
        public static object SetMemberValue([NotNull] this IBindingMemberProvider memberProvider, [NotNull] object item,
            [NotNull] string path, object[] args = null)
        {
            Should.NotBeNull(memberProvider, "memberProvider");
            Should.NotBeNull(item, "item");
            return memberProvider.GetBindingMember(item.GetType(), path, false, true).SetValue(item, args);
        }

        /// <summary>
        ///     Gets the value of binding member.
        /// </summary>
        public static object GetValue([CanBeNull]this BindingMemberValue memberValue, object[] args)
        {
            if (memberValue == null)
                return BindingConstants.UnsetValue;
            object source = memberValue.MemberSource.Target;
            if (source == null)
                return BindingConstants.UnsetValue;
            try
            {
                return memberValue.Member.GetValue(source, args);
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
        public static bool TrySetValue<TResult>([CanBeNull]this BindingMemberValue memberValue, object[] args, out TResult result)
        {
            result = default(TResult);
            if (memberValue == null)
                return false;
            object source = memberValue.MemberSource.Target;
            if (memberValue.MemberSource.Target == null)
                return false;
            try
            {
                var value = memberValue.Member.SetValue(source, args);
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
                var item = path.Parts[index];
                if (src == null)
                    return null;
                IBindingMemberInfo member = BindingServiceProvider
                    .MemberProvider
                    .GetBindingMember(src.GetType(), item, false, true);
                src = member.GetValue(src, null);
            }
            return src;
        }

        public static string MergePath([CanBeNull]string left, [CanBeNull] string right)
        {
            if (string.IsNullOrEmpty(right))
                return left;
            if (string.IsNullOrEmpty(left))
                left = right;
            else
            {
                if (right.StartsWith("["))
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

        [CanBeNull]
        public static IBindingMemberInfo TryFindMemberChangeEvent([NotNull] IBindingMemberProvider memberProvider,
            [NotNull] Type type, [NotNull] string memberName)
        {
            Should.NotBeNull(memberProvider, "memberProvider");
            var member = memberProvider.GetBindingMember(type, memberName + "Changed", false, false);
            if (member == null || member.MemberType != BindingMemberType.Event)
                member = memberProvider.GetBindingMember(type, memberName + "Change", false, false);

            if (member == null || member.MemberType != BindingMemberType.Event)
                return null;
            return member;
        }

        public static TValue TryGetValue<TValue>([NotNull] this IBindingMemberProvider memberProvider, object item,
            [NotNull] string memberName, bool ignoreAttachedMembers = false, TValue defaultValue = default(TValue))
        {
            Should.NotBeNull(memberProvider, "memberProvider");
            return memberProvider
                .GetBindingMember(item.GetType(), memberName, ignoreAttachedMembers, false)
                .TryGetValue(item, defaultValue);
        }

        public static TValue TryGetValue<TValue>([CanBeNull] this IBindingMemberInfo bindingMember, [CanBeNull]object item, TValue defaultValue = default(TValue), bool itemCanBeNull = false)
        {
            if (bindingMember == null || item == null)
                return defaultValue;
            var value = bindingMember.GetValue(item, null);
            if (value is TValue)
                return (TValue)value;
            return defaultValue;
        }

        public static TValue TryGetValue<TItem, TValue>([CanBeNull] this IAttachedBindingMemberInfo<TItem, TValue> attachedBindingMember, [CanBeNull] TItem item, TValue defaultValue = default(TValue))
        {
            return TryGetValue(bindingMember: attachedBindingMember, item: item, defaultValue: defaultValue);
        }

        public static T SetBinding<T>([NotNull] this T item, [NotNull] Action<IBindingBuilder, T> setBinding)
            where T : class
        {
            IDataBinding binding;
            return SetBinding(item, setBinding, out binding);
        }

        public static T SetBinding<T>([NotNull]this T item, [NotNull] Action<IBindingBuilder, T> setBinding, out  IDataBinding binding)
            where T : class
        {
            Should.NotBeNull(setBinding, "setBinding");
            var bindingBuilder = BindingServiceProvider.BindingProvider.CreateBuilder();
            setBinding(bindingBuilder, item);
            binding = bindingBuilder.Build();
            return item;
        }

        public static IList<IDataBinding> SetBindings([NotNull]this object item, [NotNull]string bindingExpression, IList<object> sources = null)
        {
            return BindingServiceProvider.BindingProvider.CreateBindingsFromString(item, bindingExpression, sources);
        }

        public static T SetBindings<T, TBindingSet>([NotNull] this T item, [NotNull] TBindingSet bindingSet,
            [NotNull] string bindings)
            where T : class
            where TBindingSet : BindingSet
        {
            Should.NotBeNull(item, "item");
            Should.NotBeNull(bindingSet, "bindingSet");
            Should.NotBeNull(bindings, "bindings");
            bindingSet.BindFromExpression(item, bindings);
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

        public static void ClearBindings<T>([CanBeNull]this T item, bool clearDataContext, bool clearAttachedValues)
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

        public static Exception DuplicateLambdaParameter(string parameterName)
        {
            return BindingExceptionManager.DuplicateLambdaParameter(parameterName);
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

        internal static string TryGetMemberName(this IExpressionNode target, bool allowIndexer, bool allowDynamicMember, IList<IExpressionNode> nodes = null, List<string> members = null)
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
                        .Select(node => node.Value == null ? "null" : node.Value.ToString());
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
                return obj.IsDoNothing() || obj.IsUnsetValue();
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

        internal static T GetValueOrDefault<T>(this Func<IDataContext, T> getValue, IDataContext context, T value = default(T))
        {
            if (getValue == null)
                return value;
            return getValue(context);
        }

        [Pure]
        internal static bool TryGetMemberPath(Expression expression, string separator, bool throwOnError, out Expression lastExpression, out string path)
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
                if (lastExpression.NodeType == ExpressionType.Convert || lastExpression.NodeType == ExpressionType.ConvertChecked)
                {
                    var ue = (UnaryExpression)lastExpression;
                    lastExpression = ue.Operand;
                    continue;
                }

                string memberName;
                var methodCallExpression = lastExpression as MethodCallExpression;
                if (methodCallExpression != null)
                {
                    if (methodCallExpression.Method.IsDefined(typeof(BindingSyntaxMemberAttribute), true))
                    {
                        if (methodCallExpression.Arguments.Count == 0)
                        {
                            error = true;
                            continue;
                        }
                        lastExpression = methodCallExpression.Arguments[0];
                        if (methodCallExpression.Arguments.Count > 1)
                        {
                            var constantExpression = methodCallExpression.Arguments[1] as ConstantExpression;
                            if (constantExpression == null)
                            {
                                error = true;
                                continue;
                            }
                            memberName = (string)constantExpression.Value;
                        }
                        else
                            memberName = methodCallExpression.Method.Name;
                    }
                    else
                    {
                        if (methodCallExpression.Method.Name != "get_Item" ||
                           methodCallExpression.Arguments.Count != methodCallExpression.Arguments.Count(e => e is ConstantExpression))
                        {
                            error = true;
                            continue;
                        }
                        string s = string.Join(",", methodCallExpression.Arguments.Cast<ConstantExpression>().Select(e => e.Value));
                        memberName = "[" + s + "]";
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

                if (ret.Length != 0)
                    ret.Insert(0, separator);
                ret.Insert(0, memberName);
                if (lastExpression == null)
                    break;
            }
            path = ret.ToString();
            return true;
        }

        internal static BindingSource CreateBindingSourceExplicit(IDataContext context, string path, object src)
        {
            if (src == null)
                src = context.GetData(BindingBuilderConstants.Source, true);
            IObserver observer = BindingServiceProvider
                .ObserverProvider
                .Observe(src, BindingServiceProvider.BindingPathFactory(path), false);
            return new BindingSource(observer);
        }

        internal static BindingSource CreateBindingSource(IDataContext context, string path)
        {
            IBindingContext bindingContext = BindingServiceProvider
                     .ContextManager
                     .GetBindingContext(context.GetData(BindingBuilderConstants.Target, true),
                         context.GetData(BindingBuilderConstants.TargetPath, true).Path);
            IObserver observer = BindingServiceProvider.ObserverProvider.Observe(bindingContext, BindingServiceProvider.BindingPathFactory(path), false);
            return new BindingSource(observer);
        }

        internal static BindingSource CreateBindingSourceSelf(IDataContext context, string path)
        {
            object target = context.GetData(BindingBuilderConstants.Target, true);
            return new BindingSource(BindingServiceProvider.ObserverProvider.Observe(target, BindingServiceProvider.BindingPathFactory(path), false));
        }

        private static object AsResourceMethodDelegate<TArg1, TResult>(this Func<TArg1, IDataContext, TResult> method, IList<Type> types, IList<object> args, IDataContext context)
        {
            return method((TArg1)args[0], context);
        }

        private static object AsResourceMethodDelegate<TArg1, TArg2, TResult>(this Func<TArg1, TArg2, IDataContext, TResult> method, IList<Type> types, IList<object> args, IDataContext context)
        {
            return method((TArg1)args[0], (TArg2)args[1], context);
        }

        private static string RemoveTail(string name, string word)
        {
            if (name.EndsWith(word, StringComparison.OrdinalIgnoreCase))
                name = name.Substring(0, name.Length - word.Length);
            return name;
        }

        #endregion
    }
}