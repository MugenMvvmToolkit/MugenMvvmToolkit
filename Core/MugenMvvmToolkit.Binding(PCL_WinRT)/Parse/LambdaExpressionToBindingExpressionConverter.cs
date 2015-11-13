#region Copyright

// ****************************************************************************
// <copyright file="LambdaExpressionToBindingExpressionConverter.cs">
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
using System.Linq;
using System.Linq.Expressions;
using MugenMvvmToolkit.Binding.Attributes;
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Extensions.Syntax;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Syntax;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Parse
{
    public sealed class LambdaExpressionToBindingExpressionConverter : ExpressionVisitor, IBuilderSyntaxContext
    {
        #region Nested types

        private sealed class ContextReplacerVisitor : ExpressionVisitor
        {
            #region Fields

            private static readonly ContextReplacerVisitor Instance;

            #endregion

            #region Constructors

            static ContextReplacerVisitor()
            {
                Instance = new ContextReplacerVisitor();
            }

            private ContextReplacerVisitor()
            {
            }

            #endregion

            #region Methods

            public static Expression UpdateContextParameter(Expression expression)
            {
                return Instance.Visit(expression);
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (typeof(IBindingSyntaxContext).IsAssignableFrom(node.Type))
                    return Expression.Constant(null, node.Type);
                return base.VisitParameter(node);
            }

            #endregion
        }

        private struct ParameterCacheValue
        {
            #region Fields

            public Action<IBindingToSyntax>[] Actions;
            public Func<IDataContext, object[], object> Expression;
            public Func<IDataContext, IObserver>[] Members;

            #endregion

            #region Constructors

            public ParameterCacheValue(Action<IBindingToSyntax>[] actions,
                Func<IDataContext, object[], object> expression, Func<IDataContext, IObserver>[] members)
            {
                Actions = actions;
                Expression = expression;
                Members = members;
            }

            #endregion
        }

        #endregion

        #region Fields

        private static readonly ParameterExpression ContextExpression;
        private static readonly Dictionary<Delegate, Action<IBindingToSyntax>[]> Cache;
        private static readonly Dictionary<Delegate, ParameterCacheValue> CacheParameter;

        private List<Action<IBindingToSyntax>> _callbacks;
        private List<KeyValuePair<ParameterExpression, Func<IDataContext, IObserver>>> _members;
        private ParameterExpression _sourceExpression;

        private Expression _currentExpression;
        private MethodCallExpression _methodExpression;

        #endregion

        #region Constructors

        static LambdaExpressionToBindingExpressionConverter()
        {
            ContextExpression = Expression.Parameter(typeof(IDataContext), "$context$");
            Cache = new Dictionary<Delegate, Action<IBindingToSyntax>[]>(ReferenceEqualityComparer.Instance);
            CacheParameter = new Dictionary<Delegate, ParameterCacheValue>(ReferenceEqualityComparer.Instance);
        }

        private LambdaExpressionToBindingExpressionConverter(LambdaExpression expression)
        {
            _sourceExpression = expression.Parameters[0];
            _callbacks = new List<Action<IBindingToSyntax>>();
            _members = new List<KeyValuePair<ParameterExpression, Func<IDataContext, IObserver>>>(2);
        }

        #endregion

        #region Overrides of ExpressionVisitor

        public override Expression Visit(Expression node)
        {
            Expression lastExpression;
            string path;
            if (BindingExtensions.TryGetMemberPath(node, ".", false, out lastExpression, out path) &&
                lastExpression != null)
            {
                if (lastExpression == _sourceExpression)
                    return GetOrAddParameterExpression(string.Empty, path, node, BindingExtensions.CreteBindingSourceDel);
                return TryGetExtensionExpressionOrDefault(lastExpression, node);
            }
            return TryGetExtensionExpressionOrDefault(lastExpression ?? node, node);
        }

        #endregion

        #region Methods

        public static void Convert(Func<LambdaExpression> expression, IBindingToSyntax syntax)
        {
            Should.NotBeNull(expression, "expression");
            var hasClosure = expression.HasClosure();
            if (hasClosure || syntax.Builder.Contains(BindingBuilderConstants.NoCache))
            {
                var ex = expression();
                if (hasClosure)
                    ex.TraceClosureWarn();
                Convert(ex, syntax);
                return;
            }
            Action<IBindingToSyntax>[] callbacks;
            lock (Cache)
            {
                if (!Cache.TryGetValue(expression, out callbacks))
                {
                    callbacks = ConvertInternal(expression(), syntax, false);
                    Cache[expression] = callbacks;
                }
            }
            for (int i = 0; i < callbacks.Length; i++)
                callbacks[i].Invoke(syntax);
        }

        public static void Convert(LambdaExpression expression, IBindingToSyntax syntax)
        {
            Should.NotBeNull(expression, "expression");
            var callbacks = ConvertInternal(expression, syntax, true);
            for (int i = 0; i < callbacks.Length; i++)
                callbacks[i].Invoke(syntax);
        }

        public static Func<IDataContext, object> ConvertParameter(Func<LambdaExpression> expression, IBuilderSyntax syntax)
        {
            Should.NotBeNull(expression, "expression");
            var hasClosure = expression.HasClosure();
            if (hasClosure || syntax.Builder.Contains(BindingBuilderConstants.NoCache))
            {
                var e = expression();
                if (hasClosure)
                    e.TraceClosureWarn();
                var converter = new LambdaExpressionToBindingExpressionConverter(e);
                return ConvertParameterInternal(syntax, converter.ConvertParamterInternal(e));
            }
            ParameterCacheValue value;
            lock (CacheParameter)
            {
                if (!CacheParameter.TryGetValue(expression, out value))
                {
                    var e = expression();
                    var converter = new LambdaExpressionToBindingExpressionConverter(e);
                    value = converter.ConvertParamterInternal(e);
                    CacheParameter[expression] = value;
                }
            }
            return ConvertParameterInternal(syntax, value);
        }

        private static Func<IDataContext, object> ConvertParameterInternal(IBuilderSyntax builder, ParameterCacheValue value)
        {
            var actions = value.Actions;
            for (int i = 0; i < actions.Length; i++)
                actions[i].Invoke((IBindingToSyntax)builder);

            var func = value.Expression;
            var members = value.Members;
            var sources = new IObserver[members.Length];
            for (int i = 0; i < members.Length; i++)
                sources[i] = members[i].Invoke(builder.Builder);
            return dataContext =>
            {
                var objects = new object[sources.Length + 1];
                objects[0] = dataContext;
                for (int i = 0; i < sources.Length; i++)
                    objects[i + 1] = sources[i].GetCurrentValue();
                return func(dataContext, objects);
            };
        }

        private static Action<IBindingToSyntax>[] ConvertInternal(LambdaExpression expression, IBindingToSyntax syntax, bool ignoreCallback)
        {
            Expression lastExpression;
            string path;
            if (BindingExtensions.TryGetMemberPath(expression.Body, ".", false, out lastExpression, out path) &&
                expression.Parameters[0] == lastExpression)
            {
                if (ignoreCallback)
                {
                    syntax.ToSource(context => BindingExtensions.CreateBindingSource(context, path, null));
                    return Empty.Array<Action<IBindingToSyntax>>();
                }
                return new Action<IBindingToSyntax>[] { s => s.ToSource(context => BindingExtensions.CreateBindingSource(context, path, null)) };
            }
            var visitor = new LambdaExpressionToBindingExpressionConverter(expression);
            visitor.ConvertInternal(expression);
            var actions = visitor._callbacks.ToArray();
            visitor._members = null;
            visitor._sourceExpression = null;
            visitor._currentExpression = null;
            visitor._methodExpression = null;
            visitor._callbacks = null;
            return actions;
        }

        private void ConvertInternal(LambdaExpression expression)
        {
            var multiExpression = Visit(ContextReplacerVisitor.UpdateContextParameter(expression.Body));
            if (_members.Count == 0)
                AddBuildCallback(syntax => syntax.ToSource(context => BindingExtensions.CreateBindingSource(context, string.Empty, null)));
            else
            {
                for (int i = 0; i < _members.Count; i++)
                {
                    var value = _members[i].Value;
                    AddBuildCallback(syntax => syntax.ToSource(value));
                }
            }
            if (!(multiExpression is ParameterExpression))
            {
                var func = Compile<IList<object>>(multiExpression, false);
                AddBuildCallback(syntax => syntax.Builder.Add(BindingBuilderConstants.MultiExpression, func));
            }
        }

        private ParameterCacheValue ConvertParamterInternal(LambdaExpression expression)
        {
            var multiExpression = Visit(ContextReplacerVisitor.UpdateContextParameter(expression.Body));
            var func = Compile<object[]>(multiExpression, true);
            var members = _members.Select(pair => pair.Value).ToArray();
            var actions = _callbacks.ToArray();
            return new ParameterCacheValue(actions, func, members);
        }

        private Expression TryGetExtensionExpressionOrDefault(Expression lastExpression, Expression current)
        {
            _methodExpression = lastExpression as MethodCallExpression;
            if (_methodExpression != null && _methodExpression.Method.IsStatic &&
                _methodExpression.Method.DeclaringType.IsDefined(typeof(BindingSyntaxExtensionsAttribute), false))
            {
                var type = _methodExpression.Method.DeclaringType;
                var method = type.GetMethodEx(BindingSyntaxEx.ProvideExpressionMethodName,
                    MemberFlags.Public | MemberFlags.NonPublic | MemberFlags.Static);
                if (method != null)
                {
                    _currentExpression = current;
                    var exp = (Expression)method.InvokeEx(null, this);
                    if (exp != null)
                    {
                        if (ReferenceEquals(current, exp))
                            return exp;
                        return Visit(exp);
                    }
                }
            }
            return base.Visit(current);
        }

        private Func<IDataContext, TType, object> Compile<TType>(Expression multiExpression, bool withContext)
            where TType : IList<object>
        {
            var parameters = new ParameterExpression[_members.Count + 1];
            parameters[0] = ContextExpression;
            for (int i = 0; i < _members.Count; i++)
                parameters[i + 1] = _members[i].Key;
            var @delegate = ExpressionReflectionManager
                .CreateLambdaExpression(multiExpression, parameters)
                .Compile();
            Func<object[], object> exp;
            var methodInfo = @delegate.GetType().GetMethodEx("Invoke", MemberFlags.Public | MemberFlags.Instance);
            if (methodInfo == null)
                exp = @delegate.DynamicInvoke;
            else
            {
                Func<object, object[], object> invokeMethod = ServiceProvider.ReflectionManager.GetMethodDelegate(methodInfo);
                exp = objects => invokeMethod(@delegate, objects);
            }
            if (withContext)
                return exp.AsBindingExpressionWithContext<TType>;
            return exp.AsBindingExpression<TType>;
        }

        #endregion

        #region Implementation of IBuilderSyntaxContext

        MethodCallExpression IBuilderSyntaxContext.MethodExpression
        {
            get { return _methodExpression; }
        }

        Expression IBuilderSyntaxContext.Expression
        {
            get { return _currentExpression; }
        }

        ParameterExpression IBuilderSyntaxContext.ContextParameter
        {
            get { return ContextExpression; }
        }

        public Expression GetOrAddParameterExpression(string prefix, string path, Expression expression, Func<IDataContext, string, IObserver> createSource)
        {
            var key = prefix + path;
            for (int i = 0; i < _members.Count; i++)
            {
                if (_members[i].Key.Name == key)
                {
                    Expression p = _members[i].Key;
                    if (p.Type != expression.Type)
                        p = Expression.Convert(p, expression.Type);
                    return p;
                }
            }
            var parameter = Expression.Parameter(expression.Type, key);
            _members.Add(new KeyValuePair<ParameterExpression, Func<IDataContext, IObserver>>(parameter, context => createSource(context, path)));
            return parameter;
        }

        public void AddBuildCallback(Action<IBindingToSyntax> callback)
        {
            _callbacks.Add(callback);
        }

        #endregion
    }
}
