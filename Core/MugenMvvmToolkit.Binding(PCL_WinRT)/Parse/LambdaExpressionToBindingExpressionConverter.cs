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
using MugenMvvmToolkit.Binding.Interfaces.Sources;
using MugenMvvmToolkit.Binding.Interfaces.Syntax;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Parse
{
    /// <summary>
    ///     Represents the class that allows to convert a lambda expression to binding expression.
    /// </summary>
    public sealed class LambdaExpressionToBindingExpressionConverter : ExpressionVisitor, IBuilderSyntaxContext
    {
        #region Fields

        private static readonly ParameterExpression ContextExpression;
        private static readonly Dictionary<Delegate, Action<IBindingToSyntax>[]> Cache;

        private List<Action<IBindingToSyntax>> _callbacks;
        private List<KeyValuePair<ParameterExpression, Func<IDataContext, IBindingSource>>> _members;
        private Func<IDataContext, string, IBindingSource> _getBindingSource;
        private ParameterExpression _sourceExpression;

        private Expression _currentExpression;
        private MethodCallExpression _methodExpression;

        #endregion

        #region Constructors

        static LambdaExpressionToBindingExpressionConverter()
        {
            ContextExpression = Expression.Parameter(typeof(IDataContext), "$context$");
            Cache = new Dictionary<Delegate, Action<IBindingToSyntax>[]>(ReferenceEqualityComparer.Instance);
        }

        private LambdaExpressionToBindingExpressionConverter(LambdaExpression expression, Func<IDataContext, string, IBindingSource> getBindingSource)
        {
            _getBindingSource = getBindingSource;
            _sourceExpression = expression.Parameters[0];
            _callbacks = new List<Action<IBindingToSyntax>>();
            _members = new List<KeyValuePair<ParameterExpression, Func<IDataContext, IBindingSource>>>(2);
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
                    return GetOrAddParameterExpression(string.Empty, path, node, _getBindingSource);
                return TryGetExtensionExpressionOrDefault(lastExpression, node);
            }
            return TryGetExtensionExpressionOrDefault(lastExpression ?? node, node);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Converts a <see cref="LambdaExpression" /> to a binding expression.
        /// </summary>
        public static IBindingModeInfoBehaviorSyntax Convert(Func<LambdaExpression> getExpression, IBindingToSyntax syntax,
            Func<IDataContext, string, IBindingSource> getBindingSource)
        {
            if (getExpression.Target != null || syntax.Builder.Contains(BindingBuilderConstants.NoCache))
                return Convert(getExpression(), syntax, getBindingSource);
            Action<IBindingToSyntax>[] callbacks;
            lock (Cache)
            {
                if (!Cache.TryGetValue(getExpression, out callbacks))
                {
                    callbacks = ApplyInternal(getExpression(), syntax, getBindingSource, false);
                    Cache[getExpression] = callbacks;
                }
            }
            for (int i = 0; i < callbacks.Length; i++)
                callbacks[i].Invoke(syntax);
            if (syntax.Builder.Contains(BindingBuilderConstants.NoCache))
            {
                lock (Cache)
                    Cache.Remove(getExpression);
            }
            return syntax.GetOrAddSyntaxBuilder<IBindingModeInfoBehaviorSyntax, object, object>();
        }

        /// <summary>
        ///     Converts a <see cref="LambdaExpression" /> to a binding expression.
        /// </summary>
        public static IBindingModeInfoBehaviorSyntax Convert(LambdaExpression expression, IBindingToSyntax syntax,
            Func<IDataContext, string, IBindingSource> getBindingSource)
        {
            var callbacks = ApplyInternal(expression, syntax, getBindingSource, true);
            for (int i = 0; i < callbacks.Length; i++)
                callbacks[i].Invoke(syntax);
            return syntax.GetOrAddSyntaxBuilder<IBindingModeInfoBehaviorSyntax, object, object>();
        }

        private static Action<IBindingToSyntax>[] ApplyInternal(LambdaExpression expression, IBindingToSyntax syntax, Func<IDataContext, string, IBindingSource> getBindingSource, bool ignoreCallback)
        {
            Expression lastExpression;
            string path;
            if (BindingExtensions.TryGetMemberPath(expression.Body, ".", false, out lastExpression, out path) &&
                expression.Parameters[0] == lastExpression)
            {
                if (ignoreCallback)
                {
                    syntax.ToSource(context => getBindingSource(context, path));
                    return Empty.Array<Action<IBindingToSyntax>>();
                }
                return new Action<IBindingToSyntax>[] { s => s.ToSource(context => getBindingSource(context, path)) };
            }
            var visitor = new LambdaExpressionToBindingExpressionConverter(expression, getBindingSource);
            visitor.ApplyInternal(expression);
            var actions = visitor._callbacks.ToArray();
            visitor._getBindingSource = null;
            visitor._members = null;
            visitor._sourceExpression = null;
            visitor._currentExpression = null;
            visitor._methodExpression = null;
            visitor._callbacks = null;
            return actions;
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

        private void ApplyInternal(LambdaExpression expression)
        {
            var multiExpression = Visit(expression.Body);
            if (_members.Count == 0)
                AddBuildCallback(syntax => syntax.ToSource(context => BindingExtensions.CreateBindingSource(context, string.Empty)));
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
                    var invokeMethod = ServiceProvider.ReflectionManager.GetMethodDelegate(methodInfo);
                    exp = objects => invokeMethod(@delegate, objects);
                }
                AddBuildCallback(syntax => syntax.Builder.Add(BindingBuilderConstants.MultiExpression,
                    (context, list) =>
                    {
                        var args = new object[list.Count + 1];
                        args[0] = context;
                        for (int i = 0; i < list.Count; i++)
                            args[i + 1] = list[i];
                        return exp.Invoke(args);
                    }));
            }
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

        /// <summary>
        ///     Gets or adds parameter expression.
        /// </summary>
        public Expression GetOrAddParameterExpression(string prefix, string path, Expression expression, Func<IDataContext, string, IBindingSource> createSource)
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
            _members.Add(new KeyValuePair<ParameterExpression, Func<IDataContext, IBindingSource>>(parameter, context => createSource(context, path)));
            return parameter;
        }

        /// <summary>
        ///     Adds the delegate callback that will be called when creating binding.
        /// </summary>
        public void AddBuildCallback(Action<IBindingToSyntax> callback)
        {
            _callbacks.Add(callback);
        }

        #endregion
    }
}