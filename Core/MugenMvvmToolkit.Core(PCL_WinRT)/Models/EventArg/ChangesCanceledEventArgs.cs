#region Copyright
// ****************************************************************************
// <copyright file="ChangesCanceledEventArgs.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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

        /// <summary>
        ///     Initializes the <see cref="ChangesCanceledEventArgs" />.
        /// </summary>
        public ChangesCanceledEventArgs([NotNull]object entity)
        {
            Should.NotBeNull(entity, "entity");
            _entity = entity;
        }

        #endregion

        #region Properties

        [NotNull]
        public object Entity
        {
            get { return _entity; }
        }

        #endregion
    }

    public class ChangesCanceledEventArgs<TEntity> : ChangesCanceledEventArgs
    {
        #region Constructors

        /// <summary>
        ///     Initializes the <see cref="ChangesCanceledEventArgs{TEntity}" />.
        /// </summary>
        public ChangesCanceledEventArgs([NotNull]TEntity entity)
            : base(entity)
        {
        }

        #endregion

        #region Properties

        [NotNull]
        public new TEntity Entity
        {
            get { return (TEntity) base.Entity; }
        }

        #endregion
    }
}