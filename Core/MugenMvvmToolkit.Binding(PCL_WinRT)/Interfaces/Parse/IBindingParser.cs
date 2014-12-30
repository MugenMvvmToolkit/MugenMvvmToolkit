#region Copyright

// ****************************************************************************
// <copyright file="IBindingParser.cs">
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

using System.Collections.Generic;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Interfaces.Parse
{
    /// <summary>
    ///     Represents the data binding parser inteface.
    /// </summary>
    public interface IBindingParser
    {
        /// <summary>
        ///     Gets the collection of <see cref="IBindingParserHandler" />.
        /// </summary>
        IList<IBindingParserHandler> Handlers { get; }

        /// <summary>
        ///     Parses a string to the set of instances of <see cref="IDataContext" /> that allows to create a series of instances
        ///     of <see cref="IDataBinding" />.
        /// </summary>
        /// <param name="bindingExpression">The specified binding expression.</param>
        /// <param name="context">The specified context.</param>
        /// <returns>A set of instances of <see cref="IDataContext" />.</returns>
        [NotNull]
        IList<IDataContext> Parse([NotNull] string bindingExpression, [NotNull] IDataContext context);
    }
}