using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Interfaces.Resources;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Compiling.Components
{
    public sealed class MethodIndexerExpressionBuilderComponent : ExpressionCompilerComponent.IExpressionBuilder, IHasPriority
    {
        #region Fields

        private readonly IMemberProvider? _memberProvider;
        private readonly IResourceResolver? _resourceResolver;

        private const float NotExactlyEqualWeight = 1f;
        private const float NotExactlyEqualBoxWeight = 1.1f;
        private const float NotExactlyEqualUnsafeCastWeight = 1000f;

        private static readonly Expression[] ExpressionCallBuffer = new Expression[5];
        private static readonly int[] ArraySize = new int[1];
        private static readonly MethodInfo InvokeMethod = typeof(IBindingMethodInfo).GetMethodOrThrow(nameof(IBindingMethodInfo.Invoke), MemberFlags.InstancePublic);
        private static readonly MethodInfo MethodInvokerInvokeMethod = typeof(MethodInvoker).GetMethodOrThrow(nameof(MethodInvoker.Invoke), MemberFlags.InstancePublic);

        #endregion

        #region Constructors

        public MethodIndexerExpressionBuilderComponent(IMemberProvider? memberProvider = null, IResourceResolver? resourceResolver = null)
        {
            _memberProvider = memberProvider;
            _resourceResolver = resourceResolver;
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        public MemberFlags MemberFlags { get; set; } = MemberFlags.All & ~MemberFlags.NonPublic;

        #endregion

        #region Implementation of interfaces

        public Expression? TryBuild(ExpressionCompilerComponent.IContext context, IExpressionNode expression)
        {
            switch (expression)
            {
                case IIndexExpressionNode indexExpression:
                    return indexExpression.Build(this, context, (component, ctx, m, target) => component.TryBuildIndex(ctx, m, target));
                case IMethodCallExpressionNode methodCallExpression:
                    return methodCallExpression.Build(this, context, (component, ctx, m, target) => component.TryBuildMethod(ctx, m, target));
                default:
                    return null;
            }
        }

        #endregion

        #region Methods

        private Expression? TryBuildMethod(ExpressionCompilerComponent.IContext context, IMethodCallExpressionNode methodCallExpression, Expression target)
        {
            var type = BindingMugenExtensions.GetTargetType(ref target);

            if (methodCallExpression.Method != null)
                return GenerateMethodCall(context, methodCallExpression.Method, target, methodCallExpression.Arguments);

            var targetData = new TargetData(methodCallExpression.Target!, type, target);
            var args = new ArgumentData[methodCallExpression.Arguments.Count];
            for (var i = 0; i < args.Length; i++)
            {
                var node = methodCallExpression.Arguments[i];
                args[i] = new ArgumentData(node, node.NodeType == ExpressionNodeType.Lambda ? null : context.Build(methodCallExpression.Arguments[i]), null);
            }

            return TryBuildExpression(context, methodCallExpression.MethodName, targetData, args, GetTypes(methodCallExpression.TypeArgs));
        }

        private Expression? TryBuildIndex(ExpressionCompilerComponent.IContext context, IIndexExpressionNode indexExpression, Expression target)
        {
            var type = BindingMugenExtensions.GetTargetType(ref target);

            if (indexExpression.Indexer != null)
                return GenerateMethodCall(context, indexExpression.Indexer, target, indexExpression.Arguments);

            if (target.Type.IsArray)
                return Expression.ArrayIndex(target, ToExpressions(context, indexExpression.Arguments, null, typeof(int)));

            var targetData = new TargetData(indexExpression.Target!, type, target);
            var args = new ArgumentData[indexExpression.Arguments.Count];
            for (var i = 0; i < args.Length; i++)
            {
                var node = indexExpression.Arguments[i];
                args[i] = new ArgumentData(node, node.NodeType == ExpressionNodeType.Lambda ? null : context.Build(indexExpression.Arguments[i]), null);
            }

            return TryBuildExpression(context, type.EqualsEx(typeof(string)) ? "get_Chars" : "get_Item", targetData, args, Default.EmptyArray<Type>());
        }

        private Expression? TryBuildExpression(ExpressionCompilerComponent.IContext context, string methodName, in TargetData targetData, ArgumentData[] args, Type[] typeArgs)
        {
            var methods = FindBestMethods(targetData, GetMethods(targetData.Type, methodName, targetData.IsStatic, null, context.GetMetadataOrDefault()), args, typeArgs);
            var expression = TryGenerateMethodCall(context, methods, targetData, args);
            if (expression != null)
                return expression;

            if (targetData.Expression == null)
                return null;

            var arrayArgs = new Expression[args.Length];
            for (var i = 0; i < args.Length; i++)
            {
                var data = args[i];
                if (data.IsLambda || data.Expression == null)
                    return null;
                arrayArgs[i] = data.Expression.ConvertIfNeed(typeof(object), false);
            }

            try
            {
                ExpressionCallBuffer[0] = targetData.Expression;
                ExpressionCallBuffer[1] = Expression.Constant(methodName, typeof(string));
                ExpressionCallBuffer[2] = Expression.NewArrayInit(typeof(object), arrayArgs);
                ExpressionCallBuffer[3] = Expression.Constant(typeArgs, typeof(Type[]));
                ExpressionCallBuffer[4] = context.MetadataParameter;
                return Expression.Call(Expression.Constant(new MethodInvoker(this), typeof(MethodInvoker)), MethodInvokerInvokeMethod, ExpressionCallBuffer);
            }
            finally
            {
                Array.Clear(ExpressionCallBuffer, 0, ExpressionCallBuffer.Length);
            }
        }

        private Type[] GetTypes(IReadOnlyList<string>? types)
        {
            if (types == null || types.Count == 0)
                return Default.EmptyArray<Type>();
            var resolver = _resourceResolver.ServiceIfNull();
            var typeArgs = new Type[types.Count];
            for (var i = 0; i < types.Count; i++)
            {
                var type = resolver.TryGetType(types[i]);
                if (type == null)
                    BindingExceptionManager.ThrowCannotResolveType(types[i]);
                typeArgs[i] = type!;
            }

            return typeArgs;
        }

        private static MethodData[] FindBestMethods(in TargetData target, MethodData[] methods, ArgumentData[] arguments, Type[] typeArgs)
        {
            for (var index = 0; index < methods.Length; index++)
            {
                try
                {
                    var methodInfo = methods[index];
                    if (methodInfo.IsEmpty)
                        continue;

                    methods[index] = default;
                    var args = GetMethodArgs(methodInfo.IsExtensionMethod, target, arguments);
                    var methodData = TryInferMethod(methodInfo.Method, args, typeArgs);
                    if (methodData.IsEmpty)
                        continue;

                    var parameters = methodInfo.Parameters;
                    var optionalCount = parameters.Count(info => info.HasDefaultValue);
                    var requiredCount = parameters.Length - optionalCount;
                    var hasParams = false;
                    if (parameters.Length != 0)
                    {
                        hasParams = parameters[parameters.Length - 1].IsParamsArray;
                        if (hasParams)
                            requiredCount -= 1;
                    }

                    if (requiredCount > args.Length)
                        continue;
                    if (parameters.Length < args.Length && !hasParams)
                        continue;
                    var count = parameters.Length > args.Length ? args.Length : parameters.Length;
                    var valid = true;
                    for (var i = 0; i < count; i++)
                    {
                        var arg = args[i];
                        if (!IsCompatible(parameters[i].ParameterType, arg.Node))
                        {
                            valid = false;
                            break;
                        }
                    }

                    if (valid)
                        methods[index] = methodData;
                }
                catch
                {
                    ;
                }
            }

            return methods;
        }

        private static Expression? TryGenerateMethodCall(ExpressionCompilerComponent.IContext context, MethodData[] methods, in TargetData target, ArgumentData[] arguments)
        {
            if (methods.Length == 0)
                return null;

            for (var i = 0; i < methods.Length; i++)
            {
                try
                {
                    var method = methods[i];
                    if (method.IsEmpty)
                        continue;

                    var args = GetMethodArgs(method.IsExtensionMethod, target, arguments);
                    var expressions = new Expression[args.Length];
                    for (var index = 0; index < args.Length; index++)
                    {
                        var data = args[index];
                        if (data.IsLambda)
                        {
                            var lambdaParameter = method.Parameters[index];
                            context.SetLambdaParameter(lambdaParameter);
                            try
                            {
                                data = data.UpdateExpression(context.Build(data.Node));
                            }
                            finally
                            {
                                context.ClearLambdaParameter(lambdaParameter);
                            }

                            args[index] = data;
                        }

                        expressions[index] = data.Expression!;
                    }

                    methods[i] = method.TryResolve(args, expressions);
                }
                catch
                {
                    methods[i] = default;
                }
            }

            var resultIndex = TrySelectMethod(methods, arguments, out var resultHasParams);
            if (resultIndex < 0)
                return null;

            var result = methods[resultIndex];
            var resultArgs = ConvertParameters(context, result, resultHasParams);

            if (result.Method.Member is MethodInfo m)
                return Expression.Call(result.IsExtensionMethod ? null : target.Expression, m, resultArgs);

            var invokeArgs = new Expression[3];
            invokeArgs[0] = (result.IsExtensionMethod ? null : target.Expression).ConvertIfNeed(typeof(object), false);
            invokeArgs[1] = Expression.NewArrayInit(typeof(object), resultArgs.Select(expression => expression.ConvertIfNeed(typeof(object), false)));
            invokeArgs[2] = context.MetadataParameter;
            return Expression.Call(Expression.Constant(result.Method, typeof(IBindingMethodInfo)), InvokeMethod, invokeArgs);
        }

        private static Expression GenerateMethodCall(ExpressionCompilerComponent.IContext context, IBindingMethodInfo methodInfo, Expression target, IReadOnlyList<IExpressionNode> args)
        {
            var expressions = ToExpressions(context, args, methodInfo, null);
            if (methodInfo.Member is MethodInfo method)
                return Expression.Call(target, method, expressions);

            var invokeArgs = new Expression[3];
            invokeArgs[0] = target.ConvertIfNeed(typeof(object), false);
            invokeArgs[1] = Expression.NewArrayInit(typeof(object), expressions.Select(expression => expression.ConvertIfNeed(typeof(object), false)));
            invokeArgs[2] = context.MetadataParameter;
            return Expression.Call(Expression.Constant(methodInfo, typeof(IBindingMethodInfo)), InvokeMethod, invokeArgs);
        }

        private static int TrySelectMethod(MethodData[] methods, ArgumentData[]? args, out bool resultHasParams)
        {
            var result = -1;
            resultHasParams = true;
            var resultUseParams = true;
            var resultNotExactlyEqual = float.MaxValue;
            var resultUsageCount = int.MinValue;
            for (var i = 0; i < methods.Length; i++)
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
                    if (parameters.Length != 0)
                    {
                        lastIndex = parameters.Length - 1;
                        hasParams = parameters[lastIndex].IsParamsArray;
                    }

                    float notExactlyEqual = 0;
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
                            parameterType = parameterType.GetElementType();
                        if (parameterType.EqualsEx(argType))
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

                            if (argType.IsValueTypeUnified())
                            {
                                valid = false;
                                break;
                            }

                            ++usageCount;
                            notExactlyEqual += NotExactlyEqualUnsafeCastWeight;
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

            if (result != -1 && args != null && resultNotExactlyEqual >= NotExactlyEqualUnsafeCastWeight && args.All(data => !data.IsLambda))
                return -1;

            return result;
        }

        private static bool CheckParamsCompatible(int startIndex, int lastIndex, IBindingParameterInfo[] parameters, in MethodData method, ref float notExactlyEqual)
        {
            float weight = 0;
            var elementType = parameters[lastIndex].ParameterType.GetElementType();
            for (var i = startIndex; i < method.ExpectedParameterCount; i++)
            {
                var argType = method.GetExpectedParameterType(i);
                if (elementType.EqualsEx(argType))
                    continue;
                if (argType.IsCompatibleWith(elementType, out var boxRequired))
                {
                    var w = boxRequired ? NotExactlyEqualBoxWeight : NotExactlyEqualWeight;
                    if (w > weight)
                        weight = w;
                }
                else
                {
                    if (argType.IsValueTypeUnified())
                        return false;
                    if (NotExactlyEqualUnsafeCastWeight > weight)
                        weight = NotExactlyEqualUnsafeCastWeight;
                }
            }

            notExactlyEqual += weight;
            return true;
        }

        private static Expression[] ConvertParameters(ExpressionCompilerComponent.IContext context, in MethodData method, bool hasParams)
        {
            var parameters = method.Parameters;
            var args = (Expression[])method.Args!;
            var result = new Expression[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                //optional or params
                if (i > args.Length - 1)
                {
                    for (var j = i; j < parameters.Length; j++)
                    {
                        var parameter = parameters[j];
                        if (j == parameters.Length - 1 && hasParams)
                        {
                            var type = parameter.ParameterType.GetElementType();
                            result[j] = Expression.NewArrayInit(type, Default.EmptyArray<Expression>());
                        }
                        else
                        {
                            if (parameter.ParameterType == typeof(IReadOnlyMetadataContext))
                                result[j] = context.MetadataParameter;
                            else
                                result[j] = Expression.Constant(parameter.DefaultValue).ConvertIfNeed(parameter.ParameterType, false);
                        }
                    }

                    break;
                }

                if (i == parameters.Length - 1 && hasParams && !args[i].Type.IsCompatibleWith(parameters[i].ParameterType))
                {
                    var arrayType = parameters[i].ParameterType.GetElementType();
                    var arrayArgs = new Expression[args.Length - i];
                    for (var j = i; j < args.Length; j++)
                        arrayArgs[j - i] = args[j].ConvertIfNeed(arrayType, false);
                    result[i] = Expression.NewArrayInit(arrayType, arrayArgs);
                }
                else
                    result[i] = args[i].ConvertIfNeed(parameters[i].ParameterType, false);
            }

            return result;
        }

        private static object?[] ConvertParameters(in MethodData method, object?[] args, bool hasParams, IReadOnlyMetadataContext? metadata)
        {
            var parameters = method.Parameters!;
            var result = args.Length == parameters.Length ? args : new object?[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                //optional or params
                if (i > args.Length - 1)
                {
                    for (var j = i; j < parameters.Length; j++)
                    {
                        var parameter = parameters[j];
                        if (j == parameters.Length - 1 && hasParams)
                        {
                            ArraySize[0] = 0;
                            result[j] = Array.CreateInstance(parameter.ParameterType.GetElementType(), ArraySize);
                        }
                        else
                        {
                            if (parameter.ParameterType == typeof(IReadOnlyMetadataContext))
                                result[j] = metadata;
                            else
                                result[j] = parameter.DefaultValue;
                        }
                    }

                    break;
                }

                if (i == parameters.Length - 1 && hasParams && !parameters[i].ParameterType.IsInstanceOfTypeUnified(args[i]))
                {
                    ArraySize[0] = args.Length - i;
                    var array = Array.CreateInstance(parameters[i].ParameterType.GetElementType(), ArraySize);
                    for (var j = i; j < args.Length; j++)
                    {
                        ArraySize[0] = j - i;
                        array.SetValue(args[j], ArraySize);
                    }

                    result[i] = array;
                }
                else
                    result[i] = args[i];
            }

            return result;
        }

        private static ArgumentData[] GetMethodArgs(bool isExtensionMethod, in TargetData target, ArgumentData[] args)
        {
            if (!isExtensionMethod || target.IsStatic)
                return args;

            var result = new ArgumentData[args.Length + 1];
            result[0] = new ArgumentData(target.Node, target.Expression, target.Type);
            Array.Copy(args, 0, result, 1, args.Length);
            return result;
        }

        private static MethodData TryInferMethod(IBindingMethodInfo method, ArgumentData[] args, Type[] typeArgs)
        {
            var m = ApplyTypeArgs(method, typeArgs);
            if (m != null)
                return new MethodData(m);
            if (!method.IsGenericMethod || typeArgs.Length != 0)
                return default;
            return TryInferGenericMethod(method, args);
        }

        private static MethodData TryInferGenericMethod(IBindingMethodInfo method, ArgumentData[] args)
        {
            var genericMethod = TryInferGenericMethod(method, args, out var hasUnresolved);
            if (genericMethod == null)
                return default;
            if (hasUnresolved)
                return new MethodData(genericMethod, method);
            return new MethodData(genericMethod);
        }

        private static IBindingMethodInfo? TryInferGenericMethod(IBindingMethodInfo method, ArgumentData[] args, out bool hasUnresolved)
        {
            hasUnresolved = false;
            var parameters = method.GetParameters();
            var count = parameters.Length > args.Length ? args.Length : parameters.Length;

            var genericArguments = method.GetGenericArguments();
            var inferredTypes = new Type[genericArguments.Length];
            for (var i = 0; i < genericArguments.Length; i++)
            {
                var argument = genericArguments[i];
                Type? inferred = null;
                for (var index = 0; index < count; index++)
                {
                    var parameter = parameters[index];
                    var arg = args[index];
                    if (arg.Type != null)
                    {
                        inferred = TryInferParameter(parameter.ParameterType, argument, arg.Type);
                        if (inferred != null)
                            break;
                    }
                }

                if (inferred == null)
                {
                    inferred = argument;
                    hasUnresolved = true;
                }

                inferredTypes[i] = inferred ?? argument;
            }

            for (var i = 0; i < genericArguments.Length; i++)
            {
                var inferredType = inferredTypes[i];
                var arg = genericArguments[i];
                if (ReferenceEquals(inferredType, arg))
                    continue;
                if (!IsCompatible(inferredType, arg.GetGenericParameterAttributesUnified()))
                    return null;
                var constraints = arg.GetGenericParameterConstraintsUnified();
                for (var j = 0; j < constraints.Length; j++)
                {
                    if (!constraints[j].IsAssignableFromUnified(inferredType))
                        return null;
                }
            }

            return method.MakeGenericMethod(inferredTypes);
        }

        private static Type? TryInferParameter(Type source, Type argumentType, Type inputType)
        {
            if (source == argumentType)
                return inputType;
            if (source.IsArray)
                return inputType.IsArray ? inputType.GetElementType() : null;

            if (source.IsGenericTypeUnified())
            {
                inputType = FindCommonType(source.GetGenericTypeDefinition(), inputType)!;
                if (inputType == null)
                    return null;

                var srcArgs = source.GetGenericArgumentsUnified();
                var inputArgs = inputType.GetGenericArgumentsUnified();
                for (var index = 0; index < srcArgs.Length; index++)
                {
                    var parameter = TryInferParameter(srcArgs[index], argumentType, inputArgs[index]);
                    if (parameter != null)
                        return parameter;
                }
            }

            return null;
        }

        private static IBindingMethodInfo? ApplyTypeArgs(IBindingMethodInfo m, Type[] typeArgs)
        {
            if (typeArgs.Length == 0)
            {
                if (!m.IsGenericMethodDefinition)
                    return m;
            }
            else if (m.IsGenericMethodDefinition && m.GetGenericArguments().Length == typeArgs.Length)
                return m.MakeGenericMethod(typeArgs);

            return null;
        }

        private static Type? FindCommonType(Type genericDefinition, Type type)
        {
            foreach (var baseType in BindingMugenExtensions.SelfAndBaseTypes(type))
            {
                if (baseType.IsGenericTypeUnified() && baseType.GetGenericTypeDefinition() == genericDefinition)
                    return baseType;
            }

            return null;
        }

        private static bool IsCompatible(Type parameterType, IExpressionNode node)
        {
            if (!(node is ILambdaExpressionNode lambdaExpressionNode))
                return true;

            if (typeof(Expression).IsAssignableFromUnified(parameterType) && parameterType.IsGenericTypeUnified())
                parameterType = parameterType.GetGenericArgumentsUnified().First();
            if (!typeof(Delegate).IsAssignableFromUnified(parameterType))
                return false;

            var method = parameterType.GetMethodUnified(nameof(Action.Invoke), MemberFlags.Public | MemberFlags.Instance);
            if (method == null || method.GetParameters().Length != lambdaExpressionNode.Parameters.Count)
                return false;
            return true;
        }

        private static Expression[] ToExpressions(ExpressionCompilerComponent.IContext context, IReadOnlyList<IExpressionNode> args, IBindingMethodInfo? method, Type? convertType)
        {
            var parameters = method?.GetParameters();
            var expressions = new Expression[args.Count];
            for (var i = 0; i < expressions.Length; i++)
            {
                var expression = context.Build(args[i]);
                if (convertType != null)
                    expression = expression.ConvertIfNeed(convertType, true);
                else if (parameters != null && parameters.Length > i)
                    expression = expression.ConvertIfNeed(parameters[i].ParameterType, true);
                expressions[i] = expression;
            }
            return expressions;
        }

        private static bool IsCompatible(Type type, GenericParameterAttributes attributes)
        {
            if (attributes.HasFlagEx(GenericParameterAttributes.ReferenceTypeConstraint) && type.IsValueTypeUnified())
                return false;
            if (attributes.HasFlagEx(GenericParameterAttributes.NotNullableValueTypeConstraint) && !type.IsValueTypeUnified())
                return false;
            return true;
        }

        private MethodData[] GetMethods(Type type, string methodName, bool isStatic, Type[]? typeArgs, IReadOnlyMetadataContext? metadata)
        {
            var members = _memberProvider
                .ServiceIfNull()
                .GetMembers(type, methodName, BindingMemberType.Method, isStatic ? MemberFlags & ~MemberFlags.Instance : MemberFlags, metadata);

            var count = 0;
            for (var i = 0; i < members.Count; i++)
            {
                if (members[i] is IBindingMethodInfo method && (isStatic || method.AccessModifiers.HasFlagEx(MemberFlags.Instance) || method.IsExtensionMethod))
                    ++count;
            }

            if (count == 0)
                return Default.EmptyArray<MethodData>();

            var methods = new MethodData[count];
            count = 0;
            for (var i = 0; i < members.Count; i++)
            {
                if (members[i] is IBindingMethodInfo method && (isStatic || method.AccessModifiers.HasFlagEx(MemberFlags.Instance) || method.IsExtensionMethod))
                {
                    var m = typeArgs == null ? method : ApplyTypeArgs(method, typeArgs);
                    if (m != null)
                        methods[count++] = new MethodData(m);
                }
            }

            return methods;
        }

        #endregion

        #region Nested types

        [Preserve(AllMembers = true, Conditional = true)]
        private sealed class MethodInvoker : LightDictionary<Type[], MethodData>
        {
            #region Fields

            private readonly MethodIndexerExpressionBuilderComponent _component;
            private Type? _type;

            #endregion

            #region Constructors

            public MethodInvoker(MethodIndexerExpressionBuilderComponent component) : base(3)
            {
                _component = component;
            }

            #endregion

            #region Methods

            public object? Invoke(object target, string methodName, object?[] args, Type[] typeArgs, IReadOnlyMetadataContext? metadata)
            {
                var type = target.GetType();
                var types = GetArgTypes(args);
                if (!type.EqualsEx(_type) || TryGetValue(types, out var method))
                {
                    _type = type;
                    var methods = _component.GetMethods(type, methodName, false, typeArgs, metadata);
                    Type[]? instanceArgs = null, extArgs = null;
                    for (var i = 0; i < methods.Length; i++)
                        methods[i] = methods[i].WithArgs(target, args, ref instanceArgs, ref extArgs);
                    var resultIndex = TrySelectMethod(methods, null, out _);
                    method = resultIndex >= 0 ? methods[resultIndex] : default;
                    this[types] = method;
                }

                if (method.IsEmpty)
                    BindingExceptionManager.ThrowInvalidBindingMember(type, methodName);
                if (method.Method.IsExtensionMethod)
                {
                    var newArgs = new object?[args.Length + 1];
                    newArgs[0] = target;
                    Array.Copy(args, 0, newArgs, 1, args.Length);
                    args = newArgs;
                }

                return method.Method.Invoke(target, ConvertParameters(method, args, method.Parameters.LastOrDefault()?.IsParamsArray ?? false, metadata), metadata);
            }

            private static Type[] GetArgTypes(object?[]? args)
            {
                if (args == null || args.Length == 0)
                    return Default.EmptyArray<Type>();
                var result = new Type[args.Length];
                for (var i = 0; i < args.Length; i++)
                {
                    var o = args[i];
                    result[i] = o == null ? typeof(object) : o.GetType();
                }

                return result;
            }

            protected override bool Equals(Type[] x, Type[] y)
            {
                if (x.Length != y.Length)
                    return false;
                for (var i = 0; i < x.Length; i++)
                {
                    if (x[i] != y[i])
                        return false;
                }

                return true;
            }

            protected override int GetHashCode(Type[] key)
            {
                var hash = 0;
                unchecked
                {
                    for (var index = 0; index < key.Length; index++)
                        hash = hash * 397 ^ key[index].GetHashCode();
                }

                return hash;
            }

            #endregion
        }

        [StructLayout(LayoutKind.Auto)]
        private readonly struct TargetData
        {
            #region Fields

            public readonly Expression? Expression;
            public readonly IExpressionNode Node;
            public readonly Type Type;

            #endregion

            #region Constructors

            public TargetData(IExpressionNode node, Type type, Expression? expression)
            {
                Type = type;
                Node = node;
                Expression = expression;
            }

            #endregion

            #region Properties

            public bool IsStatic => Expression == null;

            #endregion
        }

        [StructLayout(LayoutKind.Auto)]
        private readonly struct ArgumentData
        {
            #region Fields

            public readonly IExpressionNode Node;
            public readonly Expression? Expression;
            public readonly Type? Type;

            #endregion

            #region Constuctors

            public ArgumentData(IExpressionNode node, Expression? expression, Type? type)
            {
                Should.NotBeNull(node, nameof(node));
                Node = node;
                Expression = expression;
                if (type == null && expression != null)
                    type = expression.Type;
                Type = type;
            }

            #endregion

            #region Properties

            public bool IsLambda => Node.NodeType == ExpressionNodeType.Lambda;

            #endregion

            #region Methods

            public ArgumentData UpdateExpression(Expression expression)
            {
                return new ArgumentData(Node, expression, Type);
            }

            #endregion
        }

        [StructLayout(LayoutKind.Auto)]
        private readonly struct MethodData
        {
            #region Fields

            private readonly IBindingMethodInfo? _unresolvedMethod;

            #endregion

            #region Constructors

            public MethodData(IBindingMethodInfo method)
                : this(method, null)
            {
                Method = method;
            }

            public MethodData(IBindingMethodInfo method, IBindingMethodInfo? unresolvedMethod, object? args = null)
            {
                Method = method;
                Parameters = method.GetParameters();
                _unresolvedMethod = unresolvedMethod;
                Args = args;
            }

            #endregion

            #region Properties

            public bool IsEmpty => Method == null;

            public bool IsExtensionMethod => Method?.IsExtensionMethod ?? false;

            public IBindingMethodInfo Method { get; }

            public IBindingParameterInfo[] Parameters { get; }

            public object? Args { get; }

            public int ExpectedParameterCount
            {
                get
                {
                    if (Args == null)
                        return 0;
                    if (Args is Expression[] expressions)
                        return expressions.Length;
                    return ((Type[])Args!).Length;
                }
            }

            #endregion

            #region Methods

            public Type GetExpectedParameterType(int index)
            {
                if (Args is Expression[] expressions)
                    return expressions[index].Type;
                return ((Type[])Args!)[index];
            }

            public MethodData WithArgs(object target, object?[] args, ref Type[]? instanceArgs, ref Type[]? extArgs)
            {
                if (IsExtensionMethod)
                {
                    if (extArgs == null)
                    {
                        extArgs = new Type[args.Length + 1];
                        extArgs[0] = target.GetType();
                        for (var i = 0; i < args.Length; i++)
                            extArgs[i + 1] = args[i]?.GetType() ?? typeof(object);
                    }

                    return new MethodData(Method, null, extArgs);
                }

                if (instanceArgs == null)
                {
                    if (args.Length == 0)
                        instanceArgs = Default.EmptyArray<Type>();
                    else
                    {
                        instanceArgs = new Type[args.Length];
                        for (var i = 0; i < args.Length; i++)
                            instanceArgs[i] = args[i]?.GetType() ?? typeof(object);
                    }
                }

                return new MethodData(Method, null, instanceArgs);
            }

            public MethodData TryResolve(ArgumentData[] args, Expression[] expressions)
            {
                if (_unresolvedMethod == null)
                    return new MethodData(Method, null, expressions);

                var method = TryInferGenericMethod(_unresolvedMethod, args, out var unresolved);
                if (method == null || unresolved)
                    return default;
                return new MethodData(method!, null, expressions);
            }

            #endregion
        }

        #endregion
    }
}