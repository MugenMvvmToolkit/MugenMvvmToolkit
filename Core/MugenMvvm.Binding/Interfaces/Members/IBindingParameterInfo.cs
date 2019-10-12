using System;

namespace MugenMvvm.Binding.Interfaces.Members
{
    public interface IBindingParameterInfo
    {
        bool IsParamsArray { get; }

        bool HasDefaultValue { get; }

        Type ParameterType { get; }

        object? DefaultValue { get; }
    }
}