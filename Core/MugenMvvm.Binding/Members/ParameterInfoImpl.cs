using System;
using System.Reflection;
using MugenMvvm.Binding.Interfaces.Members;

namespace MugenMvvm.Binding.Members
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

        public bool HasDefaultValue => _parameterInfo.HasDefaultValue;

        public Type ParameterType => _parameterInfo.ParameterType;

        public object? DefaultValue => _parameterInfo.DefaultValue;

        #endregion

        #region Implementation of interfaces

        public bool IsDefined(Type type)
        {
            return _parameterInfo.IsDefined(typeof(ParamArrayAttribute), false);
        }

        #endregion
    }
}