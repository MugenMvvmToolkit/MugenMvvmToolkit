#region Copyright
// ****************************************************************************
// <copyright file="BindingSet.cs">
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
using System.Linq.Expressions;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Syntax;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Builders
{
    /// <summary>
    ///     Represents the binding set that allows to configure multiple bindings for a target.
    /// </summary>
    public sealed class BindingSet<TTarget, TSource> where TTarget : class
    {
        #region Fields

        private readonly List<IBindingBuilder> _builders;
        private readonly TTarget _target;
        private readonly IBindingProvider _bindingProvider;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingSet{TTarget,TSource}" /> class.
        /// </summary>
        public BindingSet([NotNull] TTarget target, IBindingProvider bindingProvider = null)
        {
            Should.NotBeNull(target, "target");
            _target = target;
            _builders = new List<IBindingBuilder>();
            _bindingProvider = bindingProvider ?? BindingServiceProvider.BindingProvider;
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
        ///     Creates a binding using a string expression.
        /// </summary>
        /// <param name="localTarget">The specified binding target.</param>
        /// <param name="bindingExpression">The specified expression.</param>
        public void BindFromExpression([NotNull]object localTarget, [NotNull]string bindingExpression)
        {
            var builders = _bindingProvider.CreateBuildersFromString(localTarget, bindingExpression, null);
            for (int index = 0; index < builders.Count; index++)
                AddBuilder(builders[index]);
        }

        /// <summary>
        ///     Creates a binding builder.
        /// </summary>
        /// <returns>The fluent syntax.</returns>
        public IBindingToSyntax<TLocalTarget, TSource> Bind<TLocalTarget>([NotNull]TLocalTarget localTarget,
            [NotNull] string targetPath) where TLocalTarget : class
        {
            return GetBuilder().Bind<TLocalTarget, TSource>(localTarget, targetPath);
        }

        /// <summary>
        ///     Creates a binding builder.
        /// </summary>
        /// <returns>The fluent syntax.</returns>
        public IBindingToSyntax<TLocalTarget, TSource> Bind<TLocalTarget>([NotNull]TLocalTarget localTarget,
            [NotNull] Expression<Func<TLocalTarget, object>> targetPath) where TLocalTarget : class
        {
            return GetBuilder().Bind<TLocalTarget, TSource>(localTarget, targetPath);
        }

        /// <summary>
        ///     Creates a binding builder.
        /// </summary>
        /// <returns>The fluent syntax.</returns>
        public IBindingToSyntax<TTarget, TSource> Bind([NotNull] string targetPath)
        {
            return GetBuilder().Bind<TTarget, TSource>(_target, targetPath);
        }

        /// <summary>
        ///     Creates a binding builder.
        /// </summary>
        /// <returns>The fluent syntax.</returns>
        public IBindingToSyntax<TTarget, TSource> Bind([NotNull] Expression<Func<TTarget, object>> targetPath)
        {
            return GetBuilder().Bind<TTarget, TSource>(_target, targetPath);
        }

        /// <summary>
        ///     Applies all bindings to the target.
        /// </summary>
        [NotNull]
        public IList<IDataBinding> Apply()
        {
            lock (_builders)
            {
                var bindings = _builders.ToArrayFast(builder => builder.Build());
                _builders.Clear();
                return bindings;
            }
        }

        private IBindingBuilder AddBuilder(IBindingBuilder builder)
        {
            lock (_builders)
                _builders.Add(builder);
            return builder;
        }

        private IBindingBuilder GetBuilder()
        {
            return AddBuilder(_bindingProvider.CreateBuilder(new DataContext()));
        }

        #endregion
    }
}