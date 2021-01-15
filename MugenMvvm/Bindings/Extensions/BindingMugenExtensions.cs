using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
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
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Interfaces.Resources;
using MugenMvvm.Bindings.Metadata;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Bindings.Observation.Observers;
using MugenMvvm.Bindings.Parsing.Visitors;
using MugenMvvm.Bindings.Resources.Components;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Extensions
{
    public static partial class BindingMugenExtensions
    {
        internal const char CommaChar = ',';
        internal const char DotChar = '.';

#if !SPAN_API
        internal static readonly char[] CommaSeparator = {CommaChar};
#endif
        internal static readonly char[] DotSeparator = {DotChar};
        private static readonly int[] ArraySize = new int[1];

        public static IMemberPath GetMemberPath(this IObservationManager observationManager, object path, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(observationManager, nameof(observationManager));
            var result = observationManager.TryGetMemberPath(path, metadata);
            if (result == null)
                ExceptionManager.ThrowRequestNotSupported<IMemberPathProviderComponent>(observationManager, path, metadata);
            return result;
        }

        public static IMemberPathObserver GetMemberPathObserver(this IObservationManager observationManager, object target, object request,
            IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(observationManager, nameof(observationManager));
            var result = observationManager.TryGetMemberPathObserver(target, request, metadata);
            if (result == null)
                ExceptionManager.ThrowRequestNotSupported<IMemberPathObserverProviderComponent>(observationManager, request, metadata);
            return result;
        }

        public static ItemOrIReadOnlyList<IBindingBuilder> ParseBindingExpression(this IBindingManager bindingManager, object expression, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(bindingManager, nameof(bindingManager));
            var result = bindingManager.TryParseBindingExpression(expression, metadata);
            if (result.IsEmpty)
                ExceptionManager.ThrowCannotParseExpression(expression);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? Convert(this IGlobalValueConverter converter, object? value, Type targetType, object? member = null, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(converter, nameof(converter));
            if (!converter.TryConvert(ref value, targetType, member, metadata))
                ExceptionManager.ThrowCannotConvertType(value, targetType);
            return value;
        }

        public static ICompiledExpression Compile(this IExpressionCompiler compiler, IExpressionNode expression, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(compiler, nameof(compiler));
            var result = compiler.TryCompile(expression, metadata);
            if (result == null)
                ExceptionManager.ThrowCannotCompileExpression(expression);
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

        public static TypeResolver GetTypeResolver(this IResourceManager resourceManager)
        {
            Should.NotBeNull(resourceManager, nameof(resourceManager));
            return resourceManager.GetOrAddComponent(_ => new TypeResolver());
        }

        public static ResourceResolver GetResourceResolver(this IResourceManager resourceManager)
        {
            Should.NotBeNull(resourceManager, nameof(resourceManager));
            return resourceManager.GetOrAddComponent(_ => new ResourceResolver());
        }

        public static void ClearBindings(object? target, bool clearAttachedValues)
        {
            if (target == null)
                return;
            foreach (var binding in MugenService.BindingManager.GetBindings(target))
                binding.Dispose();
            if (clearAttachedValues)
                target.AttachedValues().Clear();
        }

        public static ItemOrArray<Type> GetTypes(this IResourceManager? resourceResolver, ItemOrIReadOnlyList<string> types, IReadOnlyMetadataContext? metadata = null)
        {
            if (types.IsEmpty)
                return default;
            resourceResolver = resourceResolver.DefaultIfNull();
            var typeArgs = ItemOrArray.Get<Type>(types.Count);
            var index = 0;
            foreach (var t in types)
            {
                var type = resourceResolver.TryGetType(t, null, metadata);
                if (type == null)
                    ExceptionManager.ThrowCannotResolveType(t);
                typeArgs.SetAt(index++, type);
            }

            return typeArgs;
        }

        public static ItemOrArray<object?> TryGetInvokeArgs<TState>(this IGlobalValueConverter? converter, ItemOrIReadOnlyList<IParameterInfo> parameters, int parametersCount,
            TState state, int argsLength,
            Func<TState, int, IParameterInfo, object?> getValue, ItemOrArray<object?> arguments, out EnumFlags<ArgumentFlags> flags)
        {
            flags = default;
            bool hasParams;
            if (parameters.Count != 0)
            {
                var parameterInfo = parameters.Last();
                hasParams = parameterInfo.IsParamArray();
            }
            else
                hasParams = false;

            if (argsLength > parametersCount && !hasParams)
                return default;

            ItemOrArray<object?> result;
            if (!arguments.IsEmpty && argsLength == parametersCount)
                result = arguments;
            else
                result = ItemOrArray.Get<object?>(parametersCount);
            for (var i = 0; i < parametersCount; i++)
            {
                //optional or params
                if (i > argsLength - 1)
                {
                    for (var j = i; j < parametersCount; j++)
                    {
                        var parameter = parameters[j];
                        if (j == parametersCount - 1 && hasParams)
                        {
                            result.SetAt(j, Default.Array(parameter.ParameterType.GetElementType()!));
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
                                result.SetAt(j, parameter.DefaultValue);
                            }
                        }
                    }

                    break;
                }

                var parameterInfo = parameters[i];
                var value = getValue(state, i, parameterInfo);
                if (i == parametersCount - 1 && hasParams && !parameterInfo.ParameterType.IsInstanceOfType(value))
                {
                    flags |= ArgumentFlags.ParamArray;
                    lock (ArraySize)
                    {
                        ArraySize[0] = argsLength - i;
                        var array = Array.CreateInstance(parameterInfo.ParameterType.GetElementType()!, ArraySize);
                        for (var j = i; j < argsLength; j++)
                        {
                            ArraySize[0] = j - i;
                            array.SetValue(getValue(state, j, parameterInfo), ArraySize);
                        }

                        result.SetAt(i, array);
                    }
                }
                else
                {
                    converter ??= MugenService.GlobalValueConverter;
                    result.SetAt(i, converter.Convert(value, parameterInfo.ParameterType, parameterInfo));
                }
            }

            if (result.Count == parametersCount)
                return result;
            return default;
        }

        public static ItemOrArray<object?> TryGetInvokeArgs(this IGlobalValueConverter? converter, ItemOrIReadOnlyList<IParameterInfo> parameters, ItemOrArray<object?> args,
            IReadOnlyMetadataContext? metadata)
        {
            args = converter.TryGetInvokeArgs(parameters, parameters.Count, args, args.Count, (objects, i, _) => objects[i], args, out var flags)!;
            if (flags.HasFlag(ArgumentFlags.Metadata))
                args.SetAt(args.Count - 1, metadata);
            return args;
        }

        public static ItemOrArray<object?> TryGetInvokeArgs(this IGlobalValueConverter? converter, ItemOrIReadOnlyList<IParameterInfo> parameters, ItemOrArray<string> args,
            IReadOnlyMetadataContext? metadata,
            out EnumFlags<ArgumentFlags> flags) =>
            converter.TryGetInvokeArgs(parameters, parameters.Count, args, metadata, out flags);

        public static ItemOrArray<object?> TryGetInvokeArgs(this IGlobalValueConverter? converter, ItemOrIReadOnlyList<IParameterInfo> parameters, int parametersCount,
            ItemOrArray<string> args,
            IReadOnlyMetadataContext? metadata, out EnumFlags<ArgumentFlags> flags)
        {
            try
            {
                return converter.TryGetInvokeArgs(parameters, parametersCount, (args, converter.DefaultIfNull(), metadata), args.Count,
                    ((ItemOrArray<string> args, IGlobalValueConverter globalValueConverter, IReadOnlyMetadataContext? metadata) tuple, int i, IParameterInfo parameter) =>
                    {
                        var targetType = parameter.IsParamArray() && parameter.ParameterType.IsArray ? parameter.ParameterType.GetElementType()! : parameter.ParameterType;
                        return tuple.globalValueConverter.Convert(tuple.args[i], targetType, parameter, tuple.metadata);
                    }, default, out flags);
            }
            catch
            {
                flags = default;
                return default;
            }
        }

        public static void VisitParameterExpressions(this IBindingExpressionInitializerContext context, IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(context, nameof(context));
            Should.NotBeNull(visitor, nameof(visitor));
            var parameters = new ItemOrListEditor<IExpressionNode>(context.ParameterExpressions);
            for (var i = 0; i < parameters.Count; i++)
                parameters[i] = parameters[i].Accept(visitor, metadata);
            context.ParameterExpressions = parameters.ToItemOrList();
        }

        public static BindingParameterExpression TryGetParameterExpression(this IBindingExpressionInitializerContext context, IExpressionCompiler? compiler,
            BindingMemberExpressionVisitor memberExpressionVisitor,
            BindingMemberExpressionCollectorVisitor memberExpressionCollectorVisitor, string parameterName, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(context, nameof(context));
            var expression = context.TryGetParameterValue<IExpressionNode>(parameterName);
            if (expression == null)
                return default;

            expression = memberExpressionVisitor.Visit(expression, false, metadata);
            if (expression is IConstantExpressionNode constant)
                return new BindingParameterExpression(constant.Value, null);

            if (expression is IBindingMemberExpressionNode)
                return new BindingParameterExpression(expression, null);

            var collect = memberExpressionCollectorVisitor.Collect(ref expression, metadata);
            var compiledExpression = compiler.DefaultIfNull().Compile(expression, metadata);
            if (collect.IsEmpty)
                return new BindingParameterExpression(compiledExpression.Invoke(default, metadata), null);
            return new BindingParameterExpression(collect.GetRawValue(), compiledExpression);
        }

        public static void ApplyFlags(this IBindingExpressionInitializerContext context, BindingMemberExpressionVisitor memberExpressionVisitor, string parameterName,
            EnumFlags<BindingMemberExpressionFlags> flag)
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
                ExceptionManager.ThrowCannotCompileExpression(expression, BindingMessageConstant.PossibleReasons + string.Join(Environment.NewLine, errors));
            }
            else
                ExceptionManager.ThrowCannotCompileExpression(expression);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<string>? TryGetErrors(this IExpressionBuilderContext context) => context.GetOrDefault(CompilingMetadata.CompilingErrors);

        [Preserve(Conditional = true)]
        public static void Raise<TArg>(this EventListenerCollection collection, object? sender, TArg args) => collection.Raise(sender, args, null);

        [Preserve(Conditional = true)]
        public static void RaisePropertyChanged(this MemberListenerCollection collection, object? sender, PropertyChangedEventArgs args) =>
            collection.Raise(sender, args, args.PropertyName ?? "", null);

        public static object? Invoke(this ICompiledExpression? expression, object? sourceRaw, IReadOnlyMetadataContext? metadata)
        {
            if (expression == null)
                return BindingMetadata.UnsetValue;
            ItemOrArray<ParameterValue> values;
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

        public static bool TryConvert(ref object? value, Type targetType, Func<IFormatProvider>? formatProvider)
        {
            if (value == null)
            {
                value = targetType.GetDefaultValue();
                return true;
            }

            if (targetType.IsInstanceOfType(value))
                return true;
            if (targetType == typeof(string))
            {
                value = formatProvider == null
                    ? value.ToString()
                    : System.Convert.ToString(value, formatProvider());
                return true;
            }

            if (targetType.IsEnum)
            {
                value = Enum.Parse(targetType, value.ToString()!);
                return true;
            }

            if (value is IConvertible)
            {
                value = formatProvider == null
                    ? System.Convert.ChangeType(value, targetType.GetNonNullableType())
                    : System.Convert.ChangeType(value, targetType.GetNonNullableType(), formatProvider());
                return true;
            }

            return false;
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

        public static bool IsAllMembersAvailable(ItemOrArray<object?> observers)
        {
            foreach (var item in observers)
            {
                if (item is IMemberPathObserver observer && !observer.IsAllMembersAvailable())
                    return false;
            }

            return true;
        }

        public static bool IsAllMembersAvailable(this IMemberPathObserver observer) => observer.GetLastMember().IsAvailable;

        public static object? GetValueFromPath(this IMemberPath path, Type type, object? target, EnumFlags<MemberFlags> flags,
            int firstMemberIndex = 0, IReadOnlyMetadataContext? metadata = null, IMemberManager? memberManager = null)
        {
            Should.NotBeNull(type, nameof(type));
            memberManager = memberManager.DefaultIfNull();
            var members = path.Members;
            if (members.Count == 0)
            {
                if (firstMemberIndex > 0)
                    ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(firstMemberIndex));

                if (flags.HasFlag(MemberFlags.Static))
                    return type;
                return target;
            }

            if (firstMemberIndex > members.Count)
                ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(firstMemberIndex));

            for (var index = firstMemberIndex; index < members.Count; index++)
            {
                var pathMember = members[index];
                if (index == 1)
                    flags = flags.SetInstanceOrStaticFlags(false);
                target = memberManager.GetValue(type, target, pathMember, flags, metadata);
                if (target.IsNullOrUnsetValue())
                    return target;
                type = target.GetType();
            }

            return target;
        }

        public static IMemberInfo? GetLastMemberFromPath(this IMemberPath path, Type type, object? target, EnumFlags<MemberFlags> flags,
            MemberType lastMemberType, IReadOnlyMetadataContext? metadata = null, IMemberManager? memberManager = null)
        {
            Should.NotBeNull(type, nameof(type));
            memberManager = memberManager.DefaultIfNull();
            string lastMemberName;
            var members = path.Members;
            if (members.Count == 0)
                lastMemberName = path.Path;
            else
            {
                for (var i = 0; i < members.Count - 1; i++)
                {
                    if (i == 1)
                        flags = flags.SetInstanceOrStaticFlags(false);
                    var member = memberManager.TryGetMember(type, MemberType.Accessor, flags, members[i], metadata);
                    if (member is not IAccessorMemberInfo accessor || !accessor.CanRead)
                        return null;

                    target = accessor.GetValue(target, metadata);
                    if (target.IsNullOrUnsetValue())
                        return null;
                    type = target.GetType();
                }

                flags = flags.SetInstanceOrStaticFlags(false);
                lastMemberName = members[members.Count - 1];
            }

            return memberManager.TryGetMember(type, lastMemberType, flags, lastMemberName, metadata);
        }

        public static bool TryBuildBindingMemberPath(this IExpressionNode? target, StringBuilder builder, Func<IExpressionNode, bool>? condition,
            out IExpressionNode? firstExpression)
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
                    case IIndexExpressionNode indexExpressionNode when indexExpressionNode.Arguments.IsAllConstants():
                    {
                        var args = indexExpressionNode.Arguments;
                        builder.Insert(0, ']');
                        if (args.Count > 0)
                        {
                            args[args.Count - 1].ToStringValue(builder);
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
                    case IMethodCallExpressionNode methodCallExpression when methodCallExpression.Arguments.IsAllConstants():
                    {
                        var args = methodCallExpression.Arguments;
                        builder.Insert(0, ')');
                        if (args.Count > 0)
                        {
                            args[args.Count - 1].ToStringValue(builder);
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
                    case IIndexExpressionNode indexExpressionNode when indexExpressionNode.Arguments.IsAllConstants():
                        target = indexExpressionNode.Target;
                        break;
                    case IMethodCallExpressionNode methodCallExpression when methodCallExpression.Arguments.IsAllConstants():
                        target = methodCallExpression.Target;
                        break;
                    default:
                        return null;
                }
            }

            return result;
        }

        public static WeakEventListener ToWeak(this IEventListener listener) => new(listener);

        public static WeakEventListener<TState> ToWeak<TState>(this IEventListener listener, TState state) => new(listener, state);

        public static ItemOrArray<string> GetIndexerArgsRaw(string path)
        {
            var start = 1;
            if (path.StartsWith("Item[", StringComparison.Ordinal))
                start = 5;
            else if (!path.StartsWith("[", StringComparison.Ordinal) || !path.EndsWith("]", StringComparison.Ordinal))
                return default;

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

        public static ItemOrArray<string> GetMethodArgsRaw(string path, out string methodName)
        {
            var startIndex = path.IndexOf('(');
            if (startIndex < 0 || !path.EndsWith(")", StringComparison.Ordinal))
            {
                methodName = path;
                return default;
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

        public static Type GetTargetType<T>(this EnumFlags<MemberFlags> flags, ref T? target) where T : class => GetTargetType(flags.HasFlag(MemberFlags.Static), ref target);

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
        internal static EnumFlags<MemberFlags> GetDefaultFlags(this EnumFlags<MemberFlags> flags) => flags.Flags == 0 ? MemberFlags.All : flags;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static EnumFlags<BindingMemberExpressionFlags> SetTargetFlags(this EnumFlags<BindingMemberExpressionFlags> flags, bool isTarget) =>
            isTarget ? flags | BindingMemberExpressionFlags.Target : flags & ~BindingMemberExpressionFlags.Target;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static EnumFlags<MemberFlags> SetInstanceOrStaticFlags(this EnumFlags<MemberFlags> value, bool isStatic) =>
            isStatic ? (value | MemberFlags.Static) & ~MemberFlags.Instance : (value | MemberFlags.Instance) & ~MemberFlags.Static;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static EnumFlags<MemberFlags> ClearInstanceOrStaticFlags(this EnumFlags<MemberFlags> value, bool isStatic) =>
            isStatic ? value & ~MemberFlags.Instance : value & ~MemberFlags.Static;

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

        internal static ItemOrArray<T> InsertFirstArg<T>(this ItemOrArray<T> args, T firstArg)
        {
            if (args.IsEmpty)
                return firstArg;
            if (args.HasItem)
                return new[] {firstArg, args.Item!};
            return args.List.InsertFirstArg(firstArg);
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

        internal static void AddMethodObserver(this ObserverBase.IMethodPathObserver observer, object? target, IMemberInfo? lastMember, IReadOnlyMetadataContext? metadata,
            ref ActionToken unsubscriber,
            ref IWeakReference? lastValueRef)
        {
            unsubscriber.Dispose();
            if (target == null || lastMember is not IAccessorMemberInfo propertyInfo)
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
            var member = MugenService.MemberManager.TryGetMember(type, MemberType.Method, observer.MemberFlags.SetInstanceOrStaticFlags(false), observer.Method, metadata);
            if (member is IObservableMemberInfo observable)
                unsubscriber = observable.TryObserve(value, observer.GetMethodListener(), metadata);
            if (unsubscriber.IsEmpty)
                unsubscriber = ActionToken.NoDoToken;
        }

        internal static void EventHandlerWeakCanExecuteHandler(this IWeakReference weakReference, object? sender, EventArgs? args) =>
            ((BindingEventHandler?) weakReference.Target)?.OnCanExecuteChanged();

        internal static bool IsAllConstants(this ItemOrIReadOnlyList<IExpressionNode> expressions)
        {
            foreach (var expression in expressions)
            {
                if (expression.ExpressionType != ExpressionNodeType.Constant)
                    return false;
            }

            return true;
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

            if (sourceRaw is IConstantExpressionNode constant)
                return new ParameterValue(constant.Type, constant.Value);

            return new ParameterValue(sourceRaw.GetType(), sourceRaw);
        }

        private static object? GetValue(this IMemberManager memberManager, Type type, object? target, string path, EnumFlags<MemberFlags> flags, IReadOnlyMetadataContext? metadata)
        {
            var member = (IAccessorMemberInfo?) memberManager.TryGetMember(type, MemberType.Accessor, flags, path, metadata);
            if (member == null)
                ExceptionManager.ThrowInvalidBindingMember(type, path);
            return member.GetValue(target, metadata);
        }

#if SPAN_API
        private static ReadOnlySpan<char> RemoveBounds(this ReadOnlySpan<char> st, int start = 1) => st.Slice(start, st.Length - start - 1);

        private static ItemOrArray<string> UnescapeString(this ReadOnlySpan<char> source, char separator)
        {
            var length = 1;
            for (var i = 0; i < source.Length; i++)
            {
                if (source[i] == separator)
                    ++length;
            }

            if (length == 0)
                return default;

            var args = ItemOrArray.Get<string>(length);
            var index = 0;
            foreach (var arg in source.Split(separator))
            {
                var value = source[arg].Trim();
                if (value.StartsWith("\"", StringComparison.Ordinal) && value.EndsWith("\"", StringComparison.Ordinal)
                    || value.StartsWith("'", StringComparison.Ordinal) && value.EndsWith("'", StringComparison.Ordinal))
                    value = value.RemoveBounds();
                args.SetAt(index++, value.ToString());
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
    }
}