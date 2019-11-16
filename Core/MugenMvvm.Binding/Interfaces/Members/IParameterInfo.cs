using System;

namespace MugenMvvm.Binding.Interfaces.Members
{
    public interface IParameterInfo
    {
        object? UnderlyingParameter { get; }

        bool HasDefaultValue { get; }

        Type ParameterType { get; }

        object? DefaultValue { get; }

        bool IsDefined(Type type);
    }
}