#region Copyright
// ****************************************************************************
// <copyright file="IBindingParserHandler.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Interfaces.Parse
{
    /// <summary>
    ///     Represents the interface that allows to pre-process the binding text.
    /// </summary>
    public interface IBindingParserHandler
    {
        /// <summary>
        ///     Prepares a string for the binding.
        /// </summary>
        /// <param name="bindingExpression">The specified binding expression.</param>
        /// <param name="context">The specified context.</param>
        /// <returns>An instance of <see cref="string" />.</returns>
        void Handle(ref string bindingExpression, IDataContext context);

        /// <summary>
        ///     Prepares an <see cref="IExpressionNode" /> for the binding.
        /// </summary>
        /// <param name="expression">The specified binding expression.</param>
        /// <param name="isPrimaryExpression">If <c>true</c> it's main binding expression; otherwise parameter expression.</param>
        /// <param name="context">The specified context.</param>
        /// <returns>An instance of delegate to update binding.</returns>
        [CanBeNull]
        Action<IDataContext> Handle([CanBeNull] ref IExpressionNode expression, bool isPrimaryExpression, IDataContext context);
    }
}