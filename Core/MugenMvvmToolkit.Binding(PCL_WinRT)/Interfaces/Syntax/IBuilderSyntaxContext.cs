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
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Interfaces.Syntax
{
    public interface IBuilderSyntaxContext
    {
        MethodCallExpression MethodExpression { get; }

        Expression Expression { get; }

        ParameterExpression ContextParameter { get; }

        Expression GetOrAddParameterExpression(string prefix, string path, Expression expression,
            Func<IDataContext, string, IObserver> createSource);

        void AddBuildCallback(Action<IBindingBuilder> callback);
    }
}
