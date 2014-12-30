#region Copyright

// ****************************************************************************
// <copyright file="IStateTransitionManager.cs">
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

using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Interfaces
{
    /// <summary>
    ///     Represents the interface that provides methods for transiting one state to another state.
    /// </summary>
    public interface IStateTransitionManager
    {
        /// <summary>
        ///     Changes state from one state to another state.
        /// </summary>
        /// <param name="from">The specified state from.</param>
        /// <param name="to">The specified state to.</param>
        /// <param name="validateState">The flag indicating that state will be validated before assigned.</param>
        /// <returns>An instance of state, if any.</returns>
        EntityState ChangeState(EntityState from, EntityState to, bool validateState);
    }
}