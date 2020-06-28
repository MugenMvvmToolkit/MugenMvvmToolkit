﻿using System;

namespace MugenMvvm.Binding.Interfaces.Members
{
    public interface IParameterInfo
    {
        object? UnderlyingParameter { get; }

        string Name { get; }

        Type ParameterType { get; }

        bool HasDefaultValue { get; }

        object? DefaultValue { get; }

        bool IsDefined(Type type);
    }
}