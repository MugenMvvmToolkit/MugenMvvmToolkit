#region Copyright

// ****************************************************************************
// <copyright file="IBindingParser.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
        ICollection<string> ElementSourceAliases { get; }

        ICollection<string> RelativeSourceAliases { get; }

        IDictionary<string, TokenType> UnaryOperationAliases { get; }

        IDictionary<string, TokenType> BinaryOperationAliases { get; }

        IList<IBindingParserHandler> Handlers { get; }

        [NotNull]
        IList<IDataContext> Parse([NotNull] object target, [NotNull] string bindingExpression, [CanBeNull] IList<object> sources,
            [CanBeNull] IDataContext context);

        void InvalidateCache();
    }
}
