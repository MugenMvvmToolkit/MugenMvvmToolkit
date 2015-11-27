using System;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public class StateTransitionManagerMock : IStateTransitionManager
    {
        #region Properties

        public Func<object, EntityState, EntityState, EntityState> ChangeState { get; set; }

        #endregion

        #region Implementation of IStateTransitionManager

        EntityState IStateTransitionManager.ChangeState(object item, EntityState @from, EntityState to)
        {
            if (ChangeState == null)
                return to;
            return ChangeState(item, @from, to);
        }

        #endregion
    }
}
