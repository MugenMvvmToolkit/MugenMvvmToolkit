﻿using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstant.DataContractNamespace)]
    public class EntityState : EnumBase<EntityState, int>
    {
        #region Fields

        public static readonly EntityState Unchanged = new EntityState(1);
        public static readonly EntityState Added = new EntityState(2);
        public static readonly EntityState Deleted = new EntityState(3);
        public static readonly EntityState Modified = new EntityState(4);
        public static readonly EntityState Detached = new EntityState(5);

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected EntityState()
        {
        }

        public EntityState(int value) : base(value)
        {
        }

        #endregion
    }
}