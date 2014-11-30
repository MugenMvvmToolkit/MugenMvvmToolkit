using System.Threading.Tasks;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public class VisualStateManagerMock : IVisualStateManager
    {
        #region Implementation of IVisualStateManager

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
        public Task<bool> GoToStateAsync(object view, string stateName, bool useTransitions, IDataContext context)
        {
            return Empty.FalseTask;
        }

        #endregion
    }
}