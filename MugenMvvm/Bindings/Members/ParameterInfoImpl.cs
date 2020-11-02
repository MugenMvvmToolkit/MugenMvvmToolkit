﻿using System;
using System.Reflection;
using MugenMvvm.Bindings.Interfaces.Members;

namespace MugenMvvm.Bindings.Members
{
    public sealed class ParameterInfoImpl : IParameterInfo
    {
        #region Fields

        private readonly ParameterInfo _parameterInfo;

        #endregion

        #region Constructors

        public ParameterInfoImpl(ParameterInfo parameterInfo)
        {
            Should.NotBeNull(parameterInfo, nameof(parameterInfo));
            _parameterInfo = parameterInfo;
        }

        #endregion

        #region Properties

        public object? UnderlyingParameter => _parameterInfo;

        public string Name => _parameterInfo.Name ?? "";

        public Type ParameterType => _parameterInfo.ParameterType;

        public bool HasDefaultValue => _parameterInfo.HasDefaultValue;

        public object? DefaultValue => _parameterInfo.DefaultValue;

        #endregion

        #region Implementation of interfaces

        public bool IsDefined(Type type) => _parameterInfo.IsDefined(type, false);

        #endregion
    }
}