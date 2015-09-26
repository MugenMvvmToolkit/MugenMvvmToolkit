#region Copyright

// ****************************************************************************
// <copyright file="IBindingParserHandler.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Interfaces.Parse
{
    public interface IBindingParserHandler
    {
        void Handle(ref string bindingExpression, IDataContext context);

        void HandleTargetPath(ref string targetPath, IDataContext context);

        [CanBeNull]
        Action<IDataContext> Handle([CanBeNull] ref IExpressionNode expression, bool isPrimaryExpression, IDataContext context);
    }
}
