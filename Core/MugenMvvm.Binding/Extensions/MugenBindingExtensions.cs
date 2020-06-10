using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using MugenMvvm.Binding.Compiling;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Core;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Converters;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Interfaces.Resources;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Members.Descriptors;
using MugenMvvm.Binding.Metadata;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Binding.Observers.PathObservers;
using MugenMvvm.Binding.Parsing.Visitors;
using MugenMvvm.Delegates;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Internal;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Extensions
{
    public static partial class MugenBindingExtensions
    {
        #region Fields

        public static readonly char[] CommaSeparator = { ',' };
        public static readonly char[] DotSeparator = { '.' };
        private static readonly int[] ArraySize = new int[1];

        #endregion

        #region Methods

        public static object? ToBindingSource(object? sourceExpression, object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            if (sourceExpression is IBindingMemberExpressionNode v)
                return v.GetBindingSource(target, source, metadata);

            if (sourceExpression is IBindingMemberExpressionNode[] nodes)
            {
                var observers = new object?[nodes.Length];
                for (var i = 0; i < nodes.Length; i++)
                    observers[i] = nodes[i].GetBindingSource(target, source, metadata);
                return observers;
            }

            return sourceExpression;
        }

        public static void DisposeBindingSource(object? source)
        {
            if (source is IMemberPathObserver disposable)
                disposable.Dispose();
            else if (source is object[] sources)
            {
                for (int i = 0; i < sources.Length; i++)
                    (sources[i] as IMemberPathObserver)?.Dispose();
            }
        }

        public static BindingParameterExpression TryGetParameterExpression(this IBindingExpressionInitializerContext context, IExpressionCompiler? compiler, BindingMemberExpressionVisitor memberExpressionVisitor,
            BindingMemberExpressionCollectorVisitor memberExpressionCollectorVisitor, string parameterName, IReadOnlyMetadataContext? metadata)
        {
            var expression = context.TryGetParameterValue<IExpressionNode>(parameterName);
            if (expression == null)
                return default;

            expression = memberExpressionVisitor.Visit(expression, metadata);
            if (expression is IConstantExpressionNode constant)
                return new BindingParameterExpression(constant.Value, null);

            if (expression is IBindingMemberExpressionNode)
                return new BindingParameterExpression(expression, null);

            var collect = memberExpressionCollectorVisitor.Collect(expression, metadata);
            var compiledExpression = compiler.DefaultIfNull().Compile(expression, metadata);
            if (collect.Item == null && collect.List == null)
                return new BindingParameterExpression(compiledExpression.Invoke(default, metadata), null);
            return new BindingParameterExpression(collect.GetRawValue(), compiledExpression);
        }

        public static void ApplyFlags(this IBindingExpressionInitializerContext context, BindingMemberExpressionVisitor memberExpressionVisitor, string parameterName, BindingMemberExpressionFlags flag)
        {
            Should.NotBeNull(context, nameof(context));
            Should.NotBeNull(memberExpressionVisitor, nameof(memberExpressionVisitor));
            var b = context.TryGetParameterValue<bool?>(parameterName);
            if (b.GetValueOrDefault())
                memberExpressionVisitor.Flags |= flag;
        }

        public static IMemberInfo? GetMember<TRequest>(this IMemberManager memberManager, Type type, MemberType memberTypes, MemberFlags flags, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(memberManager, nameof(memberManager));
            return memberManager.GetMembers(type, memberTypes, flags, request, metadata).FirstOrDefault();
        }

        [DoesNotReturn]
        public static void ThrowCannotCompile(this IExpressionBuilderContext context, IExpressionNode expression)
        {
            var errors = context.TryGetErrors();
            if (errors != null && errors.Count != 0)
            {
                errors.Reverse();
                BindingExceptionManager.ThrowCannotCompileExpression(expression, BindingMessageConstant.PossibleReasons + string.Join(Environment.NewLine, errors));
            }
            else
                BindingExceptionManager.ThrowCannotCompileExpression(expression);
        }

        public static List<string>? TryGetErrors(this IExpressionBuilderContext context)
        {
            Should.NotBeNull(context, nameof(context));
            if (context.GetMetadataOrDefault().TryGet(CompilingMetadata.CompilingErrors, out var errors))
                return errors;
            return null;
        }

        public static object? Invoke(this ICompiledExpression? expression, object? sourceRaw, IReadOnlyMetadataContext? metadata)
        {
            if (expression == null)
                return BindingMetadata.UnsetValue;
            ItemOrList<ParameterValue, ParameterValue[]> values;
            if (sourceRaw == null)
                values = default;
            else if (sourceRaw is object?[] sources)
            {
                var expressionValues = new ParameterValue[sources.Length];
                for (var i = 0; i < sources.Length; i++)
                {
                    var expressionValue = GetParameterValue(sources[i], metadata);
                    if (expressionValue.Value.IsUnsetValueOrDoNothing())
                        return expressionValue.Value;
                    expressionValues[i] = expressionValue;
                }

                values = expressionValues;
            }
            else
            {
                values = GetParameterValue(sourceRaw, metadata);
                if (values.Item.Value.IsUnsetValueOrDoNothing())
                    return values.Item.Value;
            }
            return expression.Invoke(values, metadata);
        }

        public static object? GetParent(object? target, IReadOnlyMetadataContext? metadata = null, IMemberManager? provider = null)
        {
            return target?.GetBindableMemberValue(BindableMembers.Object.Parent, null, MemberFlags.All, metadata, provider);
        }

        public static object? FindElementSource(object target, string elementName, IReadOnlyMetadataContext? metadata = null, IMemberManager? provider = null)
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

        public static object? FindRelativeSource(object target, string typeName, int level, IReadOnlyMetadataContext? metadata = null, IMemberManager? provider = null)
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

        public static bool IsAllMembersAvailable(ItemOrList<object?, object?[]> observers)
        {
            var list = observers.List;
            if (list == null)
                return !(observers.Item is IMemberPathObserver observer) || observer.IsAllMembersAvailable();

            for (var i = 0; i < list.Length; i++)
            {
                if (list[i] is IMemberPathObserver observer && !observer.IsAllMembersAvailable())
                    return false;
            }

            return true;
        }

        public static bool IsAllMembersAvailable(this IMemberPathObserver observer)
        {
            return observer.GetLastMember().IsAvailable;
        }

        public static Type[] GetTypes(this IResourceResolver? resourceResolver, IReadOnlyList<string>? types, IReadOnlyMetadataContext? metadata = null)
        {
            if (types == null || types.Count == 0)
                return Default.Array<Type>();
            resourceResolver = resourceResolver.DefaultIfNull();
            var typeArgs = new Type[types.Count];
            for (var i = 0; i < types.Count; i++)
            {
                var type = resourceResolver.TryGetType<object?>(types[i], null, metadata);
                if (type == null)
                    BindingExceptionManager.ThrowCannotResolveType(types[i]);
                typeArgs[i] = type;
            }

            return typeArgs;
        }

        public static object? GetValueFromPath(this IMemberPath path, Type type, object? target, MemberFlags flags,
            int firstMemberIndex = 0, IReadOnlyMetadataContext? metadata = null, IMemberManager? memberManager = null)
        {
            Should.NotBeNull(type, nameof(type));
            memberManager = memberManager.DefaultIfNull();
            if (path.Members.Count == 0)
            {
                if (firstMemberIndex > 0)
                    ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(firstMemberIndex));

                if (flags.HasFlagEx(MemberFlags.Static))
                    return type;
                return target;
            }

            if (firstMemberIndex > path.Members.Count)
                ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(firstMemberIndex));

            for (var index = firstMemberIndex; index < path.Members.Count; index++)
            {
                var pathMember = path.Members[index];
                if (index == 1)
                    flags = flags.SetInstanceOrStaticFlags(false);
                target = memberManager.GetValue(type, target, pathMember, flags, metadata);
                if (target.IsNullOrUnsetValue())
                    return target;
                type = target.GetType();
            }

            return target;
        }

        public static IMemberInfo? GetLastMemberFromPath(this IMemberPath path, Type type, object? target, MemberFlags flags,
            MemberType lastMemberType, IReadOnlyMetadataContext? metadata = null, IMemberManager? memberManager = null)
        {
            Should.NotBeNull(type, nameof(type));
            memberManager = memberManager.DefaultIfNull();
            string lastMemberName;
            if (path.Members.Count == 0)
                lastMemberName = path.Path;
            else
            {
                for (var i = 0; i < path.Members.Count - 1; i++)
                {
                    if (i == 1)
                        flags = flags.SetInstanceOrStaticFlags(false);
                    var member = memberManager.GetMember(type, MemberType.Accessor, flags, path.Members[i], metadata);
                    if (!(member is IAccessorMemberInfo accessor) || !accessor.CanRead)
                        return null;

                    target = accessor.GetValue(target, metadata);
                    if (target.IsNullOrUnsetValue())
                        return null;
                    type = target.GetType();
                }

                flags = flags.SetInstanceOrStaticFlags(false);
                lastMemberName = path.Members[path.Members.Count - 1];
            }

            return memberManager.GetMember(type, lastMemberType, flags, lastMemberName, metadata);
        }

        public static bool TryBuildBindingMemberPath(this IExpressionNode? target, StringBuilder builder, out IExpressionNode? firstExpression)
        {
            return TryBuildBindingMemberPath(target, builder, null, out firstExpression);
        }

        public static bool TryBuildBindingMemberPath(this IExpressionNode? target, StringBuilder builder, Func<IExpressionNode, bool>? condition, out IExpressionNode? firstExpression)
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

                switch (target)
                {
                    case IMemberExpressionNode memberExpressionNode:
                        {
                            var memberName = memberExpressionNode.Member.Trim();
                            builder.Insert(0, memberName);
                            if (memberExpressionNode.Target != null)
                                builder.Insert(0, '.');
                            target = memberExpressionNode.Target;
                            break;
                        }
                    case IIndexExpressionNode indexExpressionNode when indexExpressionNode.Arguments.All(arg => arg.ExpressionType == ExpressionNodeType.Constant):
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
                            break;
                        }
                    case IMethodCallExpressionNode methodCallExpression when methodCallExpression.Arguments.All(arg => arg.ExpressionType == ExpressionNodeType.Constant):
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
                            builder.Insert(0, methodCallExpression.Method);
                            builder.Insert(0, '.');
                            target = methodCallExpression.Target;
                            break;
                        }
                    default:
                        return false;
                }
            }

            return true;
        }

        public static IExpressionNode? TryGetRootMemberExpression(this IExpressionNode? target, Func<IExpressionNode, bool>? condition = null)
        {
            if (target == null)
                return null;
            IExpressionNode? result = null;
            while (target != null)
            {
                result = target;
                if (condition != null && !condition(target))
                    return null;

                switch (target)
                {
                    case IMemberExpressionNode memberExpressionNode:
                        target = memberExpressionNode.Target;
                        break;
                    case IIndexExpressionNode indexExpressionNode when indexExpressionNode.Arguments.All(arg => arg.ExpressionType == ExpressionNodeType.Constant):
                        target = indexExpressionNode.Target;
                        break;
                    case IMethodCallExpressionNode methodCallExpression when methodCallExpression.Arguments.All(arg => arg.ExpressionType == ExpressionNodeType.Constant):
                        target = methodCallExpression.Target;
                        break;
                    default:
                        return null;
                }
            }

            return result;
        }

        [return: MaybeNull]
        public static TValue GetBindableMemberValue<TTarget, TValue>(this TTarget target,
            BindablePropertyDescriptor<TTarget, TValue> bindableMember, TValue defaultValue = default, MemberFlags flags = MemberFlags.All,
            IReadOnlyMetadataContext? metadata = null, IMemberManager? provider = null) where TTarget : class
        {
            var propertyInfo = provider
                .DefaultIfNull()
                .GetMember(target.GetType(), MemberType.Accessor, flags, bindableMember.Name, metadata) as IAccessorMemberInfo;
            if (propertyInfo == null)
                return defaultValue;
            return (TValue)propertyInfo.GetValue(target, metadata)!;
        }

        public static void SetBindableMemberValue<TTarget, TValue>(this TTarget target,
            BindablePropertyDescriptor<TTarget, TValue> bindableMember, [MaybeNull] TValue value, bool throwOnError = true, MemberFlags flags = MemberFlags.All,
            IReadOnlyMetadataContext? metadata = null, IMemberManager? provider = null) where TTarget : class
        {
            var propertyInfo = provider
                .DefaultIfNull()
                .GetMember(target.GetType(), MemberType.Accessor, flags, bindableMember.Name, metadata) as IAccessorMemberInfo;
            if (propertyInfo == null)
            {
                if (throwOnError)
                    BindingExceptionManager.ThrowInvalidBindingMember(target.GetType(), bindableMember.Name);
            }
            else
                propertyInfo.SetValue(target, BoxingExtensions.Box(value), metadata);
        }

        public static ActionToken TryObserveBindableMember<TTarget, TValue>(this TTarget target,
            BindablePropertyDescriptor<TTarget, TValue> bindableMember, IEventListener listener, MemberFlags flags = MemberFlags.All,
            IReadOnlyMetadataContext? metadata = null, IMemberManager? provider = null) where TTarget : class
        {
            var member = provider
                .DefaultIfNull()
                .GetMember(target.GetType(), MemberType.Accessor, flags, bindableMember.Name, metadata) as IObservableMemberInfo;
            if (member == null)
                return default;
            return member.TryObserve(target, listener, metadata);
        }

        public static ActionToken TrySubscribeBindableEvent<TTarget>(this TTarget target,
            BindableEventDescriptor<TTarget> eventMember, IEventListener listener, MemberFlags flags = MemberFlags.All,
            IReadOnlyMetadataContext? metadata = null, IMemberManager? provider = null) where TTarget : class
        {
            var eventInfo = provider
                .DefaultIfNull()
                .GetMember(target.GetType(), MemberType.Event, flags, eventMember.Name, metadata) as IObservableMemberInfo;
            if (eventInfo == null)
                return default;
            return eventInfo.TryObserve(target, listener, metadata);
        }

        public static object? TryInvokeBindableMethod<TTarget>(this TTarget target,
            BindableMethodDescriptor<TTarget> methodMember, object?[]? args = null, MemberFlags flags = MemberFlags.All,
            IReadOnlyMetadataContext? metadata = null, IMemberManager? provider = null) where TTarget : class
        {
            var methodInfo = provider
                .DefaultIfNull()
                .GetMember(target.GetType(), MemberType.Method, flags, methodMember.Name, metadata) as IMethodInfo;
            return methodInfo?.Invoke(target, args ?? Default.Array<object>());
        }

        public static WeakEventListener ToWeak(this IEventListener listener)
        {
            return new WeakEventListener(listener);
        }

        public static WeakEventListener<TState> ToWeak<TState>(this IEventListener listener, in TState state)
        {
            return new WeakEventListener<TState>(listener, state);
        }

        public static object?[]? TryGetInvokeArgs<TState>(this IGlobalValueConverter? converter, IReadOnlyList<IParameterInfo> parameters, in TState state, int argsLength,
            FuncIn<TState, int, IParameterInfo, object?> getValue, object?[]? arguments, out ArgumentFlags flags)
        {
            flags = 0;
            var hasParams = parameters.LastOrDefault()?.IsParamArray() ?? false;
            object?[] result;
            if (arguments != null && argsLength == parameters.Count)
                result = arguments;
            else
                result = new object?[parameters.Count];
            for (var i = 0; i < parameters.Count; i++)
            {
                //optional or params
                if (i > argsLength - 1)
                {
                    for (var j = i; j < parameters.Count; j++)
                    {
                        var parameter = parameters[j];
                        if (j == parameters.Count - 1 && hasParams)
                        {
                            ArraySize[0] = 0;
                            result[j] = Array.CreateInstance(parameter.ParameterType.GetElementType(), ArraySize);
                            flags |= ArgumentFlags.EmptyParamArray;
                        }
                        else
                        {
                            if (parameter.ParameterType == typeof(IReadOnlyMetadataContext))
                                flags |= ArgumentFlags.Metadata;
                            else
                            {
                                if (!parameter.HasDefaultValue)
                                    return null;
                                flags |= ArgumentFlags.Optional;
                                result[j] = parameter.DefaultValue;
                            }
                        }
                    }

                    break;
                }

                var parameterInfo = parameters[i];
                var value = getValue(state, i, parameterInfo);
                if (i == parameters.Count - 1 && hasParams && !parameterInfo.ParameterType.IsInstanceOfType(value))
                {
                    flags |= ArgumentFlags.ParamArray;
                    ArraySize[0] = argsLength - i;
                    var array = Array.CreateInstance(parameterInfo.ParameterType.GetElementType(), ArraySize);
                    for (var j = i; j < argsLength; j++)
                    {
                        ArraySize[0] = j - i;
                        array.SetValue(getValue(state, j, parameterInfo), ArraySize);
                    }

                    result[i] = array;
                }
                else
                {
                    if (converter == null)
                        converter = MugenBindingService.GlobalValueConverter;
                    result[i] = converter.Convert(value, parameterInfo.ParameterType, parameterInfo);
                }
            }

            return result;
        }

        public static object?[]? TryGetInvokeArgs(this IGlobalValueConverter? converter, IReadOnlyList<IParameterInfo> parameters, object?[] args, IReadOnlyMetadataContext? metadata)
        {
            args = converter.TryGetInvokeArgs(parameters, args, args.Length, (in object?[] objects, int i, IParameterInfo _) => objects[i], args, out var flags)!;
            if (args != null && flags.HasFlagEx(ArgumentFlags.Metadata))
                args[args.Length - 1] = metadata;
            return args;
        }

        public static object?[]? TryGetInvokeArgs(this IGlobalValueConverter? converter, IReadOnlyList<IParameterInfo> parameters, string[] args, IReadOnlyMetadataContext? metadata, out ArgumentFlags flags)
        {
            try
            {
                return converter.TryGetInvokeArgs(parameters, (args, converter.DefaultIfNull(), metadata), args.Length,
                    (in (string[] args, IGlobalValueConverter globalValueConverter, IReadOnlyMetadataContext? metadata) tuple, int i, IParameterInfo parameter) =>
                    {
                        var targetType = parameter.IsParamArray() ? parameter.ParameterType.GetElementType() : parameter.ParameterType;
                        return tuple.globalValueConverter.Convert(tuple.args[i], targetType, parameter, tuple.metadata);
                    }, null, out flags);
            }
            catch
            {
                flags = 0;
                return null;
            }
        }

        public static object? Convert(this IGlobalValueConverter? converter, string? value, Type targetType, object? member = null, IReadOnlyMetadataContext? metadata = null)
        {
            if (!string.IsNullOrEmpty(value) && value![0] == '\"' && value.EndsWith("\""))
                value = value.RemoveBounds();
            return value == "null" ? null : converter.DefaultIfNull().Convert(value, targetType, member, metadata);
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
                .Split(CommaSeparator, StringSplitOptions.RemoveEmptyEntries)
                .UnescapeString();
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
                .Split(CommaSeparator, StringSplitOptions.RemoveEmptyEntries)
                .UnescapeString();
        }

        public static Type GetTargetType(this MemberFlags flags, ref object? target)
        {
            if (flags.HasFlagEx(MemberFlags.Static))
            {
                var t = target as Type ?? target!.GetType();
                target = null;
                return t;
            }
            return target!.GetType();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFlagEx(this MemberFlags value, MemberFlags flag)
        {
            return (value & flag) == flag;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFlagEx(this ArgumentFlags value, ArgumentFlags flag)
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
        internal static MemberFlags SetInstanceOrStaticFlags(this MemberFlags value, bool isStatic)
        {
            return isStatic ? (value | MemberFlags.Static) & ~MemberFlags.Instance : (value | MemberFlags.Instance) & ~MemberFlags.Static;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static MemberFlags ClearInstanceOrStaticFlags(this MemberFlags value, bool isStatic)
        {
            return isStatic ? value & ~MemberFlags.Instance : value & ~MemberFlags.Static;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsNullOrUnsetValue([NotNullWhen(false)]this object? value)
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

        internal static void AddMethodObserver(this ObserverBase.IMethodPathObserver observer, object? target, IMemberInfo? lastMember, IReadOnlyMetadataContext? metadata, ref ActionToken unsubscriber, ref IWeakReference? lastValueRef)
        {
            unsubscriber.Dispose();
            if (target == null || !(lastMember is IAccessorMemberInfo propertyInfo))
            {
                unsubscriber = ActionToken.NoDoToken;
                return;
            }

            var value = propertyInfo.GetValue(target, metadata);
            if (ReferenceEquals(value, lastValueRef?.Target))
                return;

            if (value.IsNullOrUnsetValue())
            {
                unsubscriber = ActionToken.NoDoToken;
                return;
            }

            var type = value.GetType();
            if (type.IsValueType)
            {
                unsubscriber = ActionToken.NoDoToken;
                return;
            }

            lastValueRef = value.ToWeakReference();
            var member = MugenBindingService.MemberManager.GetMember(type, MemberType.Method, observer.MemberFlags.SetInstanceOrStaticFlags(false), observer.Method, metadata);
            if (member is IObservableMemberInfo observable)
                unsubscriber = observable.TryObserve(value, observer.GetMethodListener(), metadata);
            if (unsubscriber.IsEmpty)
                unsubscriber = ActionToken.NoDoToken;
        }

        private static ParameterValue GetParameterValue(object? sourceRaw, IReadOnlyMetadataContext? metadata)
        {
            if (sourceRaw == null)
                return new ParameterValue(typeof(object), null);
            if (sourceRaw is IMemberPathObserver observer)
            {
                var members = observer.GetLastMember(metadata);
                var value = members.GetValueOrThrow(metadata);
                if (value.IsUnsetValueOrDoNothing())
                    return new ParameterValue(typeof(object), value);
                return new ParameterValue(value?.GetType() ?? members.Member.Type, value);
            }

            return new ParameterValue(sourceRaw.GetType(), sourceRaw);
        }

        private static object? GetValue(this IMemberManager memberManager, Type type, object? target, string path, MemberFlags flags, IReadOnlyMetadataContext? metadata)
        {
            var member = memberManager.GetMember(type, MemberType.Accessor, flags, path, metadata) as IAccessorMemberInfo;
            if (member == null)
                BindingExceptionManager.ThrowInvalidBindingMember(type, path);
            return member.GetValue(target, metadata);
        }

        private static string RemoveBounds(this string st, int start = 1) //todo Span?
        {
            return st.Substring(start, st.Length - start - 1);
        }

        private static string[] UnescapeString(this string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                var value = args[i].Trim();
                if (value.StartsWith("\"", StringComparison.Ordinal) && value.EndsWith("\"", StringComparison.Ordinal)
                    || value.StartsWith("'", StringComparison.Ordinal) && value.EndsWith("'", StringComparison.Ordinal))
                    value = value.RemoveBounds();
                args[i] = value;
            }
            return args;
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