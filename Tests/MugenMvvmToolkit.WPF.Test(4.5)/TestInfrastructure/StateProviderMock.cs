using System;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public class StateManagerMock : IEntityStateManager
    {
        #region Properties

        public Func<object, IEntitySnapshot> CreateSnapshot { get; set; }

        #endregion

        #region Implementation of IEntityStateManager

        IEntitySnapshot IEntityStateManager.CreateSnapshot(object entity, IDataContext context)
        {
            return CreateSnapshot(entity);
        }

        #endregion
    }
}