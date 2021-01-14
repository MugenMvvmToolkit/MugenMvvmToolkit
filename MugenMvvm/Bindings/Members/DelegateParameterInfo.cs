using System;
using MugenMvvm.Bindings.Interfaces.Members;

namespace MugenMvvm.Bindings.Members
{
    public sealed class DelegateParameterInfo<TState> : IParameterInfo
    {
        public readonly TState State;

        private readonly Func<DelegateParameterInfo<TState>, Type, bool>? _isDefined;

        public DelegateParameterInfo(string name, Type parameterType, object? underlyingParameter, bool hasDefaultValue, object? defaultValue, TState state,
            Func<DelegateParameterInfo<TState>, Type, bool>? isDefined)
        {
            Should.NotBeNull(name, nameof(name));
            Should.NotBeNull(parameterType, nameof(parameterType));
            Name = name;
            State = state;
            UnderlyingParameter = underlyingParameter;
            ParameterType = parameterType;
            HasDefaultValue = hasDefaultValue;
            DefaultValue = defaultValue;
            _isDefined = isDefined;
        }

        public object? UnderlyingParameter { get; }

        public string Name { get; }

        public Type ParameterType { get; }

        public bool HasDefaultValue { get; }

        public object? DefaultValue { get; }

        public bool IsDefined(Type type)
        {
            if (_isDefined == null)
                return false;
            return _isDefined(this, type);
        }
    }
}