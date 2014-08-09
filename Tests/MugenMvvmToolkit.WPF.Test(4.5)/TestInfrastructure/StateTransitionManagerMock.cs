using System;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public class StateTransitionManagerMock : IStateTransitionManager
    {
        #region Properties

        public Func<EntityState, EntityState, bool, EntityState> ChangeState { get; set; }

        #endregion

        #region Implementation of IStateTransitionManager

        /// <summary>
        ///     Changes state from one state to another state.
        /// </summary>
        /// <param name="from">The specified state from.</param>
        /// <param name="to">The specified state to.</param>
        /// <param name="validateState">The flag indicating that state will be validated before assigned.</param>
        /// <returns>An instance of state, if any.</returns>
        EntityState IStateTransitionManager.ChangeState(EntityState @from, EntityState to, bool validateState)
        {
            if (ChangeState == null)
                return to;
            return ChangeState(@from, to, validateState);
        }

        #endregion
    }
}