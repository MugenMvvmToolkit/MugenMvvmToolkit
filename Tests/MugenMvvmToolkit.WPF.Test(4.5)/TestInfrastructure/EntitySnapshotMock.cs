using System;
using System.Collections.Generic;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public class EntitySnapshotMock : IEntitySnapshot
    {
        #region Properties

        public Action<object> Restore { get; set; }

        public Func<object, string, bool> HasChangesProperty { get; set; }

        public Func<object, bool> HasChanges { get; set; }

        #endregion

        #region Implementation of IEntitySnapshot

        /// <summary>
        ///     Gets a value indicating whether the snapshot supports change detection.
        /// </summary>
        public bool SupportChangeDetection { get; set; }

        /// <summary>
        ///     Restores the state of entity.
        /// </summary>
        /// <param name="entity">The specified entity to restore state.</param>
        void IEntitySnapshot.Restore(object entity)
        {
            if (Restore != null)
                Restore(entity);
        }

        /// <summary>
        ///     Gets a value indicating whether the entity has changes.
        /// </summary>
        bool IEntitySnapshot.HasChanges(object entity)
        {
            return HasChanges(entity);
        }

        /// <summary>
        ///     Gets a value indicating whether the entity has changes.
        /// </summary>
        bool IEntitySnapshot.HasChanges(object entity, string propertyName)
        {
            return HasChangesProperty(entity, propertyName);
        }

        /// <summary>
        ///     Dumps the state of object.
        /// </summary>
        public IDictionary<string, Tuple<object, object>> Dump(object entity)
        {
            return new Dictionary<string, Tuple<object, object>>();
        }

        #endregion
    }
}