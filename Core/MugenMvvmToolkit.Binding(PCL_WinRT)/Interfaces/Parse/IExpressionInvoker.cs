#region Copyright
// ****************************************************************************
// <copyright file="IExpressionInvoker.cs">
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
using System.Collections.Generic;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Interfaces.Parse
{
    /// <summary>
    ///     Represents the node expression invoker.
    /// </summary>
    public interface IExpressionInvoker
    {
        /// <summary>
        ///     Invokes an expression using specified context and source values.
        /// </summary>
        object Invoke(IDataContext context, IList<object> sourceValues);
    }
}