#region Copyright
// ****************************************************************************
// <copyright file="TrackingEntity.cs">
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
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Models
{
    /// <summary>
    ///     Represents the tracking entity.
    /// </summary>
    [StructLayout(LayoutKind.Auto), Serializable, DataContract(Namespace = ApplicationSettings.DataContractNamespace)]
    public struct TrackingEntity<TEntity>
    {
        #region Fields

        /// <summary>
        ///     Gets the entity object.
        /// </summary>
        [NotNull, DataMember]
        public readonly TEntity Entity;

        /// <summary>
        ///     Gets or sets the state of the <see cref="EntityState" />.
        /// </summary>
        [DataMember]
        public readonly EntityState State;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="TrackingEntity{TEntity}" /> class.
        /// </summary>
        public TrackingEntity([NotNull]TEntity entity, EntityState state)
        {
            Entity = entity;
            State = state;
        }

        #endregion

        #region Overrides of Object

        /// <summary>
        ///     Returns a <see cref="T:System.String" /> that represents the current <see cref="TrackingEntity{TEntity}" />.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.String" /> that represents the current <see cref="TrackingEntity{TEntity}" />.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0} - {1}", State, Entity);
        }

        #endregion
    }
}