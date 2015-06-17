#region Copyright

// ****************************************************************************
// <copyright file="INavigationOperation.cs">
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
using System.Threading.Tasks;

namespace MugenMvvmToolkit.Interfaces.Callbacks
{
    /// <summary>
    ///     Represents the navigation operation.
    /// </summary>
    public interface INavigationOperation : IAsyncOperation<bool>
    {
        /// <summary>
        ///     Gets the navigation task, this task will be completed when navigation will be completed.
        /// </summary>
        Task NavigationCompletedTask { get; }
    }
}