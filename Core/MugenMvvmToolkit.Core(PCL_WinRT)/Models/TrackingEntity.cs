#region Copyright

// ****************************************************************************
// <copyright file="TrackingEntity.cs">
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
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Models
{
    [StructLayout(LayoutKind.Auto), Serializable, DataContract(Namespace = ApplicationSettings.DataContractNamespace)]
    public struct TrackingEntity<TEntity> : IEntityStateEntry
    {
        #region Fields

        [NotNull, DataMember]
        public readonly TEntity Entity;

        [DataMember]
        public readonly EntityState State;

        #endregion

        #region Constructors

        public TrackingEntity([NotNull]TEntity entity, EntityState state)
        {
            Entity = entity;
            State = state;
        }

        #endregion

        #region Implementation of IEntityStateEntry

        EntityState IEntityStateEntry.State => State;

        object IEntityStateEntry.Entity => Entity;

        #endregion

        #region Overrides of Object

        public override string ToString()
        {
            return $"{State} - {Entity}";
        }

        #endregion
    }
}
