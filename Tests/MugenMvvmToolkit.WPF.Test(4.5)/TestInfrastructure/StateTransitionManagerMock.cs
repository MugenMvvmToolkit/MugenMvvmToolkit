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

        EntityState IStateTransitionManager.ChangeState(EntityState @from, EntityState to, bool validateState)
        {
            if (ChangeState == null)
                return to;
            return ChangeState(@from, to, validateState);
        }

        #endregion
    }
}
