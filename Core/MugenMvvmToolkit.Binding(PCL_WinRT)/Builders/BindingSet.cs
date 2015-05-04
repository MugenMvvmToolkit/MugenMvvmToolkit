#region Copyright

// ****************************************************************************
// <copyright file="BindingSet.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Syntax;

namespace MugenMvvmToolkit.Binding.Builders
{
    /// <summary>
    ///     Represents the binding set that allows to configure multiple bindings for a target.
    /// </summary>
    public class BindingSet : IDisposable
    {
        #region Fields

        private static readonly Func<IBindingBuilder, int> OrderByTargetPathDelegate;

        private readonly List<IBindingBuilder> _builders;

        #endregion

        #region Constructors

        static BindingSet()
        {
            OrderByTargetPathDelegate = OrderByTargetPath;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingSet" /> class.
        /// </summary>
        public BindingSet()
        {
            _builders = new List<IBindingBuilder>();
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Creates a binding using a string expression.
        /// </summary>
        /// <param name="target">The specified binding target.</param>
        /// <param name="targetPath">The target member path.</param>
        /// <param name="bindingExpression">The specified expression.</param>
        public void BindFromExpression([NotNull]object target, string targetPath, [NotNull]string bindingExpression)
        {
            BindFromExpression(target, targetPath, bindingExpression, null);
        }

        /// <summary>
        ///     Creates a binding using a string expression.
        /// </summary>
        /// <param name="target">The specified binding target.</param>
        /// <param name="targetPath">The target member path.</param>
        /// <param name="bindingExpression">The specified expression.</param>
        /// <param name="sources">The specified sources, if any.</param>
        public void BindFromExpression([NotNull]object target, string targetPath, [NotNull]string bindingExpression, params object[] sources)
        {
            BindFromExpression(target, targetPath + " " + bindingExpression + ";", sources);
        }

        /// <summary>
        ///     Creates a binding using a string expression.
        /// </summary>
        /// <param name="target">The specified binding target.</param>
        /// <param name="bindingExpression">The specified expression.</param>
        public void BindFromExpression([NotNull]object target, [NotNull]string bindingExpression)
        {
            BindFromExpression(target, bindingExpression, sources: null);
        }

        /// <summary>
        ///     Creates a binding using a string expression.
        /// </summary>
        /// <param name="target">The specified binding target.</param>
        /// <param name="bindingExpression">The specified expression.</param>
        /// <param name="sources">The specified sources, if any.</param>
        public void BindFromExpression([NotNull]object target, [NotNull]string bindingExpression, params object[] sources)
        {
            var builders = BindingServiceProvider.BindingProvider.CreateBuildersFromString(target, bindingExpression, sources);
            for (int index = 0; index < builders.Count; index++)
                AddBuilder(builders[index]);
        }

        /// <summary>
        ///     Creates a binding builder.
        /// </summary>
        /// <returns>The fluent syntax.</returns>
        public IBindingToSyntax<TLocalTarget> Bind<TLocalTarget>([NotNull]TLocalTarget target,
            [NotNull] string targetPath) where TLocalTarget : class
        {
            return GetBuilder().Bind(target, targetPath);
        }

        /// <summary>
        ///     Creates a binding builder.
        /// </summary>
        /// <returns>The fluent syntax.</returns>
        [Obsolete(BindingExceptionManager.ObsoleteExpressionUsage)]
        public IBindingToSyntax<TLocalTarget> Bind<TLocalTarget>([NotNull]TLocalTarget target,
            [NotNull] Expression<Func<TLocalTarget, object>> targetPath) where TLocalTarget : class
        {
            return GetBuilder().Bind(target, targetPath);
        }

        /// <summary>
        ///     Creates a binding builder.
        /// </summary>
        /// <returns>The fluent syntax.</returns>
        public IBindingToSyntax<TLocalTarget> Bind<TLocalTarget>([NotNull]TLocalTarget target,
            [NotNull] Func<Expression<Func<TLocalTarget, object>>> targetPath) where TLocalTarget : class
        {
            return GetBuilder().Bind(target, targetPath);
        }

        /// <summary>
        ///     Applies all bindings to the target.
        /// </summary>
        public void Apply()
        {
            if (_builders.Count == 0)
                return;
            lock (_builders)
            {
                foreach (var bindingBuilder in _builders.OrderByDescending(OrderByTargetPathDelegate))
                    bindingBuilder.Build();
                _builders.Clear();
            }
        }

        /// <summary>
        ///     Applies all bindings to the target and returns the collection of bindings.
        /// </summary>
        [NotNull]
        public IList<IDataBinding> ApplyWithBindings()
        {
            if (_builders.Count == 0)
                return Empty.Array<IDataBinding>();
            lock (_builders)
            {
                var result = new IDataBinding[_builders.Count];
                int index = 0;
                foreach (var bindingBuilder in _builders.OrderByDescending(OrderByTargetPathDelegate))
                    result[index++] = bindingBuilder.Build();
                _builders.Clear();
                return result;
            }
        }

        private IBindingBuilder AddBuilder(IBindingBuilder builder)
        {
            lock (_builders)
                _builders.Add(builder);
            return builder;
        }

        internal IBindingBuilder GetBuilder()
        {
            return AddBuilder(BindingServiceProvider.BindingProvider.CreateBuilder());
        }

        private static int OrderByTargetPath(IBindingBuilder bindingBuilder)
        {
            var path = bindingBuilder.GetData(BindingBuilderConstants.TargetPath);
            if (path == null)
                return 0;
            int value;
            BindingServiceProvider.BindingMemberPriorities.TryGetValue(path.Path, out value);
            return value;
        }

        #endregion

        #region Implementation of IDisposable

        void IDisposable.Dispose()
        {
            Apply();
        }

        #endregion
    }

    /// <summary>
    ///     Represents the binding set that allows to configure multiple bindings for a target.
    /// </summary>
    public class BindingSet<TSource> : BindingSet
    {
        #region Methods

        /// <summary>
        ///     Creates a binding builder.
        /// </summary>
        /// <returns>The fluent syntax.</returns>
        public new IBindingToSyntax<TLocalTarget, TSource> Bind<TLocalTarget>([NotNull]TLocalTarget target,
            [NotNull] string targetPath) where TLocalTarget : class
        {
            return GetBuilder().Bind<TLocalTarget, TSource>(target, targetPath);
        }

        /// <summary>
        ///     Creates a binding builder.
        /// </summary>
        /// <returns>The fluent syntax.</returns>
        [Obsolete(BindingExceptionManager.ObsoleteExpressionUsage)]
        public new IBindingToSyntax<TLocalTarget, TSource> Bind<TLocalTarget>([NotNull]TLocalTarget target,
            [NotNull] Expression<Func<TLocalTarget, object>> targetPath) where TLocalTarget : class
        {
            return GetBuilder().Bind<TLocalTarget, TSource>(target, targetPath);
        }

        /// <summary>
        ///     Creates a binding builder.
        /// </summary>
        /// <returns>The fluent syntax.</returns>
        public new IBindingToSyntax<TLocalTarget, TSource> Bind<TLocalTarget>([NotNull]TLocalTarget target,
            [NotNull] Func<Expression<Func<TLocalTarget, object>>> targetPath) where TLocalTarget : class
        {
            return GetBuilder().Bind<TLocalTarget, TSource>(target, targetPath);
        }

        #endregion
    }

    /// <summary>
    ///     Represents the binding set that allows to configure multiple bindings for a target.
    /// </summary>
    public class BindingSet<TTarget, TSource> : BindingSet<TSource> where TTarget : class
    {
        #region Fields

        private TTarget _target;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingSet{TTarget,TSource}" /> class.
        /// </summary>
        public BindingSet([CanBeNull]TTarget target)
        {
            _target = target;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the binding target.
        /// </summary>
        [CanBeNull]
        public TTarget Target
        {
            get { return _target; }
            set { _target = value; }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Creates a binding using a string expression.
        /// </summary>
        /// <param name="bindingExpression">The specified expression.</param>
        public void BindFromExpression([NotNull]string bindingExpression)
        {
            BindFromExpression(_target, bindingExpression);
        }

        /// <summary>
        ///     Creates a binding builder.
        /// </summary>
        /// <returns>The fluent syntax.</returns>
        public IBindingToSyntax<TTarget, TSource> Bind([NotNull] string targetPath)
        {
            return Bind(_target, targetPath);
        }

        /// <summary>
        ///     Creates a binding builder.
        /// </summary>
        /// <returns>The fluent syntax.</returns>
        [Obsolete(BindingExceptionManager.ObsoleteExpressionUsage)]
        public IBindingToSyntax<TTarget, TSource> Bind([NotNull] Expression<Func<TTarget, object>> targetPath)
        {
            return Bind(_target, targetPath);
        }

        /// <summary>
        ///     Creates a binding builder.
        /// </summary>
        /// <returns>The fluent syntax.</returns>
        public IBindingToSyntax<TTarget, TSource> Bind([NotNull] Func<Expression<Func<TTarget, object>>> targetPath)
        {
            return Bind(_target, targetPath);
        }

        #endregion
    }
}