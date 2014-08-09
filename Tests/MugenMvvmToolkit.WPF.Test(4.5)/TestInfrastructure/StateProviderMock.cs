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

        /// <summary>
        ///     Creates an instance of <see cref="IEntitySnapshot" />
        /// </summary>
        /// <param name="entity">The specified entity to create snapshot.</param>
        /// <returns>An instance of <see cref="IEntitySnapshot" /></returns>
        IEntitySnapshot IEntityStateManager.CreateSnapshot(object entity)
        {
            return CreateSnapshot(entity);
        }

        #endregion
    }
}