using System;
using System.Reflection;
using MugenMvvm.Bindings.Interfaces.Members;

namespace MugenMvvm.Bindings.Members
{
    public sealed class ParameterInfoImpl : IParameterInfo
    {
        private readonly ParameterInfo _parameterInfo;

        public ParameterInfoImpl(ParameterInfo parameterInfo)
        {
            Should.NotBeNull(parameterInfo, nameof(parameterInfo));
            _parameterInfo = parameterInfo;
        }

        public object UnderlyingParameter => _parameterInfo;

        public string Name => _parameterInfo.Name ?? "";

        public Type ParameterType => _parameterInfo.ParameterType;

        public bool HasDefaultValue => _parameterInfo.HasDefaultValue;

        public object? DefaultValue => _parameterInfo.DefaultValue;

        public bool IsDefined(Type type) => _parameterInfo.IsDefined(type, false);
    }
}