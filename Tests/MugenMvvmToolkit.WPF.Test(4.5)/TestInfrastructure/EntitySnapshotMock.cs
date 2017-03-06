#region Copyright

// ****************************************************************************
// <copyright file="EntitySnapshotMock.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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

        public bool SupportChangeDetection { get; set; }

        void IEntitySnapshot.Restore(object entity)
        {
            if (Restore != null)
                Restore(entity);
        }

        bool IEntitySnapshot.HasChanges(object entity)
        {
            return HasChanges(entity);
        }

        bool IEntitySnapshot.HasChanges(object entity, string propertyName)
        {
            return HasChangesProperty(entity, propertyName);
        }

        public IDictionary<string, Tuple<object, object>> Dump(object entity)
        {
            return new Dictionary<string, Tuple<object, object>>();
        }

        #endregion
    }
}
