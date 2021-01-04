﻿using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstant.DataContractNamespace)]
    public class UnhandledExceptionType : EnumBase<UnhandledExceptionType, int>
    {
        #region Fields

        public static readonly UnhandledExceptionType Binding = new(1);
        public static readonly UnhandledExceptionType System = new(2);

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected UnhandledExceptionType()
        {
        }

        public UnhandledExceptionType(int value, string? name = null) : base(value, name)
        {
        }

        #endregion
    }
}