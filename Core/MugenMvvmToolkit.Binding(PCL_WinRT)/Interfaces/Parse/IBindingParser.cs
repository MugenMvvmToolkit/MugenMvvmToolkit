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
    public interface IBindingParser
    {
        IList<IBindingParserHandler> Handlers { get; }

        [NotNull]
        IList<IDataContext> Parse([NotNull] string bindingExpression, [CanBeNull] IDataContext context,
            [NotNull] object target, [CanBeNull] IList<object> sources);
    }
}
