using System;
using System.Runtime.InteropServices;
using MugenMvvm.Bindings.Interfaces.Members;

namespace MugenMvvm.Bindings.Members.Builders
{
    [StructLayout(LayoutKind.Auto)]
    public ref struct ParameterBuilder
    {
        private readonly string _name;
        private readonly Type _parameterType;
        private object? _defaultValue;
        private bool _hasDefaultValue;
        private Func<DelegateParameterInfo<object?>, Type, bool>? _isDefined;
        private object? _underlyingMember;
        private object? _state;

        public ParameterBuilder(string name, Type parameterType)
        {
            Should.NotBeNull(name, nameof(name));
            Should.NotBeNull(parameterType, nameof(parameterType));
            _name = name;
            _parameterType = parameterType;
            _defaultValue = null;
            _hasDefaultValue = false;
            _isDefined = null;
            _underlyingMember = null;
            _state = null;
        }

        public ParameterBuilder UnderlyingMember(object member)
        {
            Should.NotBeNull(member, nameof(member));
            _underlyingMember = member;
            return this;
        }

        public ParameterBuilder DefaultValue(object? defaultValue)
        {
            _hasDefaultValue = true;
            _defaultValue = defaultValue;
            return this;
        }

        public ParameterBuilder IsDefinedHandler(Func<IParameterInfo, Type, bool> isDefined)
        {
            Should.NotBeNull(isDefined, nameof(isDefined));
            _isDefined = isDefined;
            return this;
        }

        public ParameterBuilder IsParamsArray()
        {
            Should.BeSupported(_isDefined == null, nameof(IsDefinedHandler));
            Should.BeValid(_parameterType.IsArray, typeof(ParamArrayAttribute).Name);
            _isDefined = (builder, type) => type == typeof(ParamArrayAttribute);
            return this;
        }

        public ParameterBuilder WithState(object? state)
        {
            _state = state;
            return this;
        }

        public IParameterInfo Build() => new DelegateParameterInfo<object?>(_name, _parameterType, _underlyingMember, _hasDefaultValue, _defaultValue, _state, _isDefined);
    }
}