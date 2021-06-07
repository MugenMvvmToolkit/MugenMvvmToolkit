using System;
using MugenMvvm.Bindings.Interfaces.Members;

namespace MugenMvvm.Tests.Bindings.Members
{
    public class TestParameterInfo : IParameterInfo
    {
        public Func<Type, bool>? IsDefined { get; set; }

        public object? UnderlyingParameter { get; set; }

        public string Name { get; set; } = null!;

        public bool HasDefaultValue { get; set; }

        public Type ParameterType { get; set; } = default!;

        public object? DefaultValue { get; set; }

        bool IParameterInfo.IsDefined(Type type) => IsDefined?.Invoke(type) ?? false;
    }
}