using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Converters;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Members.Descriptors;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

// ReSharper disable once CheckNamespace
namespace MugenMvvm.Binding
{
    public static partial class MugenBindingExtensions
    {
        #region Methods

        public static bool IsAllMembersAvailable(this ItemOrList<IMemberPathObserver?, IMemberPathObserver[]> observers)
        {
            var list = observers.List;
            if (list == null)
            {
                var item = observers.Item;
                return item == null || item.IsAllMembersAvailable();
            }

            for (int i = 0; i < list.Length; i++)
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
            memberProvider = memberProvider.ServiceIfNull();
            for (int index = firstMemberIndex; index < path.Members.Length; index++)
            {
                string item = path.Members[index];
                if (src.IsNullOrUnsetValue())
                    return null;

                var member = memberProvider.GetMember(type, item, MemberType.Field | MemberType.Property, flags) as IMemberAccessorInfo;
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
                            for (int i = args.Count - 2; i >= 0; i--)
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
                            for (int i = args.Count - 2; i >= 0; i--)
                            {
                                builder.Insert(0, ',');
                                args[i].ToStringValue(builder);
                            }
                        }
                        builder.Insert(0, '(');
                        builder.Insert(0, methodCallExpression.MethodName);
                        target = methodCallExpression.Target;
                    }
                    else
                        return false;
                }
            }

            return true;
        }

        public static bool IsMacros(this IUnaryExpressionNode? expression)
        {
            if (expression == null)
                return false;
            return expression.Token == UnaryTokenType.DynamicExpression || expression.Token == UnaryTokenType.StaticExpression;
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

        public static bool TryConvert(this IGlobalValueConverter? converter, object? value, Type targetType, IMemberInfo? member, IReadOnlyMetadataContext? metadata, out object? result)
        {
            try
            {
                result = converter.ServiceIfNull().Convert(value, targetType, member, metadata);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        public static TValue GetBindableMemberValue<TTarget, TValue>(this TTarget target,
            BindablePropertyDescriptor<TTarget, TValue> bindableMember, TValue defaultValue = default, MemberFlags flags = MemberFlags.All,
            IReadOnlyMetadataContext? metadata = null, IMemberProvider? provider = null) where TTarget : class
        {
            var propertyInfo = provider
                .ServiceIfNull()
                .GetMember(target.GetType(), bindableMember.Name, MemberType.Property | MemberType.Field, flags, metadata) as IMemberAccessorInfo;
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
                .ServiceIfNull()
                .GetMember(target.GetType(), bindableMember.Name, MemberType.Property | MemberType.Field, flags, metadata) as IMemberAccessorInfo;
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
                .ServiceIfNull()
                .GetMember(target.GetType(), bindableMember.Name, MemberType.Property | MemberType.Field, flags, metadata) as IObservableMemberInfo;
            if (propertyInfo == null)
                return default;
            return propertyInfo.TryObserve(target, listener, metadata);
        }

        public static ActionToken TrySubscribeBindableEvent<TTarget>(this TTarget target,
            BindableEventDescriptor<TTarget> eventMember, IEventListener listener, MemberFlags flags = MemberFlags.All,
            IReadOnlyMetadataContext? metadata = null, IMemberProvider? provider = null) where TTarget : class
        {
            var eventInfo = provider
                .ServiceIfNull()
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
                .ServiceIfNull()
                .GetMember(target.GetType(), methodMember.Name, MemberType.Method, flags, metadata) as IMethodInfo;
            return methodInfo?.Invoke(target, args ?? Default.EmptyArray<object>());
        }

        public static WeakEventListener ToWeak(this IEventListener listener)
        {
            return new WeakEventListener(listener);
        }

        #endregion
    }
}