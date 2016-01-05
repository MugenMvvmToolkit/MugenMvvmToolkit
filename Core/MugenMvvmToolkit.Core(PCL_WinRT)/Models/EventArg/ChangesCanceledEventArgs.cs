#region Copyright

// ****************************************************************************
// <copyright file="ChangesCanceledEventArgs.cs">
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
    public class ChangesCanceledEventArgs : EventArgs
    {
        #region Fields

        private readonly object _entity;

        #endregion

        #region Constructors

        public ChangesCanceledEventArgs([NotNull]object entity)
        {
            Should.NotBeNull(entity, nameof(entity));
            _entity = entity;
        }

        #endregion

        #region Properties

        [NotNull]
        public object Entity => _entity;

        #endregion
    }

    public class ChangesCanceledEventArgs<TEntity> : ChangesCanceledEventArgs
    {
        #region Constructors

        public ChangesCanceledEventArgs([NotNull]TEntity entity)
            : base(entity)
        {
        }

        #endregion

        #region Properties

        [NotNull]
        public new TEntity Entity => (TEntity) base.Entity;

        #endregion
    }
}
