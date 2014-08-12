#region Copyright
// ****************************************************************************
// <copyright file="CompileExpressionInvoker.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
using MugenMvvmToolkit.Binding.Core;
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Interfaces.Parse;
using MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Parse.Nodes;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Parse
{
    public class CompileExpressionInvoker : IExpressionInvoker
    {
        #region Nested types

        [StructLayout(LayoutKind.Auto)]
        private struct CacheKey
        {
            #region Fields

            public readonly int _hash;
            public readonly Type[] _types;

            #endregion

            #region Constructors

            public CacheKey(Type[] types)
            {
                _types = types;
                _hash = 0;
                unchecked
                {
                    for (int index = 0; index < types.Length; index++)
                    {
                        Type type = types[index];
                        _hash += type == null ? 0 : type.GetHashCode();
                    }
                }
            }

            #endregion
        }

        private sealed class CacheKeyComparer : IEqualityComparer<CacheKey>
        {
            #region Fields

            public static readonly CacheKeyComparer Instance = new CacheKeyComparer();

            #endregion

            #region Implementation of IEqualityComparer<in CacheKey>

            bool IEqualityComparer<CacheKey>.Equals(CacheKey x, CacheKey y)
            {
                if (x._types.Length != y._types.Length)
                    return false;
                for (int i = 0; i < x._types.Length; i++)
                {
                    if (x._types[i] != y._types[i])
                        return false;
                }
                return true;
            }

            int IEqualityComparer<CacheKey>.GetHashCode(CacheKey obj)
            {
                return obj._hash;
            }

            #endregion
        }

        #endregion

        #region Fields

        private static readonly MethodInfo ProxyMethod = typeof(CompileExpressionInvoker)
            .GetMethodEx("InvokeDynamicMethod", MemberFlags.Instance | MemberFlags.NonPublic);
        protected static readonly ParameterExpression DataContextParameter = Expression.Parameter(typeof(IDataContext), "dataContext");

        private readonly Dictionary<ExpressionNodeType, Func<IExpressionNode, Expression>> _nodeToExpressionMapping;
        private readonly Dictionary<TokenType, Func<Expression, Expression>> _unaryToExpressionMapping;
        private readonly Dictionary<TokenType, Func<Expression, Expression, Expression>> _binaryToExpressionMapping;
        private readonly ConstantExpression _thisExpression;

        private readonly Dictionary<string, Expression> _lambdaParameters;
        private readonly Dictionary<CacheKey, Func<object[], object>> _expressionCache;
        private readonly IExpressionNode _node;
        private readonly bool _isEmpty;
        private ParameterInfo _lambdaParameter;
        private IDataContext _dataContext;
        private IList<KeyValuePair<int, ParameterExpression>> _parameters;
        private IList<object> _sourceValues;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="CompileExpressionInvoker" /> class.
        /// </summary>
        public CompileExpressionInvoker([NotNull] IExpressionNode node, IList<KeyValuePair<string, BindingMemberExpressionNode>> members, bool isEmpty)
        {
            Should.NotBeNull(node, "node");
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
                {TokenType.DoubleEqual, (expression, expression1) => GenerateEqualityExpression(expression, expression1, Expression.Equal)},
                {TokenType.ExclamationEqual, (expression, expression1) => GenerateEqualityExpression(expression, expression1, Expression.NotEqual)},
                {TokenType.GreaterThan, (expression, expression1) => GenerateEqualityExpression(expression, expression1, Expression.GreaterThan)},
                {TokenType.GreaterThanEqual, (expression, expression1) => GenerateEqualityExpression(expression, expression1, Expression.GreaterThanOrEqual)},
                {TokenType.LessThan, (expression, expression1) => GenerateEqualityExpression(expression, expression1, Expression.LessThan)},
                {TokenType.LessThanEqual, (expression, expression1) => GenerateEqualityExpression(expression, expression1, Expression.LessThanOrEqual)},
                {TokenType.Equal, (expression, expression1) => ExpressionReflectionManager.Assign(expression, ExpressionReflectionManager.ConvertIfNeed(expression1, expression.Type, false))},
                {TokenType.DoubleQuestion, (expression, expression1) =>
                {
                    Convert(ref expression, ref expression1, true);
                    return Expression.Coalesce(expression, expression1);
                }}
            };
        }

        #endregion

        #region Properties

        protected Dictionary<ExpressionNodeType, Func<IExpressionNode, Expression>> NodeToExpressionMapping
        {
            get { return _nodeToExpressionMapping; }
        }

        protected Dictionary<TokenType, Func<Expression, Expression>> UnaryToExpressionMapping
        {
            get { return _unaryToExpressionMapping; }
        }

        protected Dictionary<TokenType, Func<Expression, Expression, Expression>> BinaryToExpressionMapping
        {
            get { return _binaryToExpressionMapping; }
        }

        protected ConstantExpression ThisExpression
        {
            get { return _thisExpression; }
        }

        protected IDataContext DataContext
        {
            get { return _dataContext; }
        }

        protected IList<KeyValuePair<int, ParameterExpression>> Parameters
        {
            get { return _parameters; }
        }

        protected IList<object> SourceValues
        {
            get { return _sourceValues; }
        }

        #endregion

        #region Methods

        public object Invoke(IDataContext context, IList<object> sourceValues)
        {
            var key = new CacheKey(sourceValues.ToArrayFast(o => o == null ? null : o.GetType()));
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

            var items = new object[sourceValues.Count + 1];
            items[0] = context;
            for (int i = 0; i < sourceValues.Count; i++)
                items[i + 1] = sourceValues[i];
            return expression.Invoke(items);
        }

        protected virtual object InvokeDynamicMethod(string methodName, IDataContext context, IList<Type> typeArgs, object[] items)
        {
            var resourceMethod = BindingProvider.Instance.ResourceResolver.ResolveMethod(methodName, context, false);
            if (resourceMethod != null)
                return resourceMethod.Invoke(typeArgs, items, context);

            var binding = context.GetData(BindingConstants.Binding);
            Type targetType = binding == null
                ? typeof(object)
                : binding.TargetAccessor.Source.GetPathMembers(false).LastMember.Type;
            var converter = BindingProvider.Instance.ResourceResolver.ResolveConverter(methodName, context, true);
            return converter.Convert(items[0], targetType, items.Length > 1 ? items[1] : null,
                items.Length > 2 ? (CultureInfo)items[2] : CultureInfo.CurrentCulture);
        }

        protected virtual Func<object[], object> CreateDelegate()
        {
            var expression = BuildExpression(_node);
            var @delegate = ExpressionReflectionManager
                .CreateLambdaExpression(expression, Parameters.OrderBy(pair => pair.Key).Select(pair => pair.Value).ToArray())
                .Compile();
            var methodInfo = @delegate.GetType().GetMethodEx("Invoke", MemberFlags.Public | MemberFlags.Instance);
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
                var binding = DataContext.GetData(BindingConstants.Binding);
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
            var member = type.FindPropertyOrField(expression.Member, target == null);
            return member is PropertyInfo
                ? Expression.Property(target, (PropertyInfo)member)
                : Expression.Field(target, (FieldInfo)member);
        }

        private Expression BuildLambdaExpression(ILambdaExpressionNode lambdaExpression)
        {
            if (_lambdaParameter == null)
                throw BindingExceptionManager.UnexpectedExpressionNode(lambdaExpression);

            var method = _lambdaParameter.ParameterType.GetMethodEx("Invoke", MemberFlags.Instance | MemberFlags.Public);
            if (method == null)
                throw BindingExceptionManager.UnexpectedExpressionNode(lambdaExpression);

            var parameters = method.GetParameters();
            var lambdaParameters = new ParameterExpression[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
                lambdaParameters[i] = Expression.Parameter(parameters[i].ParameterType, lambdaExpression.Parameters[i]);

            for (int index = 0; index < lambdaParameters.Length; index++)
            {
                var parameter = lambdaParameters[index];
                _lambdaParameters.Add(parameter.Name, parameter);
            }
            var expression = BuildExpression(lambdaExpression.Expression);
            for (int index = 0; index < lambdaParameters.Length; index++)
                _lambdaParameters.Remove(lambdaParameters[index].Name);
            return ExpressionReflectionManager.CreateLambdaExpression(expression, lambdaParameters);
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
                var dynamicMethod = BindingProvider
                    .Instance
                    .ResourceResolver
                    .ResolveMethod(methodCall.Method, _dataContext, false);
                if (dynamicMethod != null)
                    returnType = dynamicMethod.GetReturnType(arrayArg.Expressions.ToArrayFast(expression => expression.Type), typeArgs, DataContext);

                return ExpressionReflectionManager.ConvertIfNeed(Expression.Call(_thisExpression, ProxyMethod, Expression.Constant(methodCall.Method),
                            DataContextParameter, Expression.Constant(typeArgs, typeof(IList<Type>)), arrayArg), returnType, false);
            }

            var target = BuildExpression(methodCall.Target);
            var type = GetTargetType(ref target);
            var targetData = new ArgumentData(methodCall.Target, target, type);
            var args = methodCall
                .Arguments
                .ToArrayFast(node => new ArgumentData(node, node.NodeType == ExpressionNodeType.Lambda ? null : BuildExpression(node), null));

            var method = targetData
                .FindMethod(methodCall.Method, typeArgs, args, BindingProvider.Instance.ResourceResolver.GetKnownTypes(), target == null);
            return GenerateMethodCall(method, targetData, args);
        }

        private Expression BuildIndex(IIndexExpressionNode indexer)
        {
            if (indexer.Object == null)
                throw BindingExceptionManager.UnexpectedExpressionNode(indexer);
            var target = BuildExpression(indexer.Object);
            if (target.Type.IsArray)
                return Expression.ArrayIndex(target, indexer.Arguments.Select(BuildExpression));

            var type = GetTargetType(ref target);
            var targetData = new ArgumentData(indexer.Object, target, type);
            var args = indexer
                .Arguments
                .ToArrayFast(node => new ArgumentData(node, node.NodeType == ExpressionNodeType.Lambda ? null : BuildExpression(node), null));

            var method = targetData.FindIndexer(args, target == null);
            return GenerateMethodCall(method, targetData, args);
        }

        private Expression GenerateMethodCall(MethodData method, ArgumentData target, IList<ArgumentData> args)
        {
            args = BindingReflectionExtensions.GetMethodArgs(method.IsExtensionMethod, target, args);
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
            expressions = ConvertParameters(methodInfo, expressions);
            if (method.IsExtensionMethod)
                return Expression.Call(null, methodInfo, expressions);
            return Expression.Call(target.Expression, methodInfo, expressions);
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
            return Expression.Call(null, typeof(string).GetMethodEx("Concat", new[] { typeof(object), typeof(object) }),
                new[] { ExpressionReflectionManager.ConvertIfNeed(left, typeof(object), false), ExpressionReflectionManager.ConvertIfNeed(right, typeof(object), false) });
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

        private static void Convert(ref Expression left, ref Expression right, bool exactly)
        {
            if (left.Type.Equals(right.Type))
                return;
            if (left.Type.IsCompatibleWith(right.Type))
                left = ExpressionReflectionManager.ConvertIfNeed(left, right.Type, exactly);
            else if (right.Type.IsCompatibleWith(left.Type))
                right = ExpressionReflectionManager.ConvertIfNeed(right, left.Type, exactly);
        }

        private static Expression[] ConvertParameters(MethodBase method, Expression[] args)
        {
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length == 1 && parameters[0].IsDefined(typeof(ParamArrayAttribute), true))
            {
                var elementType = parameters[0].ParameterType.GetElementType();
                var initializers = new Expression[args.Length];
                for (int i = 0; i < args.Length; i++)
                    initializers[i] = ExpressionReflectionManager.ConvertIfNeed(args[i], elementType, false);
                var array = Expression.NewArrayInit(elementType, initializers);
                return new Expression[] { array };
            }
            for (int index = 0; index < args.Length; index++)
            {
                Expression expression = args[index];
                ParameterInfo parameter = parameters[index];
                args[index] = ExpressionReflectionManager.ConvertIfNeed(expression, parameter.ParameterType, false);
            }
            return args;
        }

        private Type[] GetTypes(IList<string> types)
        {
            if (types.IsNullOrEmpty())
                return EmptyValue<Type>.ArrayInstance;
            var resolver = BindingProvider.Instance.ResourceResolver;
            var typeArgs = new Type[types.Count];
            for (int i = 0; i < types.Count; i++)
                typeArgs[i] = resolver.ResolveType(types[i], _dataContext, true);
            return typeArgs;
        }

        #endregion
    }
}