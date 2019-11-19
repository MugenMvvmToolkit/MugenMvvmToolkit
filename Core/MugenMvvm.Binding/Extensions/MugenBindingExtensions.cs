using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using MugenMvvm.Binding.Compiling;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Converters;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Members.Descriptors;
using MugenMvvm.Binding.Metadata;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

// ReSharper disable once CheckNamespace
namespace MugenMvvm.Binding
{
    public static partial class MugenBindingExtensions
    {
        #region Fields

        public static readonly char[] CommaSeparator = { ',' };
        public static readonly char[] DotSeparator = { '.' };

        #endregion

        #region Methods

        public static object? Invoke(this ICompiledExpression expression, object? sourceRaw, IReadOnlyMetadataContext? metadata)
        {
            ItemOrList<ExpressionValue, ExpressionValue[]> values;
            if (sourceRaw == null)
                values = Default.EmptyArray<ExpressionValue>();
            else if (sourceRaw is IMemberPathObserver[] sources)
            {
                var expressionValues = new ExpressionValue[sources.Length];
                for (var i = 0; i < sources.Length; i++)
                {
                    var members = sources[i].GetLastMember(metadata);
                    var value = members.GetValue(metadata);
                    if (value.IsUnsetValueOrDoNothing())
                        return value;
                    expressionValues[i] = new ExpressionValue(value?.GetType() ?? members.Member.Type, null);
                }

                values = expressionValues;
            }
            else
            {
                var members = ((IMemberPathObserver)sourceRaw).GetLastMember(metadata);
                var value = members.GetValue(metadata);
                if (value.IsUnsetValueOrDoNothing())
                    return value;

                values = new ExpressionValue(value?.GetType() ?? members.Member.Type, value);
            }
            return expression.Invoke(values, metadata);
        }

        public static bool EqualsEx(this IMemberInfo x, IMemberInfo y)
        {
            if (x.MemberType != y.MemberType || x.Name != y.Name || x.DeclaringType != y.DeclaringType)
                return false;

            if (x.MemberType != MemberType.Method)
                return true;

            var xM = ((IMethodInfo)x).GetParameters();
            var yM = ((IMethodInfo)y).GetParameters();
            if (xM.Count != yM.Count)
                return false;

            for (var i = 0; i < xM.Count; i++)
            {
                if (xM[i].ParameterType != yM[i].ParameterType)
                    return false;
            }

            return true;
        }

        public static int GetHashCodeEx(this IMemberInfo memberInfo)
        {
            unchecked
            {
                return memberInfo.DeclaringType.GetHashCode() * 397 ^ (int)memberInfo.MemberType * 397 ^ memberInfo.Name.GetHashCode();
            }
        }

        public static object? GetParent(object? target, IReadOnlyMetadataContext? metadata = null, IMemberProvider? provider = null)
        {
            return target?.GetBindableMemberValue(BindableMembers.Object.Parent, null, MemberFlags.All, metadata, provider);
        }

        public static object? FindElementSource(object target, string elementName, IReadOnlyMetadataContext? metadata = null, IMemberProvider? provider = null)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(elementName, nameof(elementName));
            var args = new object[1];
            while (target != null)
            {
                args[0] = elementName;
                var result = target.TryInvokeBindableMethod(BindableMembers.Object.FindByName, args, MemberFlags.All, metadata, provider);
                if (result != null)
                    return result;
                target = GetParent(target, metadata, provider)!;
            }

            return null;
        }

        public static object? FindRelativeSource(object target, string typeName, int level, IReadOnlyMetadataContext? metadata = null, IMemberProvider? provider = null)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(typeName, nameof(typeName));
            object? fullNameSource = null;
            object? nameSource = null;
            var fullNameLevel = 0;
            var nameLevel = 0;

            target = GetParent(target, metadata, provider)!;
            while (target != null)
            {
                TypeNameEqual(target.GetType(), typeName, out var shortNameEqual, out var fullNameEqual);
                if (shortNameEqual)
                {
                    nameSource = target;
                    nameLevel++;
                }

                if (fullNameEqual)
                {
                    fullNameSource = target;
                    fullNameLevel++;
                }

                if (fullNameSource != null && fullNameLevel == level)
                    return fullNameSource;
                if (nameSource != null && nameLevel == level)
                    return nameSource;

                target = GetParent(target, metadata, provider)!;
            }

            return null;
        }

        public static bool IsAllMembersAvailable(this ItemOrList<IMemberPathObserver?, IMemberPathObserver[]> observers)
        {
            var list = observers.List;
            if (list == null)
            {
                var item = observers.Item;
                return item == null || item.IsAllMembersAvailable();
            }

            for (var i = 0; i < list.Length; i++)
            {
                if (!list[i].IsAllMembersAvailable())
                    return false;
            }

            return true;
        }

        public static bool IsAllMembersAvailable(this IMemberPathObserver observer)
        {
            return observer.GetLastMember().IsAvailable;
        }

        public static object? GetValueFromPath(this IMemberPath path, Type type, object? src, MemberFlags flags,
            int firstMemberIndex = 0, IReadOnlyMetadataContext? metadata = null, IMemberProvider? memberProvider = null)
        {
            Should.NotBeNull(type, nameof(type));
            memberProvider = memberProvider.DefaultIfNull();
            for (var index = firstMemberIndex; index < path.Members.Length; index++)
            {
                var item = path.Members[index];
                if (src.IsNullOrUnsetValue())
                    return null;

                var member = memberProvider.GetMember(type, item, MemberType.Accessor, flags) as IMemberAccessorInfo;
                if (member == null)
                    BindingExceptionManager.ThrowInvalidBindingMember(type, item);
                src = member.GetValue(src, metadata);
                if (src != null)
                    type = src.GetType();
            }

            return src;
        }

        public static bool TryBuildBindingMember(this IExpressionNode? target, StringBuilder builder, out IExpressionNode? firstExpression)
        {
            return TryBuildBindingMember(target, builder, null, out firstExpression);
        }

        public static bool TryBuildBindingMember(this IExpressionNode? target, StringBuilder builder, Func<IExpressionNode, bool>? condition, out IExpressionNode? firstExpression)
        {
            Should.NotBeNull(builder, nameof(builder));
            firstExpression = null;
            builder.Clear();
            if (target == null)
                return false;
            while (target != null)
            {
                firstExpression = target;
                if (condition != null && !condition(target))
                    return false;

                if (target is IMemberExpressionNode memberExpressionNode)
                {
                    var memberName = memberExpressionNode.MemberName.Trim();
                    builder.Insert(0, memberName);
                    if (memberExpressionNode.Target != null)
                        builder.Insert(0, '.');
                    target = memberExpressionNode.Target;
                }
                else
                {
                    if (target is IIndexExpressionNode indexExpressionNode && indexExpressionNode.Arguments.All(arg => arg.NodeType == ExpressionNodeType.Constant))
                    {
                        var args = indexExpressionNode.Arguments;
                        builder.Insert(0, ']');
                        if (args.Count > 0)
                        {
                            args.Last().ToStringValue(builder);
                            for (var i = args.Count - 2; i >= 0; i--)
                            {
                                builder.Insert(0, ',');
                                args[i].ToStringValue(builder);
                            }
                        }

                        builder.Insert(0, '[');
                        target = indexExpressionNode.Target;
                    }
                    else if (target is IMethodCallExpressionNode methodCallExpression && methodCallExpression.Arguments.All(arg => arg.NodeType == ExpressionNodeType.Constant))
                    {
                        var args = methodCallExpression.Arguments;
                        builder.Insert(0, ')');
                        if (args.Count > 0)
                        {
                            args.Last().ToStringValue(builder);
                            for (var i = args.Count - 2; i >= 0; i--)
                            {
                                builder.Insert(0, ',');
                                args[i].ToStringValue(builder);
                            }
                        }

                        builder.Insert(0, '(');
                        builder.Insert(0, methodCallExpression.MethodName);
                        builder.Insert(0, '.');
                        target = methodCallExpression.Target;
                    }
                    else
                        return false;
                }
            }

            return true;
        }

        public static TValue GetBindableMemberValue<TTarget, TValue>(this TTarget target,
            BindablePropertyDescriptor<TTarget, TValue> bindableMember, TValue defaultValue = default, MemberFlags flags = MemberFlags.All,
            IReadOnlyMetadataContext? metadata = null, IMemberProvider? provider = null) where TTarget : class
        {
            var propertyInfo = provider
                .DefaultIfNull()
                .GetMember(target.GetType(), bindableMember.Name, MemberType.Accessor, flags, metadata) as IMemberAccessorInfo;
            if (propertyInfo == null)
                return defaultValue;
            if (propertyInfo is IMemberAccessorInfo<TTarget, TValue> p)
                return p.GetValue(target, metadata);
            return (TValue)propertyInfo.GetValue(target, metadata)!;
        }

        public static void SetBindableMemberValue<TTarget, TValue>(this TTarget target,
            BindablePropertyDescriptor<TTarget, TValue> bindableMember, TValue value, bool throwOnError = true, MemberFlags flags = MemberFlags.All,
            IReadOnlyMetadataContext? metadata = null, IMemberProvider? provider = null) where TTarget : class
        {
            var propertyInfo = provider
                .DefaultIfNull()
                .GetMember(target.GetType(), bindableMember.Name, MemberType.Accessor, flags, metadata) as IMemberAccessorInfo;
            if (propertyInfo == null)
            {
                if (throwOnError)
                    BindingExceptionManager.ThrowInvalidBindingMember(target.GetType(), bindableMember.Name);
                return;
            }

            if (propertyInfo is IMemberAccessorInfo<TTarget, TValue> p)
                p.SetValue(target, value, metadata);
            else
                propertyInfo.SetValue(target, value, metadata);
        }

        public static ActionToken TryObserveBindableMember<TTarget, TValue>(this TTarget target,
            BindablePropertyDescriptor<TTarget, TValue> bindableMember, IEventListener listener, MemberFlags flags = MemberFlags.All,
            IReadOnlyMetadataContext? metadata = null, IMemberProvider? provider = null) where TTarget : class
        {
            var propertyInfo = provider
                .DefaultIfNull()
                .GetMember(target.GetType(), bindableMember.Name, MemberType.Accessor, flags, metadata) as IObservableMemberInfo;
            if (propertyInfo == null)
                return default;
            return propertyInfo.TryObserve(target, listener, metadata);
        }

        public static ActionToken TrySubscribeBindableEvent<TTarget>(this TTarget target,
            BindableEventDescriptor<TTarget> eventMember, IEventListener listener, MemberFlags flags = MemberFlags.All,
            IReadOnlyMetadataContext? metadata = null, IMemberProvider? provider = null) where TTarget : class
        {
            var eventInfo = provider
                .DefaultIfNull()
                .GetMember(target.GetType(), eventMember.Name, MemberType.Event, flags, metadata) as IEventInfo;
            if (eventInfo == null)
                return default;
            return eventInfo.TrySubscribe(target, listener, metadata);
        }

        public static object? TryInvokeBindableMethod<TTarget>(this TTarget target,
            BindableMethodDescriptor<TTarget> methodMember, object?[]? args = null, MemberFlags flags = MemberFlags.All,
            IReadOnlyMetadataContext? metadata = null, IMemberProvider? provider = null) where TTarget : class
        {
            var methodInfo = provider
                .DefaultIfNull()
                .GetMember(target.GetType(), methodMember.Name, MemberType.Method, flags, metadata) as IMethodInfo;
            return methodInfo?.Invoke(target, args ?? Default.EmptyArray<object>());
        }

        public static WeakEventListener ToWeak(this IEventListener listener)
        {
            return new WeakEventListener(listener);
        }

        public static WeakEventListener<TState> ToWeak<TState>(this IEventListener listener, TState state)
        {
            return new WeakEventListener<TState>(listener, state);
        }

        public static TItem[] ConvertValues<TItem>(this IGlobalValueConverter? converter, string[] args, IReadOnlyMetadataContext? metadata)
        {
            converter = converter.DefaultIfNull();
            var result = new TItem[args.Length];
            for (var i = 0; i < args.Length; i++)
            {
                var s = args[i];
                if (!string.IsNullOrEmpty(s) && s[0] == '\"' && s.EndsWith("\""))
                    s = s.RemoveBounds();
                result[i] = (TItem)(s == "null" ? null : converter.Convert(s, typeof(TItem), metadata: metadata))!;
            }

            return result;
        }

        public static object?[] ConvertValues(this IGlobalValueConverter? converter, string[] args, ParameterInfo[]? parameters,
            Type? castType, IReadOnlyMetadataContext? metadata, int parametersStartIndex = 0)
        {
            if (parameters == null)
                Should.NotBeNull(castType, nameof(castType));
            else
                Should.NotBeNull(parameters, nameof(parameters));
            if (args.Length == 0)
                return Default.EmptyArray<object?>();

            converter = converter.DefaultIfNull();
            var result = new object?[args.Length];
            for (var i = 0; i < args.Length; i++)
            {
                var s = args[i];
                if (parameters != null)
                    castType = parameters[i + parametersStartIndex].ParameterType;
                if (!string.IsNullOrEmpty(s) && s[0] == '\"' && s.EndsWith("\""))
                    s = s.RemoveBounds();
                result[i] = s == "null" ? null : converter.Convert(s, castType!, parameters?[i + parametersStartIndex], metadata);
            }

            return result;
        }

        public static string[]? GetIndexerArgsRaw(string path)
        {
            int start = 1;
            if (path.StartsWith("Item[", StringComparison.Ordinal))
                start = 5;
            else if (!path.StartsWith("[", StringComparison.Ordinal) || !path.EndsWith("]", StringComparison.Ordinal))
                return null;

            return path
                .RemoveBounds(start)
                .Split(CommaSeparator, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string[]? GetMethodArgsRaw(string path, out string methodName)
        {
            var startIndex = path.IndexOf('(');
            if (startIndex < 0 || !path.EndsWith(")", StringComparison.Ordinal))
            {
                methodName = path;
                return null;
            }

            methodName = path.Substring(0, startIndex);
            return path
                .RemoveBounds(startIndex + 1)
                .Split(CommaSeparator, StringSplitOptions.RemoveEmptyEntries);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFlagEx(this MemberFlags value, MemberFlags flag)
        {
            return (value & flag) == flag;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFlagEx(this BindingMemberExpressionFlags value, BindingMemberExpressionFlags flag)
        {
            return (value & flag) == flag;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFlagEx(this MemberType value, MemberType flag)
        {
            return (value & flag) == flag;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsNullOrUnsetValue(this object? value)
        {
            return value == null || ReferenceEquals(value, BindingMetadata.UnsetValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsUnsetValueOrDoNothing(this object? value)
        {
            return ReferenceEquals(value, BindingMetadata.UnsetValue) || ReferenceEquals(value, BindingMetadata.DoNothing);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsDoNothing(this object? value)
        {
            return ReferenceEquals(value, BindingMetadata.DoNothing);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsUnsetValue(this object? value)
        {
            return ReferenceEquals(value, BindingMetadata.UnsetValue);
        }

        internal static string GetPath(this StringBuilder memberNameBuilder)
        {
            if (memberNameBuilder.Length != 0 && memberNameBuilder[0] == '.')
                memberNameBuilder.Remove(0, 1);
            return memberNameBuilder.ToString();
        }

        internal static T[] InsertFirstArg<T>(this T[] args, T firstArg)
        {
            if (args == null || args.Length == 0)
                return new[] { firstArg };
            var objects = new T[args.Length + 1];
            objects[0] = firstArg;
            Array.Copy(args, 0, objects, 1, args.Length);
            return objects;
        }

        internal static void AddMethodObserver(this ObserverBase.IMethodPathObserver observer, object? target, IMemberInfo? lastMember, ref ActionToken unsubscriber, ref IWeakReference? lastValueRef)
        {
            unsubscriber.Dispose();
            if (target == null || !(lastMember is IMemberAccessorInfo propertyInfo))
            {
                unsubscriber = ActionToken.NoDoToken;
                return;
            }

            var value = propertyInfo.GetValue(target);
            if (ReferenceEquals(value, lastValueRef?.Target))
                return;

            var type = value?.GetType()!;
            if (value.IsNullOrUnsetValue() || type.IsValueType)
            {
                unsubscriber = ActionToken.NoDoToken;
                return;
            }

            lastValueRef = value.ToWeakReference();
            var memberFlags = observer.MemberFlags & ~MemberFlags.Static;
            var member = MugenBindingService.MemberProvider.GetMember(type!, observer.Method, MemberType.Method, memberFlags);
            if (member is IObservableMemberInfo observable)
                unsubscriber = observable.TryObserve(target, observer.GetMethodListener());
            if (unsubscriber.IsEmpty)
                unsubscriber = ActionToken.NoDoToken;
        }

        private static string RemoveBounds(this string st, int start = 1) //todo Span?
        {
            return st.Substring(start, st.Length - start - 1);
        }

        private static void ToStringValue(this IExpressionNode expression, StringBuilder builder)
        {
            var constantExpressionNode = (IConstantExpressionNode)expression;
            var value = constantExpressionNode.Value;

            if (value == null)
            {
                builder.Insert(0, "null");
                return;
            }

            if (value is string st)
            {
                builder.Insert(0, '"');
                builder.Insert(0, st);
                builder.Insert(0, '"');
                return;
            }

            builder.Insert(0, value);
        }

        #endregion
    }
}