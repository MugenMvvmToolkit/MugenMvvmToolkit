#region Copyright

// ****************************************************************************
// <copyright file="CompiledExpressionInvoker.cs">
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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using MugenMvvmToolkit.Attributes;
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Interfaces.Parse;
using MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Parse.Nodes;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Parse
{
    public class CompiledExpressionInvoker : IExpressionInvoker
    {
        #region Nested types

        [StructLayout(LayoutKind.Auto)]
        public struct CacheKey
        {
            #region Fields

            public int Hash;
            public Type[] Types;

            #endregion

            #region Constructors

            public CacheKey(Type[] types)
            {
                Types = types;
                Hash = 0;
                unchecked
                {
                    for (int index = 0; index < types.Length; index++)
                    {
                        Type type = types[index];
                        Hash += type == null ? 0 : type.GetHashCode();
                    }
                }
            }

            #endregion
        }

        [Preserve(AllMembers = true)]
        public sealed class MethodInvoker : Dictionary<CacheKey, Func<object[], object>>
        {
            #region Fields

            private Type _type;

            #endregion

            #region Methods

            public object InvokeIndex(object target, object[] args)
            {
                return InvokeMethodInternal(target, ReflectionExtensions.IndexerName, args, null);
            }

            public object Invoke(object target, string methodName, object[] args, Type[] typeArgs)
            {
                return InvokeMethodInternal(target, methodName, args, typeArgs);
            }

            private object InvokeMethodInternal(object target, string methodName, object[] args, Type[] typeArgs)
            {
                var type = target.GetType();
                var argTypes = GetArgTypes(args);
                var key = new CacheKey(argTypes);
                Func<object[], object> result = null;
                lock (this)
                {
                    if (type == _type)
                        TryGetValue(key, out result);
                }
                if (result != null)
                    return result(BindingReflectionExtensions.InsertFirstArg(args, target));

                List<MethodInfo> methods;
                if (methodName == ReflectionExtensions.IndexerName)
                {
                    methods = new List<MethodInfo>();
                    foreach (var property in type.GetPropertiesEx(MemberFlags.Public | MemberFlags.Instance))
                    {
                        if (property.GetIndexParameters().Length == args.Length)
                            methods.AddIfNotNull(property.GetGetMethod(true));
                    }
                }
                else
                {
                    methods = BindingReflectionExtensions.GetExtensionsMethods(methodName, BindingServiceProvider.ResourceResolver.GetKnownTypes());
                    foreach (var method in type.GetMethodsEx(MemberFlags.Public | MemberFlags.Instance))
                    {
                        try
                        {
                            if (method.Name == methodName)
                                methods.AddIfNotNull(BindingReflectionExtensions.ApplyTypeArgs(method, typeArgs));
                        }
                        catch
                        {
                            ;
                        }
                    }
                }
                bool hasParams;
                var resultIndex = TrySelectMethod(methods, argTypes, (i, types) => types, out hasParams);
                if (resultIndex >= 0)
                {
                    var method = methods[resultIndex];
                    var parameters = argTypes.Select(t => (Expression)Expression.Parameter(t)).ToList();
                    var argExpressions = ConvertParameters(method.GetParameters(), parameters, hasParams);
                    parameters.Insert(0, Expression.Parameter(type));
                    var compile = Expression.Lambda(Expression.Call(parameters[0], method, argExpressions), parameters.Cast<ParameterExpression>()).Compile();
                    var methodInfo = compile.GetType().GetMethodEx(nameof(Action.Invoke), MemberFlags.Public | MemberFlags.Instance);
                    if (methodInfo == null)
                        result = compile.DynamicInvoke;
                    else
                    {
                        var del = ServiceProvider.ReflectionManager.GetMethodDelegate(methodInfo);
                        result = objects => del(compile, objects);
                    }
                }
                lock (this)
                {
                    _type = type;
                    this[key] = result;
                }
                if (result == null)
                    throw BindingExceptionManager.InvalidBindingMember(target.GetType(), methodName);
                return result(BindingReflectionExtensions.InsertFirstArg(args, target));
            }

            private static Type[] GetArgTypes(object[] args)
            {
                if (args == null || args.Length == 0)
                    return Empty.Array<Type>();
                var result = new Type[args.Length];
                for (int i = 0; i < args.Length; i++)
                {
                    var o = args[i];
                    result[i] = o == null ? typeof(object) : o.GetType();
                }
                return result;
            }

            #endregion
        }

        private sealed class CacheKeyComparer : IEqualityComparer<CacheKey>
        {
            #region Fields

            public static readonly CacheKeyComparer Instance;

            #endregion

            #region Constructors

            static CacheKeyComparer()
            {
                Instance = new CacheKeyComparer();
            }

            private CacheKeyComparer()
            {
            }

            #endregion

            #region Implementation of IEqualityComparer<in CacheKey>

            bool IEqualityComparer<CacheKey>.Equals(CacheKey x, CacheKey y)
            {
                var xTypes = x.Types;
                var yTypes = y.Types;
                if (xTypes.Length != yTypes.Length)
                    return false;
                for (int i = 0; i < xTypes.Length; i++)
                {
                    if (xTypes[i] != yTypes[i])
                        return false;
                }
                return true;
            }

            int IEqualityComparer<CacheKey>.GetHashCode(CacheKey obj)
            {
                return obj.Hash;
            }

            #endregion
        }

        #endregion

        #region Fields

        private const float NotExactlyEqualWeight = 1f;
        private const float NotExactlyEqualBoxWeight = 1.1f;
        private const float NotExactlyEqualUnsafeCastWeight = 1000f;

        private static readonly MethodInfo ProxyMethod;
        private static readonly MethodInfo BindingMemberGetValueMethod;
        private static readonly MethodInfo GetMemberValueDynamicMethod;
        private static readonly MethodInfo GetIndexValueDynamicMethod;
        private static readonly MethodInfo InvokeMemberDynamicMethod;
        private static readonly MethodInfo EqualsMethod;
        private static readonly Expression EmptyObjectArrayExpression;
        private static readonly MethodInfo StringConcatMethod;
        protected static readonly ParameterExpression DataContextParameter;

        private readonly Dictionary<ExpressionNodeType, Func<IExpressionNode, Expression>> _nodeToExpressionMapping;
        private readonly Dictionary<TokenType, Func<Expression, Expression>> _unaryToExpressionMapping;
        private readonly Dictionary<TokenType, Func<Expression, Expression, Expression>> _binaryToExpressionMapping;
        private readonly Dictionary<string, Expression> _lambdaParameters;
        private readonly Dictionary<CacheKey, Func<object[], object>> _expressionCache;

        private readonly ConstantExpression _thisExpression;
        private readonly IExpressionNode _node;
        private readonly bool _isEmpty;
        private ParameterInfo _lambdaParameter;
        private IDataContext _dataContext;
        private IList<KeyValuePair<int, ParameterExpression>> _parameters;
        private IList<object> _sourceValues;

        #endregion

        #region Constructors

        static CompiledExpressionInvoker()
        {
            StringConcatMethod = typeof(string).GetMethodEx(nameof(string.Concat), new[] { typeof(object), typeof(object) }, MemberFlags.Public | MemberFlags.Static);
            ProxyMethod = typeof(CompiledExpressionInvoker).GetMethodEx(nameof(InvokeDynamicMethod), MemberFlags.Instance | MemberFlags.Public);
            DataContextParameter = Expression.Parameter(typeof(IDataContext), "dataContext");
            BindingMemberGetValueMethod = typeof(IBindingMemberInfo).GetMethodEx(nameof(IBindingMemberInfo.GetValue), new[] { typeof(object), typeof(object[]) }, MemberFlags.Public | MemberFlags.Instance);
            GetMemberValueDynamicMethod = typeof(CompiledExpressionInvoker).GetMethodEx(nameof(GetMemberValueDynamic), MemberFlags.Static | MemberFlags.Public);
            GetIndexValueDynamicMethod = typeof(CompiledExpressionInvoker).GetMethodEx(nameof(GetIndexValueDynamic), MemberFlags.Static | MemberFlags.Public);
            InvokeMemberDynamicMethod = typeof(CompiledExpressionInvoker).GetMethodEx(nameof(InvokeMemberDynamic), MemberFlags.Static | MemberFlags.Public);
            EqualsMethod = typeof(object).GetMethodEx(nameof(Equals), MemberFlags.Public | MemberFlags.Static);
            EmptyObjectArrayExpression = Expression.Constant(Empty.Array<object>(), typeof(object[]));
            SupportCoalesceExpression = true;
            Should.BeSupported(BindingMemberGetValueMethod != null, nameof(BindingMemberGetValueMethod));
            Should.BeSupported(GetMemberValueDynamicMethod != null, nameof(GetMemberValueDynamicMethod));
            Should.BeSupported(GetIndexValueDynamicMethod != null, nameof(GetIndexValueDynamicMethod));
            Should.BeSupported(InvokeMemberDynamicMethod != null, nameof(InvokeMemberDynamicMethod));
            Should.BeSupported(EqualsMethod != null, nameof(EqualsMethod));
        }

        public CompiledExpressionInvoker([NotNull] IExpressionNode node, bool isEmpty)
        {
            Should.NotBeNull(node, nameof(node));
            _node = node;
            _isEmpty = isEmpty;
            _expressionCache = new Dictionary<CacheKey, Func<object[], object>>(CacheKeyComparer.Instance);
            _lambdaParameters = new Dictionary<string, Expression>();

            _thisExpression = Expression.Constant(this);
            _nodeToExpressionMapping = new Dictionary<ExpressionNodeType, Func<IExpressionNode, Expression>>
            {
                {ExpressionNodeType.Binary, expressionNode => BuildBinary((IBinaryExpressionNode) expressionNode)},
                {ExpressionNodeType.Condition, expressionNode => BuildCondition((IConditionExpressionNode) expressionNode)},
                {ExpressionNodeType.Constant, expressionNode => BuildConstant((IConstantExpressionNode) expressionNode)},
                {ExpressionNodeType.Index, expressionNode => BuildIndex((IIndexExpressionNode) expressionNode)},
                {ExpressionNodeType.Member, expressionNode => BuildMemberExpression((IMemberExpressionNode) expressionNode)},
                {ExpressionNodeType.MethodCall, expressionNode => BuildMethodCall((IMethodCallExpressionNode) expressionNode)},
                {ExpressionNodeType.Unary, expressionNode => BuildUnary((IUnaryExressionNode) expressionNode)},
                {ExpressionNodeType.BindingMember, expressionNode => BuildBindingMember((BindingMemberExpressionNode) expressionNode)},
                {ExpressionNodeType.Lambda, expressionNode => BuildLambdaExpression((ILambdaExpressionNode) expressionNode)}
            };
            _unaryToExpressionMapping = new Dictionary<TokenType, Func<Expression, Expression>>
            {
                {TokenType.Minus, Expression.Negate},
                {TokenType.Exclamation, Expression.Not},
                {TokenType.Tilde, Expression.Not}
            };
            _binaryToExpressionMapping = new Dictionary<TokenType, Func<Expression, Expression, Expression>>
            {
                {TokenType.Asterisk, (expression, expression1) => GenerateNumericalExpression(expression, expression1, Expression.Multiply)},
                {TokenType.Slash, (expression, expression1) => GenerateNumericalExpression(expression, expression1, Expression.Divide)},
                {TokenType.Minus, (expression, expression1) => GenerateNumericalExpression(expression, expression1, Expression.Subtract)},
                {TokenType.Percent, (expression, expression1) => GenerateNumericalExpression(expression, expression1, Expression.Modulo)},
                {TokenType.Plus, GeneratePlusExpression},
                {TokenType.Amphersand, (expression, expression1) => GenerateBooleanExpression(expression, expression1, Expression.And)},
                {TokenType.DoubleAmphersand, (expression, expression1) => GenerateBooleanExpression(expression, expression1, Expression.AndAlso)},
                {TokenType.Bar, (expression, expression1) => GenerateBooleanExpression(expression, expression1, Expression.Or)},
                {TokenType.DoubleBar, (expression, expression1) => GenerateBooleanExpression(expression, expression1, Expression.OrElse)},
                {TokenType.DoubleEqual, GenerateEqual},
                {TokenType.ExclamationEqual, (expression, expression1) => Expression.Not(GenerateEqual(expression, expression1))},
                {TokenType.GreaterThan, (expression, expression1) => GenerateEqualityExpression(expression, expression1, Expression.GreaterThan)},
                {TokenType.GreaterThanEqual, (expression, expression1) => GenerateEqualityExpression(expression, expression1, Expression.GreaterThanOrEqual)},
                {TokenType.LessThan, (expression, expression1) => GenerateEqualityExpression(expression, expression1, Expression.LessThan)},
                {TokenType.LessThanEqual, (expression, expression1) => GenerateEqualityExpression(expression, expression1, Expression.LessThanOrEqual)},
                {TokenType.Equal, (expression, expression1) => Expression.Assign(expression, ExpressionReflectionManager.ConvertIfNeed(expression1, expression.Type, false))},
                {TokenType.DoubleQuestion, (expression, expression1) =>
                {
                    Convert(ref expression, ref expression1, true);
                    if (SupportCoalesceExpression)
                        return Expression.Coalesce(expression, expression1);
                    return Expression.Condition(Expression.Equal(expression, Expression.Constant(null, expression.Type)), expression1, expression);
                }}
            };
        }

        #endregion

        #region Properties

        public static bool SupportCoalesceExpression { get; set; }

        protected Dictionary<ExpressionNodeType, Func<IExpressionNode, Expression>> NodeToExpressionMapping => _nodeToExpressionMapping;

        protected Dictionary<TokenType, Func<Expression, Expression>> UnaryToExpressionMapping => _unaryToExpressionMapping;

        protected Dictionary<TokenType, Func<Expression, Expression, Expression>> BinaryToExpressionMapping => _binaryToExpressionMapping;

        protected ConstantExpression ThisExpression => _thisExpression;

        protected IDataContext DataContext => _dataContext;

        protected IList<KeyValuePair<int, ParameterExpression>> Parameters => _parameters;

        protected IList<object> SourceValues => _sourceValues;

        #endregion

        #region Methods

        public object Invoke(IDataContext context, IList<object> sourceValues)
        {
            var key = new CacheKey(sourceValues.ToArrayEx(o => o == null ? null : o.GetType()));
            Func<object[], object> expression;
            lock (_expressionCache)
            {
                if (!_expressionCache.TryGetValue(key, out expression))
                {
                    try
                    {
                        _sourceValues = sourceValues;
                        _parameters = new List<KeyValuePair<int, ParameterExpression>>
                        {

                            new KeyValuePair<int, ParameterExpression>(-1, DataContextParameter)
                        };
                        _dataContext = context;
                        expression = CreateDelegate();
                        _expressionCache[key] = expression;
                    }
                    finally
                    {
                        _lambdaParameter = null;
                        _lambdaParameters.Clear();
                        _sourceValues = null;
                        _parameters = null;
                        _dataContext = null;
                    }
                }
            }
            if (_isEmpty)
                return expression.Invoke(new object[] { context });
            return expression.Invoke(BindingReflectionExtensions.InsertFirstArg(sourceValues, context));
        }

        [Preserve]
        public virtual object InvokeDynamicMethod(string methodName, IDataContext context, IList<Type> typeArgs, object[] items)
        {
            var resourceMethod = BindingServiceProvider.ResourceResolver.ResolveMethod(methodName, context, false);
            if (resourceMethod != null)
                return resourceMethod.Invoke(typeArgs, items, context);

            var binding = context.GetData(BindingConstants.Binding);
            Type targetType = binding == null
                ? typeof(object)
                : binding.TargetAccessor.Source.GetPathMembers(false).LastMember.Type;
            var converter = BindingServiceProvider.ResourceResolver.ResolveConverter(methodName, context, true);
            return converter.Convert(items[0], targetType, items.Length > 1 ? items[1] : null,
                items.Length > 2 ? (CultureInfo)items[2] : BindingServiceProvider.BindingCultureInfo(), context);
        }

        protected virtual Func<object[], object> CreateDelegate()
        {
            var expression = BuildExpression(_node);
            var @delegate = Expression.Lambda(expression, Parameters.OrderBy(pair => pair.Key).Select(pair => pair.Value).ToArray()).Compile();
            var methodInfo = @delegate.GetType().GetMethodEx(nameof(Action.Invoke), MemberFlags.Public | MemberFlags.Instance);
            if (methodInfo == null)
                return @delegate.DynamicInvoke;

            var invokeMethod = ServiceProvider.ReflectionManager.GetMethodDelegate(methodInfo);
            return objects => invokeMethod(@delegate, objects);
        }

        protected Expression BuildExpression(IExpressionNode node)
        {
            Func<IExpressionNode, Expression> func;
            if (!_nodeToExpressionMapping.TryGetValue(node.NodeType, out func))
                throw BindingExceptionManager.UnexpectedExpressionNode(node);
            return func(node);
        }

        private Expression BuildBinary(IBinaryExpressionNode binary)
        {
            var left = BuildExpression(binary.Left);
            var right = BuildExpression(binary.Right);
            Func<Expression, Expression, Expression> func;
            if (!_binaryToExpressionMapping.TryGetValue(binary.Token, out func))
                throw BindingExceptionManager.UnexpectedExpressionNode(binary);
            return func(left, right);
        }

        private Expression BuildCondition(IConditionExpressionNode condition)
        {
            var ifTrue = BuildExpression(condition.IfTrue);
            var ifFalse = BuildExpression(condition.IfFalse);
            Convert(ref ifTrue, ref ifFalse, true);
            return Expression.Condition(ExpressionReflectionManager.ConvertIfNeed(BuildExpression(condition.Condition), typeof(bool), true), ifTrue,
                ifFalse);
        }

        private static Expression BuildConstant(IConstantExpressionNode constant)
        {
            var expression = constant.Value as Expression;
            if (expression != null)
                return expression;
            return Expression.Constant(constant.Value, constant.Type);
        }

        private Expression BuildUnary(IUnaryExressionNode unary)
        {
            var operand = BuildExpression(unary.Operand);
            Func<Expression, Expression> func;
            if (!_unaryToExpressionMapping.TryGetValue(unary.Token, out func))
                throw BindingExceptionManager.UnexpectedExpressionNode(unary);
            return func(operand);
        }

        private Expression BuildBindingMember(BindingMemberExpressionNode bindingMember)
        {
            var param = Parameters.FirstOrDefault(expression => expression.Value.Name == bindingMember.ParameterName);
            if (param.Value != null)
                return param.Value;

            var index = bindingMember.Index;
            if (index >= SourceValues.Count)
                throw BindingExceptionManager.BindingSourceNotFound(bindingMember);
            var parameter = SourceValues[index];

            Type type = typeof(object);
            if (parameter == null)
            {
                var binding = _dataContext.GetData(BindingConstants.Binding);
                if (binding != null)
                {
                    var last = binding.SourceAccessor.Sources[index].GetPathMembers(false).LastMember;
                    type = last.Type;
                }
            }
            else
                type = parameter.GetType();
            var exParam = Expression.Parameter(type, bindingMember.ParameterName);
            Parameters.Add(new KeyValuePair<int, ParameterExpression>(index, exParam));
            return exParam;
        }

        private Expression BuildMemberExpression(IMemberExpressionNode expression)
        {
            if (expression.Member.Contains("("))
                return BuildMethodCall(new MethodCallExpressionNode(expression.Target,
                    expression.Member.Replace("(", string.Empty).Replace(")", string.Empty), null, null));
            if (expression.Target == null)
            {
                Expression value;
                if (!_lambdaParameters.TryGetValue(expression.Member, out value))
                    throw BindingExceptionManager.UnexpectedExpressionNode(expression);
                return value;
            }

            var target = BuildExpression(expression.Target);
            var type = GetTargetType(ref target);
            var @enum = BindingReflectionExtensions.TryParseEnum(expression.Member, type);
            if (@enum != null)
                return Expression.Constant(@enum);

            if (type != null)
            {
                var bindingMember = BindingServiceProvider
                    .MemberProvider
                    .GetBindingMember(type, expression.Member, false, false);
                if (bindingMember != null)
                {
                    var methodCall = Expression.Call(Expression.Constant(bindingMember, typeof(IBindingMemberInfo)),
                        BindingMemberGetValueMethod,
                        ExpressionReflectionManager.ConvertIfNeed(target, typeof(object), false),
                        EmptyObjectArrayExpression);
                    return Expression.Convert(methodCall, bindingMember.Type);
                }
            }

            var member = type.FindPropertyOrField(expression.Member, target == null);
            //Trying to get dynamic value.
            if (member == null)
                return Expression.Call(null, GetMemberValueDynamicMethod,
                    ExpressionReflectionManager.ConvertIfNeed(target, typeof(object), false),
                    Expression.Constant(expression.Member, typeof(string)));
            return member is PropertyInfo
                ? Expression.Property(target, (PropertyInfo)member)
                : Expression.Field(target, (FieldInfo)member);
        }

        private Expression BuildLambdaExpression(ILambdaExpressionNode lambdaExpression)
        {
            if (_lambdaParameter == null)
                throw BindingExceptionManager.UnexpectedExpressionNode(lambdaExpression);

            var method = _lambdaParameter.ParameterType.GetMethodEx(nameof(Action.Invoke), MemberFlags.Instance | MemberFlags.Public);
            if (method == null)
                throw BindingExceptionManager.UnexpectedExpressionNode(lambdaExpression);

            var parameters = method.GetParameters();
            var lambdaParameters = new ParameterExpression[parameters.Length];
            try
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    var parameter = Expression.Parameter(parameters[i].ParameterType, lambdaExpression.Parameters[i]);
                    lambdaParameters[i] = parameter;
                    _lambdaParameters.Add(parameter.Name, parameter);
                }

                var expression = BuildExpression(lambdaExpression.Expression);
                return Expression.Lambda(expression, lambdaParameters);
            }
            finally
            {
                for (int index = 0; index < lambdaParameters.Length; index++)
                    _lambdaParameters.Remove(lambdaParameters[index].Name);
            }
        }

        private Expression BuildMethodCall(IMethodCallExpressionNode methodCall)
        {
            if (methodCall.Target == null)
                throw BindingExceptionManager.UnexpectedExpressionNode(methodCall);

            var typeArgs = GetTypes(methodCall.TypeArgs);
            var hasLambda = methodCall.Arguments
                .OfType<ILambdaExpressionNode>()
                .Any();
            if (methodCall.Target.NodeType == ExpressionNodeType.DynamicMember)
            {
                if (hasLambda)
                    throw BindingExceptionManager.UnexpectedExpressionNode(methodCall);
                var parameters = methodCall.Arguments
                    .Select(node => ExpressionReflectionManager.ConvertIfNeed(BuildExpression(node), typeof(object), false));
                var arrayArg = Expression.NewArrayInit(typeof(object), parameters);
                Type returnType = typeof(object);
                var dynamicMethod = BindingServiceProvider
                    .ResourceResolver
                    .ResolveMethod(methodCall.Method, _dataContext, false);
                if (dynamicMethod != null)
                    returnType = dynamicMethod.GetReturnType(arrayArg.Expressions.ToArrayEx(expression => expression.Type), typeArgs, _dataContext);

                return ExpressionReflectionManager.ConvertIfNeed(Expression.Call(_thisExpression, ProxyMethod, Expression.Constant(methodCall.Method),
                            DataContextParameter, Expression.Constant(typeArgs, typeof(IList<Type>)), arrayArg), returnType, false);
            }

            var target = BuildExpression(methodCall.Target);
            var type = GetTargetType(ref target);
            var targetData = new ArgumentData(methodCall.Target, target, type, target == null);
            var args = methodCall
                .Arguments
                .ToArrayEx(node => new ArgumentData(node, node.NodeType == ExpressionNodeType.Lambda ? null : BuildExpression(node), null, false));

            var types = new List<Type>(BindingServiceProvider.ResourceResolver.GetKnownTypes())
            {
                typeof (BindingReflectionExtensions)
            };
            var methods = targetData.FindMethod(methodCall.Method, typeArgs, args, types, target == null);
            var exp = TryGenerateMethodCall(methods, targetData, args);
            if (exp != null)
                return exp;
            var arrayArgs = new Expression[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                var data = args[i];
                if (data.IsLambda || data.Expression == null)
                    throw BindingExceptionManager.InvalidBindingMember(type, methodCall.Method);
                arrayArgs[i] = ExpressionReflectionManager.ConvertIfNeed(data.Expression, typeof(object), false);
            }
            if (target == null)
                throw BindingExceptionManager.InvalidBindingMember(type, methodCall.Method);
            return Expression.Call(InvokeMemberDynamicMethod,
                ExpressionReflectionManager.ConvertIfNeed(target, typeof(object), false),
                Expression.Constant(methodCall.Method),
                Expression.NewArrayInit(typeof(object), arrayArgs), Expression.Constant(typeArgs),
                Expression.Constant(new MethodInvoker()), DataContextParameter);
        }

        private Expression BuildIndex(IIndexExpressionNode indexer)
        {
            if (indexer.Object == null)
                throw BindingExceptionManager.UnexpectedExpressionNode(indexer);
            var target = BuildExpression(indexer.Object);
            if (target.Type.IsArray)
                return Expression.ArrayIndex(target, indexer.Arguments.Select(BuildExpression));

            var type = GetTargetType(ref target);
            var targetData = new ArgumentData(indexer.Object, target, type, target == null);
            var args = indexer
                .Arguments
                .ToArrayEx(node => new ArgumentData(node, node.NodeType == ExpressionNodeType.Lambda ? null : BuildExpression(node), null, false));

            var exp = TryGenerateMethodCall(targetData.FindIndexer(args, target == null), targetData, args);
            if (exp != null)
                return exp;
            var arrayArgs = new Expression[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                var data = args[i];
                if (data.IsLambda || data.Expression == null)
                    throw BindingExceptionManager.InvalidBindingMember(type, ReflectionExtensions.IndexerName);
                arrayArgs[i] = ExpressionReflectionManager.ConvertIfNeed(data.Expression, typeof(object), false);
            }
            return Expression.Call(GetIndexValueDynamicMethod,
                ExpressionReflectionManager.ConvertIfNeed(target, typeof(object), false),
                Expression.NewArrayInit(typeof(object), arrayArgs), Expression.Constant(new MethodInvoker()), DataContextParameter);
        }

        [CanBeNull]
        private Expression TryGenerateMethodCall(IList<MethodData> methods, ArgumentData target, IList<ArgumentData> arguments)
        {
            if (methods == null || methods.Count == 0)
                return null;

            var methodInfos = new List<MethodInfo>(methods.Count);
            var methodArgs = new List<Expression[]>(methods.Count);
            for (int i = 0; i < methods.Count; i++)
            {
                try
                {
                    var method = methods[i];
                    var args = BindingReflectionExtensions.GetMethodArgs(method.IsExtensionMethod, target, arguments);
                    var expressions = new Expression[args.Count];
                    for (int index = 0; index < args.Count; index++)
                    {
                        var data = args[index];
                        if (data.IsLambda)
                        {
                            _lambdaParameter = method.Parameters[index];
                            data.UpdateExpression(BuildExpression(data.Node));
                            _lambdaParameter = null;
                        }
                        expressions[index] = data.Expression;
                    }
                    var methodInfo = method.Build(args);
                    if (methodInfo != null)
                    {
                        methodInfos.Add(methodInfo);
                        methodArgs.Add(expressions);
                    }
                }
                catch
                {
                    ;
                }
            }

            bool resultHasParams;
            var resultIndex = TrySelectMethod(methodInfos, methodArgs, (i, args) => args[i].ToArrayEx(e => e.Type), out resultHasParams);
            if (resultIndex < 0)
                return null;
            var result = methodInfos[resultIndex];
            var resultArgs = methodArgs[resultIndex];
            var resultParameters = result.GetParameters();

            resultArgs = ConvertParameters(resultParameters, resultArgs, resultHasParams);
            return Expression.Call(result.IsExtensionMethod() ? null : target.Expression, result, resultArgs);
        }

        private static int TrySelectMethod<TArgs>(IList<MethodInfo> methods, TArgs args, Func<int, TArgs, Type[]> getArgTypes, out bool resultHasParams)
        {
            int result = -1;
            resultHasParams = true;
            bool resultUseParams = true;
            float resultNotExactlyEqual = float.MaxValue;
            int resultUsageCount = int.MinValue;
            for (int i = 0; i < methods.Count; i++)
            {
                try
                {
                    var methodInfo = methods[i];
                    var parameters = methodInfo.GetParameters();
                    bool useParams = false;
                    bool hasParams = false;
                    int lastIndex = 0;
                    int usageCount = 0;
                    if (parameters.Length != 0)
                    {
                        lastIndex = parameters.Length - 1;
                        hasParams = parameters[lastIndex].IsDefined(typeof(ParamArrayAttribute), true);
                    }

                    float notExactlyEqual = 0;
                    bool valid = true;
                    var argTypes = getArgTypes(i, args);
                    for (int j = 0; j < argTypes.Length; j++)
                    {
                        //params
                        if (j > lastIndex)
                        {
                            valid = hasParams && CheckParamsCompatible(j - 1, lastIndex, parameters, argTypes, ref notExactlyEqual);
                            useParams = true;
                            break;
                        }

                        var argType = argTypes[j];
                        var parameterType = parameters[j].ParameterType;
                        if (parameterType.IsByRef)
                            parameterType = parameterType.GetElementType();
                        if (parameterType.Equals(argType))
                        {
                            ++usageCount;
                            continue;
                        }

                        bool boxRequired;
                        if (argType.IsCompatibleWith(parameterType, out boxRequired))
                        {
                            notExactlyEqual += boxRequired ? NotExactlyEqualBoxWeight : NotExactlyEqualWeight;
                            ++usageCount;
                        }
                        else
                        {
                            if (lastIndex == j && hasParams)
                            {
                                valid = CheckParamsCompatible(j, lastIndex, parameters, argTypes, ref notExactlyEqual);
                                useParams = true;
                                break;
                            }

                            if (argType.IsValueType())
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
            return result;
        }

        private static bool CheckParamsCompatible(int startIndex, int lastIndex, ParameterInfo[] parameters, Type[] types, ref float notExactlyEqual)
        {
            float weight = 0;
            var elementType = parameters[lastIndex].ParameterType.GetElementType();
            for (int k = startIndex; k < types.Length; k++)
            {
                var argType = types[k];
                if (elementType.Equals(argType))
                    continue;
                bool boxRequired;
                if (argType.IsCompatibleWith(elementType, out boxRequired))
                {
                    var w = boxRequired ? NotExactlyEqualBoxWeight : NotExactlyEqualWeight;
                    if (w > weight)
                        weight = w;
                }
                else
                {
                    if (argType.IsValueType())
                        return false;
                    if (NotExactlyEqualUnsafeCastWeight > weight)
                        weight = NotExactlyEqualUnsafeCastWeight;
                }
            }
            notExactlyEqual += weight;
            return true;
        }

        private static Expression GeneratePlusExpression(Expression left, Expression right)
        {
            if (left.Type.Equals(typeof(string)) || right.Type.Equals(typeof(string)))
                return GenerateStringConcat(left, right);
            Convert(ref left, ref right, true);
            return Expression.Add(left, right);
        }

        private static Expression GenerateStringConcat(Expression left, Expression right)
        {
            return Expression.Call(null, StringConcatMethod, ExpressionReflectionManager.ConvertIfNeed(left, typeof(object), false),
                ExpressionReflectionManager.ConvertIfNeed(right, typeof(object), false));
        }

        private static Expression GenerateEqualityExpression(Expression left, Expression right,
            Func<Expression, Expression, Expression> getExpr)
        {
            Convert(ref left, ref right, true);
            return getExpr(left, right);
        }

        private static Expression GenerateBooleanExpression(Expression left, Expression right,
            Func<Expression, Expression, Expression> getExpr)
        {
            Convert(ref left, ref right, true);
            return getExpr(left, right);
        }

        private static Expression GenerateNumericalExpression(Expression left, Expression right, Func<Expression, Expression, Expression> getExpr)
        {
            Convert(ref left, ref right, true);
            return getExpr(left, right);
        }

        private static Type GetTargetType(ref Expression target)
        {
            var constant = target as ConstantExpression;
            Type type = target.Type;
            if (constant != null && constant.Value is Type)
            {
                type = (Type)constant.Value;
                target = null;
            }
            return type;
        }

        private static Expression[] ConvertParameters(ParameterInfo[] parameters, IList<Expression> args, bool hasParams)
        {
            var result = new Expression[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                //optional or params
                if (i > args.Count - 1)
                {
                    for (int j = i; j < parameters.Length; j++)
                    {
                        if (j == parameters.Length - 1 && hasParams)
                        {
                            var type = parameters[j].ParameterType.GetElementType();
                            result[j] = Expression.NewArrayInit(type);
                        }
                        else
                        {
                            result[j] = ExpressionReflectionManager.ConvertIfNeed(Expression.Constant(parameters[j].DefaultValue), parameters[j].ParameterType, false);
                        }
                    }
                    break;
                }

                if (i == parameters.Length - 1 && hasParams && !args[i].Type.IsCompatibleWith(parameters[i].ParameterType))
                {
                    var arrayType = parameters[i].ParameterType.GetElementType();
                    var arrayArgs = new Expression[args.Count - i];
                    for (int j = i; j < args.Count; j++)
                        arrayArgs[j - i] = ExpressionReflectionManager.ConvertIfNeed(args[j], arrayType, false);
                    result[i] = Expression.NewArrayInit(arrayType, arrayArgs);
                }
                else
                {
                    result[i] = ExpressionReflectionManager.ConvertIfNeed(args[i], parameters[i].ParameterType, false);
                }
            }
            return result;
        }

        private static void Convert(ref Expression left, ref Expression right, bool exactly)
        {
            if (left.Type.Equals(right.Type))
                return;
            if (left.Type.IsCompatibleWith(right.Type))
                left = ExpressionReflectionManager.ConvertIfNeed(left, right.Type, exactly);
            else if (right.Type.IsCompatibleWith(left.Type))
                right = ExpressionReflectionManager.ConvertIfNeed(right, left.Type, exactly);
        }

        private static Expression GenerateEqual(Expression left, Expression right)
        {
            Convert(ref left, ref right, true);
            try
            {
                return Expression.Equal(left, right);
            }
            catch
            {
                return Expression.Call(null, EqualsMethod, ExpressionReflectionManager.ConvertIfNeed(left, typeof(object), false),
                    ExpressionReflectionManager.ConvertIfNeed(right, typeof(object), false));
            }
        }

        private Type[] GetTypes(IList<string> types)
        {
            if (types.IsNullOrEmpty())
                return Empty.Array<Type>();
            var resolver = BindingServiceProvider.ResourceResolver;
            var typeArgs = new Type[types.Count];
            for (int i = 0; i < types.Count; i++)
                typeArgs[i] = resolver.ResolveType(types[i], _dataContext, true);
            return typeArgs;
        }

        [Preserve]
        public static object GetMemberValueDynamic(object target, string member)
        {
            if (target == null)
                return null;
            return BindingServiceProvider.MemberProvider.GetBindingMember(target.GetType(), member, false, true).GetValue(target, Empty.Array<object>());
        }

        [Preserve]
        public static object GetIndexValueDynamic(object target, object[] args, MethodInvoker methodInvoker, IDataContext context)
        {
            if (target == null)
                return null;
            var dynamicObject = target as IDynamicObject;
            if (dynamicObject != null)
                return dynamicObject.GetIndex(args, context);

            var bindingMember = BindingServiceProvider
                .MemberProvider
                .GetBindingMember(target.GetType(), ReflectionExtensions.IndexerName, false, false);
            if (bindingMember != null)
                return bindingMember.GetValue(target, args);
            return methodInvoker.InvokeIndex(target, args);
        }

        [Preserve]
        public static object InvokeMemberDynamic(object target, string member, object[] args, Type[] typeArgs, MethodInvoker methodInvoker, IDataContext context)
        {
            if (target == null)
                return null;
            var dynamicObject = target as IDynamicObject;
            if (dynamicObject != null)
                return dynamicObject.InvokeMember(member, args, typeArgs, context);

            var bindingMember = BindingServiceProvider
                .MemberProvider
                .GetBindingMember(target.GetType(), member, false, false);
            if (bindingMember != null)
                return bindingMember.GetValue(target, args);
            return methodInvoker.Invoke(target, member, args, typeArgs);
        }

        #endregion
    }
}
