using System;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Interfaces.Members
{
    public interface IParameterInfo : IHasName
    {
        object? UnderlyingParameter { get; }

        Type ParameterType { get; }

        bool HasDefaultValue { get; }

        object? DefaultValue { get; }

        bool IsDefined(Type type);
    }
}