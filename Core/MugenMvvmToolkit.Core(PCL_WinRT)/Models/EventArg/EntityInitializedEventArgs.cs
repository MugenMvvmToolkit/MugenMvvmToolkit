#region Copyright

// ****************************************************************************
// <copyright file="EntityInitializedEventArgs.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Models.EventArg
{
    public class EntityInitializedEventArgs : EventArgs
    {
        #region Fields

        private readonly object _entity;
        private readonly object _originalEntity;

        #endregion

        #region Constructors

        public EntityInitializedEventArgs([NotNull]object originalEntity, [NotNull] object entity)
        {
            Should.NotBeNull(originalEntity, nameof(originalEntity));
            Should.NotBeNull(entity, nameof(entity));
            _originalEntity = originalEntity;
            _entity = entity;
        }

        #endregion

        #region Properties

        [NotNull]
        public object OriginalEntity => _originalEntity;

        [NotNull]
        public object Entity => _entity;

        #endregion
    }

    public class EntityInitializedEventArgs<TEntity> : EntityInitializedEventArgs
    {
        #region Constructors

        public EntityInitializedEventArgs([NotNull]TEntity originalEntity, [NotNull] TEntity entity)
            : base(originalEntity, entity)
        {
        }

        #endregion

        #region Properties

        [NotNull]
        public new TEntity OriginalEntity => (TEntity)base.OriginalEntity;

        [NotNull]
        public new TEntity Entity => (TEntity)base.Entity;

        #endregion
    }
}
