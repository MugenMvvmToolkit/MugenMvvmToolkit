#region Copyright

// ****************************************************************************
// <copyright file="EntityStateEntry.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
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
using System.Runtime.Serialization;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Models
{
    /// <summary>
    ///     Represents the entity state entry.
    /// </summary>
    [Serializable, DataContract(Namespace = ApplicationSettings.DataContractNamespace, IsReference = true)]
    public class EntityStateEntry : IEntityStateEntry, IEquatable<EntityStateEntry>
    {
        #region Fields

        private object _entity;
        private EntityState _state;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="EntityStateEntry" /> class.
        /// </summary>
        public EntityStateEntry(EntityState state, object entity)
        {
            Should.NotBeNull(entity, "entity");
            _state = state;
            _entity = entity;
        }

        #endregion

        #region Implementation of IEntityStateEntry

        /// <summary>
        ///     Gets or sets the state of the <see cref="EntityState" />.
        /// </summary>
        [DataMember]
        public EntityState State
        {
            get { return _state; }
            internal set { _state = value; }
        }

        /// <summary>
        ///     Gets the entity object.
        /// </summary>
        [DataMember]
        public object Entity
        {
            get { return _entity; }
            internal set { _entity = value; }
        }

        #endregion

        #region Overrides of Object

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0} - {1}", State, Entity);
        }

        #endregion

        #region Equality members

        /// <summary>
        ///     Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        ///     true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(EntityStateEntry other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return State == other.State && Entity.Equals(other.Entity);
        }

        /// <summary>
        ///     Determines whether the specified <see cref="T:System.Object" /> is equal to the current
        ///     <see cref="T:System.Object" />.
        /// </summary>
        /// <returns>
        ///     true if the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />;
        ///     otherwise, false.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object" /> to compare with the current <see cref="T:System.Object" />. </param>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((EntityStateEntry)obj);
        }

        /// <summary>
        ///     Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        ///     A hash code for the current <see cref="T:System.Object" />.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)State * 397) ^ Entity.GetHashCode();
            }
        }

        #endregion
    }
}