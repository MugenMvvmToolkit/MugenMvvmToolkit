using System;

namespace MugenMvvm.Binding.Interfaces.Members
{
    public interface IParameterInfo
    {
        bool IsParamsArray { get; }

        bool HasDefaultValue { get; }

        Type ParameterType { get; }

        object? DefaultValue { get; }
    }
}