#region Copyright

// ****************************************************************************
// <copyright file="EntityStateEntry.cs">
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
using System.Runtime.Serialization;
using MugenMvvmToolkit.Attributes;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Models
{
    [Serializable, DataContract(Namespace = ApplicationSettings.DataContractNamespace, IsReference = true)]
    public class EntityStateEntry : IEntityStateEntry, IEquatable<EntityStateEntry>
    {
        #region Fields

        private object _entity;
        private EntityState _state;

        #endregion

        #region Constructors

        //Only for serialization
        [Preserve]
        internal EntityStateEntry() { }

        public EntityStateEntry(EntityState state, object entity)
        {
            Should.NotBeNull(entity, nameof(entity));
            State = state;
            Entity = entity;
        }

        #endregion

        #region Implementation of IEntityStateEntry

        [DataMember(Name = "st")]
        public EntityState State
        {
            get { return _state; }
            internal set { _state = value; }
        }

        [DataMember(Name = "en")]
        public object Entity
        {
            get { return _entity; }
            internal set { _entity = value; }
        }

        #endregion

        #region Overrides of Object

        public override string ToString()
        {
            return $"{State} - {Entity}";
        }

        #endregion

        #region Equality members

        public bool Equals(EntityStateEntry other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return State == other.State && Entity.Equals(other.Entity);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((EntityStateEntry)obj);
        }

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
