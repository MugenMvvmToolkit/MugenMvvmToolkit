#region Copyright
// ****************************************************************************
// <copyright file="IVisualStateManager.cs">
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
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Views;

namespace MugenMvvmToolkit.Interfaces
{
    /// <summary>
    ///     Manages states and the logic for transitioning between states for controls.
    /// </summary>
    public interface IVisualStateManager
    {
        /// <summary>
        ///     Transitions the control between two states.
        /// </summary>
        /// <returns>
        ///     true if the control successfully transitioned to the new state; otherwise, false.
        /// </returns>
        /// <param name="view">The view to transition between states. </param>
        /// <param name="stateName">The state to transition to.</param>
        /// <param name="useTransitions">true to use a VisualTransition to transition between states; otherwise, false.</param>
        /// <param name="context">The specified context.</param>
        [NotNull]
        Task<bool> GoToStateAsync([NotNull]IView view, [NotNull] string stateName, bool useTransitions,
            [CanBeNull] IDataContext context);
    }
}