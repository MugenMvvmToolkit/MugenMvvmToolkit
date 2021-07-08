using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Compiling;
using MugenMvvm.Bindings.Interfaces.Compiling.Components;
using MugenMvvm.Bindings.Interfaces.Converting;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Interfaces.Resources;
using MugenMvvm.Bindings.Metadata;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Compiling.Components
{
    public sealed class MethodCallIndexerExpressionBuilder : IExpressionBuilderComponent, IHasPriority
    {
        private const float NotExactlyEqualWeight = 1f;
        private const float NotExactlyEqualBoxWeight = 1.1f;
        private const float NotExactlyEqualUnsafeCastWeight = 1000f;
        private const float NotExactlyEqualUnsafeCastObjectWeight = 10f;

        private static readonly Expression[] ExpressionCallBuffer = new Expression[5];

        private static readonly ConstructorInfo ItemOrArrayInternalConstructor =
            typeof(ItemOrArray<object>).GetConstructorOrThrow(BindingFlagsEx.InstancePublic | BindingFlagsEx.InstanceNonPublic,
                new[] { typeof(object), typeof(object[]), typeof(int) });

        private static readonly MethodInfo InvokeMethod = typeof(IMethodMemberInfo).GetMethodOrThrow(nameof(IMethodMemberInfo.Invoke), BindingFlagsEx.InstancePublic);
        private static readonly MethodInfo MethodInvokerInvokeMethod = typeof(MethodInvoker).GetMethodOrThrow(nameof(MethodInvoker.Invoke), BindingFlagsEx.InstancePublic);

        private readonly IGlobalValueConverter? _globalValueConverter;

        private readonly IMemberManager? _memberManager;
        private readonly IResourceManager? _resourceResolver;

        public MethodCallIndexerExpressionBuilder(IMemberManager? memberManager = null, IResourceManager? resourceResolver = null,
            IGlobalValueConverter? globalValueConverter = null)
        {
            _memberManager = memberManager;
            _resourceResolver = resourceResolver;
            _globalValueConverter = globalValueConverter;
        }

        public EnumFlags<MemberFlags> MemberFlags { get; set; } = Enums.MemberFlags.All & ~Enums.MemberFlags.NonPublic;

        public int Priority { get; init; } = CompilingComponentPriority.Member;

        public Expression? TryBuild(IExpressionBuilderContext context, IExpressionNode expression) =>
            expression switch
            {
                IIndexExpressionNode indexExpression => TryBuildIndex(context, indexExpression),
                IMethodCallExpressionNode methodCallExpression => TryBuildMethod(context, methodCallExpression),
                _ => null
            };

        private static void GetBestMethodCandidates(ref ItemOrArray<MethodData> methods, ItemOrArray<ArgumentData> arguments)
        {
            for (var index = 0; index < methods.Count; index++)
            {
                try
                {
                    var methodInfo = methods[index];
                    if (methodInfo.IsEmpty)
                        continue;

                    methods.SetAt(index, default);
                    var methodData = TryInferMethod(methodInfo.Method!, arguments);
                    if (methodData.IsEmpty)
                        continue;

                    var parameters = methodInfo.Parameters;
                    var optionalCount = parameters.Count(info => info.HasDefaultValue);
                    var requiredCount = parameters.Count - optionalCount;
                    var hasParams = false;
                    if (parameters.Count != 0)
                    {
                        hasParams = parameters.Last().IsParamArray();
                        if (hasParams)
                            requiredCount -= 1;
                    }

                    if (requiredCount > arguments.Count)
                        continue;
                    if (parameters.Count < arguments.Count && !hasParams)
                        continue;
                    var count = parameters.Count > arguments.Count ? arguments.Count : parameters.Count;
                    var valid = true;
                    for (var i = 0; i < count; i++)
                    {
                        if (!IsCompatible(parameters[i].ParameterType, arguments[i].Node))
                        {
                            valid = false;
                            break;
                        }
                    }

                    if (valid)
                        methods.SetAt(index, methodData);
                }
                catch
                {
                    // ignored
                }
            }
        }

        private static Expression? TryGenerateMethodCall(IExpressionBuilderContext context, ref ItemOrArray<MethodData> methods, in TargetData target,
            ref ItemOrArray<ArgumentData> arguments)
        {
            if (methods.IsEmpty)
                return null;

            for (var i = 0; i < methods.Count; i++)
            {
                try
                {
                    var method = methods[i];
                    if (method.IsEmpty)
                        continue;

                    var expressions = ItemOrArray.Get<Expression>(arguments.Count);
                    for (var index = 0; index < arguments.Count; index++)
                    {
                        var data = arguments[index];
                        if (data.IsLambda)
                        {
                            var oldLambdaParameter = context.Metadata.Get(CompilingMetadata.LambdaParameter);
                            context.Metadata.Set(CompilingMetadata.LambdaParameter, method.Parameters[index], out _);
                            try
                            {
                                data = data.UpdateExpression(context.Build(data.Node));
                            }
                            finally
                            {
                                if (oldLambdaParameter == null)
                                    context.Metadata.Remove(CompilingMetadata.LambdaParameter, out _);
                                else
                                    context.Metadata.Set(CompilingMetadata.LambdaParameter, oldLambdaParameter, out _);
                            }

                            arguments.SetAt(index, data);
                        }

                        expressions.SetAt(index, data.Expression!);
                    }

                    methods.SetAt(i, method.TryResolve(arguments, expressions));
                }
                catch
                {
                    methods.SetAt(i, default);
                }
            }

            var resultIndex = TrySelectMethod(methods, arguments, out var resultHasParams);
            if (resultIndex < 0)
                return null;

            var result = methods[resultIndex];
            var resultArgs = ConvertParameters(context, result, resultHasParams);
            if (result.Method!.UnderlyingMember is MethodInfo m)
            {
                Expression? targetExp;
                if (result.Method.MemberFlags.HasFlag(Enums.MemberFlags.Extension))
                {
                    targetExp = null;
                    resultArgs = resultArgs.InsertFirstArg(target.Expression!);
                }
                else
                    targetExp = target.Expression;

                return Expression.Call(targetExp, m, resultArgs.AsList());
            }

            for (var i = 0; i < resultArgs.Count; i++)
                resultArgs.SetAt(i, resultArgs[i].ConvertIfNeed(typeof(object), false));

            var invokeArgs = new Expression[3];
            invokeArgs[0] = target.Expression.ConvertIfNeed(typeof(object), false) ?? MugenExtensions.NullConstantExpression;
            invokeArgs[1] = ToItemOrList(resultArgs);
            invokeArgs[2] = context.MetadataExpression;
            return Expression.Call(Expression.Constant(result.Method), InvokeMethod, invokeArgs).ConvertIfNeed(result.Method.Type, true);
        }

        private static int TrySelectMethod(ItemOrArray<MethodData> methods, ItemOrArray<ArgumentData> args, out bool resultHasParams,
            float safeWeight = NotExactlyEqualUnsafeCastWeight)
        {
            var result = -1;
            resultHasParams = true;
            var resultUseParams = true;
            var resultNotExactlyEqual = float.MaxValue;
            var resultUsageCount = int.MinValue;
            for (var i = 0; i < methods.Count; i++)
            {
                var methodInfo = methods[i];
                if (methodInfo.IsEmpty)
                    continue;

                try
                {
                    var parameters = methodInfo.Parameters;
                    var useParams = false;
                    var hasParams = false;
                    var lastIndex = 0;
                    var usageCount = 0;
                    if (parameters.Count != 0)
                    {
                        lastIndex = parameters.Count - 1;
                        hasParams = parameters[lastIndex].IsParamArray();
                    }

                    var notExactlyEqual = methodInfo.Method!.MemberFlags.HasFlag(Enums.MemberFlags.Extension) ? NotExactlyEqualBoxWeight : 0;
                    var valid = true;
                    for (var j = 0; j < methodInfo.ExpectedParameterCount; j++)
                    {
                        //params
                        if (j > lastIndex)
                        {
                            valid = hasParams && CheckParamsCompatible(j - 1, lastIndex, parameters, methodInfo, ref notExactlyEqual);
                            useParams = true;
                            break;
                        }

                        var argType = methodInfo.GetExpectedParameterType(j);
                        var parameterType = parameters[j].ParameterType;
                        if (parameterType.IsByRef)
                            parameterType = parameterType.GetElementType()!;
                        if (parameterType == argType)
                        {
                            ++usageCount;
                            continue;
                        }

                        if (argType.IsCompatibleWith(parameterType, out var boxRequired))
                        {
                            notExactlyEqual += boxRequired ? NotExactlyEqualBoxWeight : NotExactlyEqualWeight;
                            ++usageCount;
                        }
                        else
                        {
                            if (lastIndex == j && hasParams)
                            {
                                valid = CheckParamsCompatible(j, lastIndex, parameters, methodInfo, ref notExactlyEqual);
                                useParams = true;
                                break;
                            }

                            if (argType.IsValueType)
                            {
                                valid = false;
                                break;
                            }

                            ++usageCount;
                            notExactlyEqual += argType == typeof(object) ? NotExactlyEqualUnsafeCastObjectWeight : NotExactlyEqualUnsafeCastWeight;
                        }
                    }

                    if (!valid)
                        continue;
                    if (notExactlyEqual > resultNotExactlyEqual)
                        continue;
                    if (notExactlyEqual == resultNotExactlyEqual)
                    {
                        if (usageCount < resultUsageCount)
                            continue;
                        if (usageCount == resultUsageCount)
                        {
                            if (useParams && !resultUseParams)
                                continue;
                            if (!useParams && !resultUseParams)
                            {
                                if (hasParams && !resultHasParams)
                                    continue;
                            }
                        }
                    }

                    result = i;
                    resultNotExactlyEqual = notExactlyEqual;
                    resultUsageCount = usageCount;
                    resultUseParams = useParams;
                    resultHasParams = hasParams;
                }
                catch
                {
                    ;
                }
            }

            if (result != -1 && resultNotExactlyEqual >= safeWeight && args.All(data => !data.IsLambda))
                return -1;

            return result;
        }

        private static bool CheckParamsCompatible(int startIndex, int lastIndex, ItemOrIReadOnlyList<IParameterInfo> parameters, in MethodData method, ref float notExactlyEqual)
        {
            float weight = 0;
            var elementType = parameters[lastIndex].ParameterType.GetElementType()!;
            for (var i = startIndex; i < method.ExpectedParameterCount; i++)
            {
                var argType = method.GetExpectedParameterType(i);
                if (elementType == argType)
                    continue;
                if (argType.IsCompatibleWith(elementType, out var boxRequired))
                {
                    var w = boxRequired ? NotExactlyEqualBoxWeight : NotExactlyEqualWeight;
                    if (w > weight)
                        weight = w;
                }
                else
                {
                    if (argType.IsValueType)
                        return false;
                    if (NotExactlyEqualUnsafeCastWeight > weight)
                        weight = NotExactlyEqualUnsafeCastWeight;
                }
            }

            notExactlyEqual += weight;
            return true;
        }

        private static ItemOrArray<Expression> ConvertParameters(IExpressionBuilderContext context, in MethodData method, bool hasParams)
        {
            var parameters = method.Parameters;
            var args = method.Expressions;
            var result = ItemOrArray.Get<Expression>(parameters.Count);
            for (var i = 0; i < parameters.Count; i++)
            {
                //optional or params
                if (i > args.Count - 1)
                {
                    for (var j = i; j < parameters.Count; j++)
                    {
                        var parameter = parameters[j];
                        if (j == parameters.Count - 1 && hasParams)
                            result.SetAt(j, Expression.Constant(Default.Array(parameter.ParameterType.GetElementType()!)));
                        else
                        {
                            result.SetAt(j,
                                parameter.ParameterType == typeof(IReadOnlyMetadataContext)
                                    ? context.MetadataExpression
                                    : Expression.Constant(parameter.DefaultValue).ConvertIfNeed(parameter.ParameterType, false));
                        }
                    }

                    break;
                }

                if (i == parameters.Count - 1 && hasParams && !args[i].Type.IsCompatibleWith(parameters[i].ParameterType))
                {
                    var arrayType = parameters[i].ParameterType.GetElementType()!;
                    var arrayArgs = new Expression[args.Count - i];
                    for (var j = i; j < args.Count; j++)
                        arrayArgs[j - i] = args[j].ConvertIfNeed(arrayType, false);
                    result.SetAt(i, Expression.NewArrayInit(arrayType, arrayArgs));
                }
                else
                    result.SetAt(i, args[i].ConvertIfNeed(parameters[i].ParameterType, false));
            }

            return result;
        }

        private static ItemOrArray<ArgumentData> GetArguments(IHasArgumentsExpressionNode<IExpressionNode> hasArguments, IExpressionBuilderContext context)
        {
            var arguments = hasArguments.Arguments;
            if (arguments.Count == 0)
                return default;
            var args = ItemOrArray.Get<ArgumentData>(arguments.Count);
            for (var i = 0; i < args.Count; i++)
            {
                var node = arguments[i];
                args.SetAt(i, new ArgumentData(node, node.ExpressionType == ExpressionNodeType.Lambda ? null : context.Build(node), null));
            }

            return args;
        }

        private static MethodData TryInferMethod(IMethodMemberInfo method, ItemOrArray<ArgumentData> args)
        {
            if (!method.IsGenericMethodDefinition)
                return new MethodData(method);
            var genericMethod = TryInferGenericMethod(method, args, out var hasUnresolved);
            if (genericMethod == null)
                return default;
            if (hasUnresolved)
                return new MethodData(genericMethod, method);
            return new MethodData(genericMethod);
        }

        private static IMethodMemberInfo? TryInferGenericMethod(IMethodMemberInfo method, ItemOrArray<ArgumentData> args, out bool hasUnresolved)
        {
            var inferredTypes = BindingMugenExtensions.TryInferGenericParameters(method.GetGenericArguments(), method.GetParameters(), info => info.ParameterType, args,
                (data, i) => data[i].Type, args.Count,
                out hasUnresolved);
            if (inferredTypes.IsEmpty)
                return null;
            return method.MakeGenericMethod(inferredTypes);
        }

        private static IMethodMemberInfo? ApplyTypeArgs(IMethodMemberInfo m, ItemOrArray<Type> typeArgs)
        {
            if (typeArgs.IsEmpty)
            {
                if (!m.IsGenericMethodDefinition)
                    return m;
            }
            else
            {
                if (m.IsGenericMethod && !m.IsGenericMethodDefinition)
                    m = m.GetGenericMethodDefinition();
                if (m.IsGenericMethodDefinition && m.GetGenericArguments().Count == typeArgs.Count)
                    return m.MakeGenericMethod(typeArgs);
            }

            return null;
        }

        private static bool IsCompatible(Type parameterType, IExpressionNode node)
        {
            if (node is not ILambdaExpressionNode lambdaExpressionNode)
                return true;

            if (typeof(Expression).IsAssignableFrom(parameterType) && parameterType.IsGenericType)
                parameterType = parameterType.GetGenericArguments()[0];
            if (!typeof(Delegate).IsAssignableFrom(parameterType))
                return false;

            var method = parameterType.GetMethod(nameof(Action.Invoke), BindingFlagsEx.InstancePublic);
            if (method == null || method.GetParameters().Length != lambdaExpressionNode.Parameters.Count)
                return false;
            return true;
        }

        private static ItemOrArray<Expression> ToExpressions(IExpressionBuilderContext context, ItemOrIReadOnlyList<IExpressionNode> args, IMethodMemberInfo? method,
            Type? convertType)
        {
            var count = args.Count;
            var parameters = method?.GetParameters() ?? default;
            var expressions = ItemOrArray.Get<Expression>(count);
            var index = 0;
            foreach (var arg in args)
            {
                var expression = context.Build(arg);
                if (convertType != null)
                    expression = expression.ConvertIfNeed(convertType, true);
                else if (parameters.Count > index)
                    expression = expression.ConvertIfNeed(parameters[index].ParameterType, true);
                expressions.SetAt(index++, expression);
            }

            return expressions;
        }

        private static Expression ToItemOrList(ItemOrArray<Expression> args)
        {
            if (args.IsEmpty)
                return Expression.Default(typeof(ItemOrArray<object>));
            if (args.HasItem)
                return Expression.New(ItemOrArrayInternalConstructor, args.Item!, MugenExtensions.NullArrayConstantExpression, MugenExtensions.GetConstantExpression(1));
            return Expression.New(ItemOrArrayInternalConstructor, MugenExtensions.NullConstantExpression, Expression.NewArrayInit(typeof(object), args.List!),
                MugenExtensions.GetConstantExpression(args.Count));
        }

        private Expression? TryBuildMethod(IExpressionBuilderContext context, IMethodCallExpressionNode methodCallExpression)
        {
            if (methodCallExpression.Target == null)
            {
                context.TryGetErrors()?.Add(BindingMessageConstant.CannotCompileIndexerMethodExpressionNullTargetFormat1.Format(methodCallExpression));
                return null;
            }

            var target = context.BuildTarget(methodCallExpression.Target, out var type);
            return TryBuildExpression(context, methodCallExpression.Method, new TargetData(type, target), GetArguments(methodCallExpression, context),
                _resourceResolver.GetTypes(methodCallExpression.TypeArgs, context.Metadata));
        }

        private Expression? TryBuildIndex(IExpressionBuilderContext context, IIndexExpressionNode indexExpression)
        {
            if (indexExpression.Target == null)
            {
                context.TryGetErrors()?.Add(BindingMessageConstant.CannotCompileIndexerMethodExpressionNullTargetFormat1.Format(indexExpression));
                return null;
            }

            var target = context.BuildTarget(indexExpression.Target, out var type);
            if (type.IsArray)
            {
                var expressions = ToExpressions(context, indexExpression.Arguments, null, typeof(int));
                if (expressions.List != null)
                    return Expression.ArrayIndex(target!, expressions.List);
                return Expression.ArrayIndex(target!, expressions.Item!);
            }

            return TryBuildExpression(context, BindingInternalConstant.IndexerGetterName, new TargetData(type, target), GetArguments(indexExpression, context), default);
        }

        private Expression? TryBuildExpression(IExpressionBuilderContext context, string methodName, in TargetData targetData, ItemOrArray<ArgumentData> args,
            ItemOrArray<Type> typeArgs)
        {
            var methods = GetMethods(targetData.Type, methodName, targetData.IsStatic, typeArgs, context.GetMetadataOrDefault());
            GetBestMethodCandidates(ref methods, args);
            var expression = TryGenerateMethodCall(context, ref methods, targetData, ref args);
            if (expression != null)
                return expression;

            if (targetData.Expression == null)
            {
                context.TryGetErrors()?.Add(BindingMessageConstant.InvalidBindingMemberFormat2.Format(methodName, targetData.Type));
                return null;
            }

            var arrayArgs = ItemOrArray.Get<Expression>(args.Count);
            for (var i = 0; i < args.Count; i++)
            {
                var data = args[i];
                if (data.IsLambda || data.Expression == null)
                {
                    context.TryGetErrors()?.Add(BindingMessageConstant.InvalidBindingMemberFormat2.Format(methodName, targetData.Type));
                    return null;
                }

                arrayArgs.SetAt(i, data.Expression.ConvertIfNeed(typeof(object), false));
            }

            try
            {
                ExpressionCallBuffer[0] = targetData.Expression;
                ExpressionCallBuffer[1] = Expression.Constant(methodName);
                ExpressionCallBuffer[2] = ToItemOrList(arrayArgs);
                ExpressionCallBuffer[3] = Expression.Constant(typeArgs.GetRawValue());
                ExpressionCallBuffer[4] = context.MetadataExpression;
                return Expression.Call(Expression.Constant(new MethodInvoker(this)), MethodInvokerInvokeMethod, ExpressionCallBuffer);
            }
            finally
            {
                Array.Clear(ExpressionCallBuffer, 0, ExpressionCallBuffer.Length);
            }
        }

        private ItemOrArray<MethodData> GetMethods(Type type, string methodName, bool isStatic, ItemOrArray<Type> typeArgs, IReadOnlyMetadataContext? metadata)
        {
            var members = _memberManager
                          .DefaultIfNull()
                          .TryGetMembers(type, MemberType.Method, MemberFlags.SetInstanceOrStaticFlags(isStatic), methodName, metadata);

            var methods = ItemOrArray.Get<MethodData>(members.Count);
            var count = 0;
            foreach (var member in members)
            {
                if (member is IMethodMemberInfo method)
                {
                    var m = typeArgs.IsEmpty ? method : ApplyTypeArgs(method, typeArgs);
                    if (m != null)
                        methods.SetAt(count++, new MethodData(m));
                }
            }

            return methods;
        }

        [StructLayout(LayoutKind.Auto)]
        internal readonly struct MethodInvokerKey : IEquatable<MethodInvokerKey>
        {
            public readonly object? Args;
            public readonly Type Type;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public MethodInvokerKey(Type type, ItemOrArray<object?> args)
            {
                Type = type;
                if (args.IsEmpty)
                    Args = null;
                else if (args.HasItem)
                    Args = GetValueType(args.Item);
                else
                    Args = args.GetRawValue();
            }

            public bool Equals(MethodInvokerKey other)
            {
                if (Type != other.Type)
                    return false;
                if (ReferenceEquals(Args, other.Args))
                    return true;
                if (Args == null || other.Args == null)
                    return false;

                if (Args is Type t)
                {
                    if (other.Args is Type oT)
                        return t == oT;
                    return false;
                }

                var typesX = Args as Type[];
                var typesY = other.Args as Type[];
                if (typesX == null && typesY == null)
                    return false;

                if (typesX == null)
                    return Equals(typesY!, (object?[])Args);
                if (typesY == null)
                    return Equals(typesX!, (object?[])other.Args);
                return InternalEqualityComparer.Equals(typesX, typesY);
            }

            public override int GetHashCode()
            {
                if (Args == null)
                    return Type.GetHashCode();
                if (Args is Type t)
                    return HashCode.Combine(Type, t);

                var hashCode = new HashCode();
                hashCode.Add(Type);
                if (Args is Type[] types)
                {
                    foreach (var type in types)
                        hashCode.Add(type);
                }
                else
                {
                    foreach (var value in (object[])Args)
                        hashCode.Add(GetValueType(value));
                }

                return hashCode.ToHashCode();
            }

            public MethodInvokerKey ToKey(ItemOrArray<object?> args)
            {
                if (args.Count < 2)
                    return this;

                var result = new Type[args.Count];
                for (var i = 0; i < args.Count; i++)
                    result[i] = GetValueType(args[i]);
                return new MethodInvokerKey(Type, result);
            }

            private static bool Equals(Type[] types, object?[] values)
            {
                if (values.Length != types.Length)
                    return false;
                for (var i = 0; i < values.Length; i++)
                {
                    if (GetValueType(values[i]) != types[i])
                        return false;
                }

                return true;
            }

            private static Type GetValueType(object? value) => value == null ? typeof(object) : value.GetType();
        }

        [StructLayout(LayoutKind.Auto)]
        private readonly struct TargetData
        {
            public readonly Expression? Expression;
            public readonly Type Type;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TargetData(Type type, Expression? expression)
            {
                Type = type;
                Expression = expression;
            }

            [MemberNotNullWhen(false, nameof(Expression))]
            public bool IsStatic
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => Expression == null;
            }
        }

        [StructLayout(LayoutKind.Auto)]
        private readonly struct ArgumentData
        {
            public readonly IExpressionNode Node;
            public readonly Expression? Expression;
            public readonly Type? Type;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ArgumentData(IExpressionNode node, Expression? expression, Type? type)
            {
                Should.NotBeNull(node, nameof(node));
                Node = node;
                Expression = expression;
                if (type == null && expression != null)
                    type = expression.Type;
                Type = type;
            }

            public bool IsLambda
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => Node.ExpressionType == ExpressionNodeType.Lambda;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ArgumentData UpdateExpression(Expression expression) => new(Node, expression, Type);
        }

        [StructLayout(LayoutKind.Auto)]
        private readonly struct MethodData
        {
            private readonly object? _parametersRaw;
            private readonly IMethodMemberInfo? _unresolvedMethod;
            public readonly IMethodMemberInfo? Method;
            private readonly object? _args;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public MethodData(IMethodMemberInfo method)
                : this(method, null)
            {
                Method = method;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public MethodData(IMethodMemberInfo method, IMethodMemberInfo? unresolvedMethod, object? args = null)
            {
                Method = method;
                _parametersRaw = method.GetParameters().GetRawValue();
                _unresolvedMethod = unresolvedMethod;
                _args = args;
            }

            [MemberNotNullWhen(false, nameof(Method))]
            public bool IsEmpty => Method == null;

            public int ExpectedParameterCount
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    if (_args == null)
                        return 0;
                    if (_args is Expression || _args is Type)
                        return 1;
                    if (_args is Expression[] expressions)
                        return expressions.Length;
                    return ((Type[])_args).Length;
                }
            }

            public ItemOrArray<Expression> Expressions
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ItemOrArray.FromRawValue<Expression>(_args);
            }

            public ItemOrIReadOnlyList<IParameterInfo> Parameters
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ItemOrIReadOnlyList.FromRawValue<IParameterInfo>(_parametersRaw);
            }

            public Type GetExpectedParameterType(int index)
            {
                if (_args is Expression exp)
                {
                    Should.BeValid(index == 0, nameof(index));
                    return exp.Type;
                }

                if (_args is Type t)
                {
                    Should.BeValid(index == 0, nameof(index));
                    return t;
                }

                if (_args is Expression[] expressions)
                    return expressions[index].Type;
                return ((Type[])_args!)[index];
            }

            public MethodData WithArgs(ItemOrArray<object?> args, ref ItemOrArray<Type> instanceArgs)
            {
                if (IsEmpty)
                    return default;
                if (instanceArgs.IsEmpty)
                {
                    if (args.Count == 0)
                        instanceArgs = default;
                    else
                    {
                        instanceArgs = ItemOrArray.Get<Type>(args.Count);
                        for (var i = 0; i < args.Count; i++)
                            instanceArgs.SetAt(i, args[i]?.GetType() ?? typeof(object));
                    }
                }

                return new MethodData(Method!, null, instanceArgs.GetRawValue());
            }

            public MethodData TryResolve(ItemOrArray<ArgumentData> args, ItemOrArray<Expression> expressions)
            {
                if (_unresolvedMethod == null)
                    return new MethodData(Method!, null, expressions.GetRawValue());

                var method = TryInferGenericMethod(_unresolvedMethod, args, out var unresolved);
                if (method == null || unresolved)
                    return default;
                return new MethodData(method!, null, expressions.GetRawValue());
            }
        }

        [Preserve(AllMembers = true, Conditional = true)]
        private sealed class MethodInvoker : Dictionary<MethodInvokerKey, MethodData>
        {
            private readonly MethodCallIndexerExpressionBuilder _component;

            public MethodInvoker(MethodCallIndexerExpressionBuilder component) : base(3)
            {
                _component = component;
            }

            public object? Invoke(object? target, string methodName, ItemOrArray<object?> args, object? typeArgs, IReadOnlyMetadataContext? metadata)
            {
                if (target.IsNullOrUnsetValue())
                    return BindingMetadata.UnsetValue;

                var key = new MethodInvokerKey(target.GetType(), args);
                if (!TryGetValue(key, out var method))
                {
                    var methods = _component.GetMethods(key.Type, methodName, false, ItemOrArray.FromRawValue<Type>(typeArgs), metadata);
                    ItemOrArray<Type> instanceArgs = default;
                    for (var i = 0; i < methods.Count; i++)
                        methods.SetAt(i, methods[i].WithArgs(args, ref instanceArgs));
                    var resultIndex = TrySelectMethod(methods, default, out _);
                    method = resultIndex >= 0 ? methods[resultIndex] : default;
                    this[key.ToKey(args)] = method;
                }

                if (method.IsEmpty)
                    ExceptionManager.ThrowInvalidBindingMember(key.Type, methodName);
                return method.Method!.Invoke(target, _component._globalValueConverter.TryGetInvokeArgs(method.Parameters, args, metadata)!, metadata);
            }
        }
    }
}