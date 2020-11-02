using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Compiling;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Core.Components;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Compiling;
using MugenMvvm.Bindings.Interfaces.Convert;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Interfaces.Observation.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Interfaces.Resources;
using MugenMvvm.Bindings.Metadata;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Bindings.Observation.Observers;
using MugenMvvm.Bindings.Parsing.Visitors;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Extensions
{
    public static partial class MugenBindingExtensions
    {
        #region Fields

        internal const char CommaChar = ',';
        internal const char DotChar = '.';

        public static readonly char[] CommaSeparator = {CommaChar};
        public static readonly char[] DotSeparator = {DotChar};
        private static readonly int[] ArraySize = new int[1];

        #endregion

        #region Methods

        public static IMemberPath GetMemberPath(this IObservationManager observationManager, object path, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(observationManager, nameof(observationManager));
            var result = observationManager.TryGetMemberPath(path, metadata);
            if (result == null)
                ExceptionManager.ThrowRequestNotSupported<IMemberPathProviderComponent>(observationManager, path, metadata);
            return result;
        }

        public static IMemberPathObserver GetMemberPathObserver(this IObservationManager observationManager, object target, object request, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(observationManager, nameof(observationManager));
            var result = observationManager.TryGetMemberPathObserver(target, request, metadata);
            if (result == null)
                ExceptionManager.ThrowRequestNotSupported<IMemberPathObserverProviderComponent>(observationManager, request, metadata);
            return result;
        }

        public static ItemOrList<IBindingBuilder, IReadOnlyList<IBindingBuilder>> ParseBindingExpression(this IBindingManager bindingManager, object expression, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(bindingManager, nameof(bindingManager));
            var result = bindingManager.TryParseBindingExpression(expression, metadata);
            if (result.IsEmpty)
                BindingExceptionManager.ThrowCannotParseExpression(expression);
            return result;
        }

        public static object? Convert(this IGlobalValueConverter converter, object? value, Type targetType, object? member = null, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(converter, nameof(converter));
            if (!converter.TryConvert(ref value, targetType, member, metadata))
                BindingExceptionManager.ThrowCannotConvertType(value, targetType);
            return value;
        }

        public static ICompiledExpression Compile(this IExpressionCompiler compiler, IExpressionNode expression, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(compiler, nameof(compiler));
            var result = compiler.TryCompile(expression, metadata);
            if (result == null)
                BindingExceptionManager.ThrowCannotCompileExpression(expression);
            return result;
        }

        public static Expression Build(this IExpressionBuilderContext context, IExpressionNode expression)
        {
            Should.NotBeNull(context, nameof(context));
            Should.NotBeNull(expression, nameof(expression));
            var exp = context.TryBuild(expression);
            if (exp != null)
                return exp;

            context.ThrowCannotCompile(expression);
            return null;
        }

        public static void ClearBindings(object? target, bool clearAttachedValues)
        {
            if (target == null)
                return;
            foreach (var binding in MugenBindingService.BindingManager.GetBindings(target))
                binding.Dispose();
            if (clearAttachedValues)
                target.AttachedValues().Clear();
        }

        public static Type[] GetTypes(this IResourceResolver? resourceResolver, IReadOnlyList<string>? types, IReadOnlyMetadataContext? metadata = null)
        {
            if (types == null || types.Count == 0)
                return Default.Array<Type>();
            resourceResolver = resourceResolver.DefaultIfNull();
            var typeArgs = new Type[types.Count];
            for (var i = 0; i < types.Count; i++)
            {
                var type = resourceResolver.TryGetType(types[i], null, metadata);
                if (type == null)
                    BindingExceptionManager.ThrowCannotResolveType(types[i]);
                typeArgs[i] = type;
            }

            return typeArgs;
        }

        public static object?[]? TryGetInvokeArgs<TState>(this IGlobalValueConverter? converter, IReadOnlyList<IParameterInfo> parameters, TState state, int argsLength,
            Func<TState, int, IParameterInfo, object?> getValue, object?[]? arguments, out ArgumentFlags flags)
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
                            result[j] = Array.CreateInstance(parameter.ParameterType.GetElementType()!, ArraySize);
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
                    var array = Array.CreateInstance(parameterInfo.ParameterType.GetElementType()!, ArraySize);
                    for (var j = i; j < argsLength; j++)
                    {
                        ArraySize[0] = j - i;
                        array.SetValue(getValue(state, j, parameterInfo), ArraySize);
                    }

                    result[i] = array;
                }
                else
                {
                    converter ??= MugenBindingService.GlobalValueConverter;
                    result[i] = converter.Convert(value, parameterInfo.ParameterType, parameterInfo);
                }
            }

            return result;
        }

        public static object?[]? TryGetInvokeArgs(this IGlobalValueConverter? converter, IReadOnlyList<IParameterInfo> parameters, object?[] args, IReadOnlyMetadataContext? metadata)
        {
            args = converter.TryGetInvokeArgs(parameters, args, args.Length, (objects, i, _) => objects[i], args, out var flags)!;
            if (args != null && flags.HasFlagEx(ArgumentFlags.Metadata))
                args[args.Length - 1] = metadata;
            return args;
        }

        public static object?[]? TryGetInvokeArgs(this IGlobalValueConverter? converter, IReadOnlyList<IParameterInfo> parameters, string[] args, IReadOnlyMetadataContext? metadata, out ArgumentFlags flags)
        {
            try
            {
                return converter.TryGetInvokeArgs(parameters, (args, converter.DefaultIfNull(), metadata), args.Length,
                    ((string[] args, IGlobalValueConverter globalValueConverter, IReadOnlyMetadataContext? metadata) tuple, int i, IParameterInfo parameter) =>
                    {
                        var targetType = parameter.IsParamArray() ? parameter.ParameterType.GetElementType()! : parameter.ParameterType;
                        return tuple.globalValueConverter.Convert(tuple.args[i], targetType, parameter, tuple.metadata);
                    }, null, out flags);
            }
            catch
            {
                flags = 0;
                return null;
            }
        }

        public static BindingParameterExpression TryGetParameterExpression(this IBindingExpressionInitializerContext context, IExpressionCompiler? compiler, BindingMemberExpressionVisitor memberExpressionVisitor,
            BindingMemberExpressionCollectorVisitor memberExpressionCollectorVisitor, string parameterName, IReadOnlyMetadataContext? metadata)
        {
            var expression = context.TryGetParameterValue<IExpressionNode>(parameterName);
            if (expression == null)
                return default;

            expression = memberExpressionVisitor.Visit(expression, false, metadata);
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

        [Preserve(Conditional = true)]
        public static void Raise<TArg>(this EventListenerCollection collection, object? sender, TArg args) => collection.Raise(sender, args, null);

        [Preserve(Conditional = true)]
        public static void RaisePropertyChanged(this MemberListenerCollection collection, object? sender, PropertyChangedEventArgs args) => collection.Raise(sender, args, args.PropertyName, null);

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

                values = ItemOrList.FromList(expressionValues);
            }
            else
            {
                values = ItemOrList.FromItem<ParameterValue, ParameterValue[]>(GetParameterValue(sourceRaw, metadata), true);
                if (values.Item.Value.IsUnsetValueOrDoNothing())
                    return values.Item.Value;
            }

            return expression.Invoke(values, metadata);
        }

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
                for (var i = 0; i < sources.Length; i++)
                    (sources[i] as IMemberPathObserver)?.Dispose();
            }
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

        public static bool IsAllMembersAvailable(this IMemberPathObserver observer) => observer.GetLastMember().IsAvailable;

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
                    var member = memberManager.TryGetMember(type, MemberType.Accessor, flags, path.Members[i], metadata);
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

            return memberManager.TryGetMember(type, lastMemberType, flags, lastMemberName, metadata);
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

        public static WeakEventListener ToWeak(this IEventListener listener) => new WeakEventListener(listener);

        public static WeakEventListener<TState> ToWeak<TState>(this IEventListener listener, TState state) => new WeakEventListener<TState>(listener, state);

        public static string[]? GetIndexerArgsRaw(string path)
        {
            var start = 1;
            if (path.StartsWith("Item[", StringComparison.Ordinal))
                start = 5;
            else if (!path.StartsWith("[", StringComparison.Ordinal) || !path.EndsWith("]", StringComparison.Ordinal))
                return null;

#if SPAN_API
            return path
                .AsSpan()
                .RemoveBounds(start)
                .UnescapeString(CommaChar);
#else
            return path
                .RemoveBounds(start)
                .Split(CommaSeparator, StringSplitOptions.RemoveEmptyEntries)
                .UnescapeString();
#endif
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
#if SPAN_API
            return path
                .AsSpan()
                .RemoveBounds(startIndex + 1)
                .UnescapeString(CommaChar);
#else
            return path
                .RemoveBounds(startIndex + 1)
                .Split(CommaSeparator, StringSplitOptions.RemoveEmptyEntries)
                .UnescapeString();
#endif
        }

        public static Type GetTargetType<T>(this MemberFlags flags, ref T? target) where T : class => GetTargetType(flags.HasFlagEx(MemberFlags.Static), ref target);

        public static Type GetTargetType<T>(bool isStatic, ref T? target) where T : class
        {
            if (isStatic)
            {
                Should.NotBeNull(target, nameof(target));
                var t = target;
                target = null;
                return t as Type ?? t.GetType();
            }

            return target?.GetType() ?? typeof(object);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFlagEx(this MemberFlags value, MemberFlags flag) => (value & flag) == flag;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFlagEx(this ArgumentFlags value, ArgumentFlags flag) => (value & flag) == flag;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFlagEx(this BindingMemberExpressionFlags value, BindingMemberExpressionFlags flag) => (value & flag) == flag;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFlagEx(this MemberType value, MemberType flag) => (value & flag) == flag;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static BindingMemberExpressionFlags SetTargetFlags(this BindingMemberExpressionFlags flags, bool isTarget) =>
            isTarget ? flags | BindingMemberExpressionFlags.Target : flags & ~BindingMemberExpressionFlags.Target;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static MemberFlags SetInstanceOrStaticFlags(this MemberFlags value, bool isStatic) =>
            isStatic ? (value | MemberFlags.Static) & ~MemberFlags.Instance : (value | MemberFlags.Instance) & ~MemberFlags.Static;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static MemberFlags ClearInstanceOrStaticFlags(this MemberFlags value, bool isStatic) => isStatic ? value & ~MemberFlags.Instance : value & ~MemberFlags.Static;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsNullOrUnsetValue([NotNullWhen(false)] this object? value) => value == null || value == BindingMetadata.UnsetValue;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsUnsetValueOrDoNothing(this object? value) => value == BindingMetadata.UnsetValue || value == BindingMetadata.DoNothing;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsDoNothing(this object? value) => value == BindingMetadata.DoNothing;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsUnsetValue(this object? value) => value == BindingMetadata.UnsetValue;

        internal static string GetPath(this StringBuilder memberNameBuilder)
        {
            if (memberNameBuilder.Length != 0 && memberNameBuilder[0] == '.')
                memberNameBuilder.Remove(0, 1);
            return memberNameBuilder.ToString();
        }

        internal static T[] InsertFirstArg<T>(this T[]? args, T firstArg)
        {
            if (args == null || args.Length == 0)
                return new[] {firstArg};
            var objects = new T[args.Length + 1];
            objects[0] = firstArg;
            Array.Copy(args, 0, objects, 1, args.Length);
            return objects;
        }

        internal static void AddMethodObserver(this ObserverBase.IMethodPathObserver observer, object? target, IMemberInfo? lastMember, IReadOnlyMetadataContext? metadata, ref ActionToken unsubscriber,
            ref IWeakReference? lastValueRef)
        {
            unsubscriber.Dispose();
            if (target == null || !(lastMember is IAccessorMemberInfo propertyInfo))
            {
                unsubscriber = ActionToken.NoDoToken;
                return;
            }

            var value = propertyInfo.GetValue(target, metadata);
            if (value == lastValueRef?.Target)
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
            var member = MugenBindingService.MemberManager.TryGetMember(type, MemberType.Method, observer.MemberFlags.SetInstanceOrStaticFlags(false), observer.Method, metadata);
            if (member is IObservableMemberInfo observable)
                unsubscriber = observable.TryObserve(value, observer.GetMethodListener(), metadata);
            if (unsubscriber.IsEmpty)
                unsubscriber = ActionToken.NoDoToken;
        }

        internal static void EventHandlerWeakCanExecuteHandler(this IWeakReference weakReference, object? sender, EventArgs? args) => ((EventHandlerBindingComponent?) weakReference.Target)?.OnCanExecuteChanged();

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
            var member = (IAccessorMemberInfo?) memberManager.TryGetMember(type, MemberType.Accessor, flags, path, metadata);
            if (member == null)
                BindingExceptionManager.ThrowInvalidBindingMember(type, path);
            return member.GetValue(target, metadata);
        }

#if SPAN_API
        private static ReadOnlySpan<char> RemoveBounds(this ReadOnlySpan<char> st, int start = 1) => st.Slice(start, st.Length - start - 1);

        private static string[] UnescapeString(this ReadOnlySpan<char> source, char separator)
        {
            var length = 1;
            for (var i = 0; i < source.Length; i++)
            {
                if (source[i] == separator)
                    ++length;
            }

            if (length == 0)
                return Array.Empty<string>();

            var args = new string[length];
            var index = 0;
            foreach (var arg in source.Split(separator))
            {
                var value = source[arg].Trim();
                if (value.StartsWith("\"", StringComparison.Ordinal) && value.EndsWith("\"", StringComparison.Ordinal)
                    || value.StartsWith("'", StringComparison.Ordinal) && value.EndsWith("'", StringComparison.Ordinal))
                    value = value.RemoveBounds();
                args[index++] = value.ToString();
            }

            return args;
        }
#else
        private static string RemoveBounds(this string st, int start = 1) => st.Substring(start, st.Length - start - 1);

        private static string[] UnescapeString(this string[] args)
        {
            for (var i = 0; i < args.Length; i++)
            {
                var value = args[i].Trim();
                if (value.StartsWith("\"", StringComparison.Ordinal) && value.EndsWith("\"", StringComparison.Ordinal)
                    || value.StartsWith("'", StringComparison.Ordinal) && value.EndsWith("'", StringComparison.Ordinal))
                    value = value.RemoveBounds();
                args[i] = value;
            }

            return args;
        }
#endif

        private static void ToStringValue(this IExpressionNode expression, StringBuilder builder)
        {
            var constantExpressionNode = (IConstantExpressionNode) expression;
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