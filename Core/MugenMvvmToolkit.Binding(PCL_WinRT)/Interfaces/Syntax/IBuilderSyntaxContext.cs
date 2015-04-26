#region Copyright

// ****************************************************************************
// <copyright file="IBuilderSyntaxContext.cs">
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
using System.Linq.Expressions;
using MugenMvvmToolkit.Binding.Interfaces.Sources;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Interfaces.Syntax
{
    /// <summary>
    ///     Represents the binding builder context.
    /// </summary>
    public interface IBuilderSyntaxContext
    {
        /// <summary>
        ///     Gets the current method expression.
        /// </summary>
        MethodCallExpression MethodExpression { get; }

        /// <summary>
        ///     Gets the current expression.
        /// </summary>
        Expression Expression { get; }

        /// <summary>
        ///     Gets the <see cref="IDataContext" /> parameter.
        /// </summary>
        ParameterExpression ContextParameter { get; }

        /// <summary>
        ///     Gets or adds parameter expression.
        /// </summary>
        Expression GetOrAddParameterExpression(string prefix, string path, Expression expression,
            Func<IDataContext, string, IBindingSource> createSource);

        /// <summary>
        ///     Adds the delegate callback that will be called when creating binding.
        /// </summary>
        void AddBuildCallback(Action<IBindingToSyntax> callback);
    }
}