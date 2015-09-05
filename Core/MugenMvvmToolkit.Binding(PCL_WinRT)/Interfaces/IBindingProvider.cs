#region Copyright

// ****************************************************************************
// <copyright file="IBindingProvider.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces.Parse;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Interfaces
{
    /// <summary>
    ///     Represents the binding provider that allows to create and manage the data bindings.
    /// </summary>
    public interface IBindingProvider
    {
        /// <summary>
        ///     Gets the default behaviors.
        /// </summary>
        [NotNull]
        ICollection<IBindingBehavior> DefaultBehaviors { get; }

        /// <summary>
        ///     Gets or sets the <see cref="IBindingParser" />.
        /// </summary>
        [NotNull]
        IBindingParser Parser { get; set; }

        /// <summary>
        ///     Occurs when binding starts initialization.
        /// </summary>
        event Action<IBindingProvider, IDataContext> BindingInitializing;

        /// <summary>
        ///     Occurs when binding ends initialization.
        /// </summary>
        event Action<IBindingProvider, IDataBinding> BindingInitialized;

        /// <summary>
        ///     Creates an instance of <see cref="IBindingBuilder" />.
        /// </summary>
        /// <param name="context">The specified context.</param>
        /// <returns>An instance of <see cref="IBindingBuilder" />.</returns>
        [NotNull]
        IBindingBuilder CreateBuilder(IDataContext context = null);

        /// <summary>
        ///     Creates an instance of <see cref="IDataBinding" />.
        /// </summary>
        /// <param name="context">The specified context.</param>
        /// <returns>An instance of <see cref="IDataBinding" />.</returns>
        [NotNull]
        IDataBinding CreateBinding([NotNull] IDataContext context);

        /// <summary>
        ///     Creates a series of instances of <see cref="IBindingBuilder" />.
        /// </summary>
        /// <param name="target">The specified binding target.</param>
        /// <param name="bindingExpression">The specified binding expression.</param>
        /// <param name="sources">The specified sources, if any.</param>
        /// <returns>An instance of <see cref="IBindingBuilder" />.</returns>
        [NotNull]
        IList<IBindingBuilder> CreateBuildersFromString([NotNull] object target, [NotNull] string bindingExpression, IList<object> sources = null);

        /// <summary>
        ///     Creates a series of instances of <see cref="IDataBinding" />.
        /// </summary>
        /// <param name="target">The specified binding target.</param>
        /// <param name="bindingExpression">The specified binding expression.</param>
        /// <param name="sources">The specified sources, if any.</param>
        /// <returns>An instance of <see cref="IDataBinding" />.</returns>
        [NotNull]
        IList<IDataBinding> CreateBindingsFromString([NotNull] object target, [NotNull] string bindingExpression, IList<object> sources = null);
    }
}