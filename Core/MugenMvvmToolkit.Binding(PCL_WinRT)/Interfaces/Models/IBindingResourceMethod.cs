#region Copyright
// ****************************************************************************
// <copyright file="IBindingResourceMethod.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Interfaces.Models
{
    /// <summary>
    ///     Represents the binding expression method that can be used in multi binding expression.
    /// </summary>
    public interface IBindingResourceMethod
    {
        /// <summary>
        ///     Gets the return type of method.
        /// </summary>
        [NotNull]
        Type GetReturnType(IList<Type> parameters, [NotNull] IList<Type> typeArgs, [NotNull] IDataContext context);

        /// <summary>
        ///     Invokes the method
        /// </summary>
        [CanBeNull]
        object Invoke([NotNull] IList<Type> typeArgs, object[] args, [NotNull] IDataContext context);
    }
}