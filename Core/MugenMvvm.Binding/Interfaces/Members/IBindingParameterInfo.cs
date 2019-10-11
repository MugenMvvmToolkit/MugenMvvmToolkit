using System;

namespace MugenMvvm.Binding.Interfaces.Members
{
    public interface IBindingParameterInfo
    {
        bool IsParamArray { get; }

        bool HasDefaultValue { get; }

        Type ParameterType { get; }

        object? DefaultValue { get; }
    }
}