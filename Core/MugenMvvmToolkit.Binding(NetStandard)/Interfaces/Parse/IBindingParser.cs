#region Copyright

// ****************************************************************************
// <copyright file="IBindingParser.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Interfaces.Parse
{
    public interface IBindingParser
    {
        [NotNull]
        ICollection<string> ElementSourceAliases { get; }

        [NotNull]
        ICollection<string> RelativeSourceAliases { get; }

        [NotNull]
        IDictionary<string, TokenType> UnaryOperationAliases { get; }

        [NotNull]
        IDictionary<string, TokenType> BinaryOperationAliases { get; }

        [NotNull]
        IList<IBindingParserHandler> Handlers { get; }

        [NotNull]
        IList<IDataContext> Parse([NotNull] object target, [NotNull] string bindingExpression, [CanBeNull] IList<object> sources,
            [CanBeNull] IDataContext context);

        void InvalidateCache();
    }
}
