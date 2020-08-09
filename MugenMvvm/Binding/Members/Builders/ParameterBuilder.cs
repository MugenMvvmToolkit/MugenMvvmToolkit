using System;
using System.Runtime.InteropServices;
using MugenMvvm.Binding.Interfaces.Members;

namespace MugenMvvm.Binding.Members.Builders
{
    [StructLayout(LayoutKind.Auto)]
    public ref struct ParameterBuilder
    {
        #region Fields

        private readonly string _name;
        private readonly Type _parameterType;
        private object? _defaultValue;
        private bool _hasDefaultValue;
        private Func<IParameterInfo, Type, bool>? _isDefined;
        private object? _underlyingMember;

        #endregion

        #region Constructors

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
        }

        #endregion

        #region Methods

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
            _isDefined = (builder, type) => type == typeof(ParamArrayAttribute);
            return this;
        }

        public IParameterInfo Build() => new DelegateParameterInfo<object?>(_name, _parameterType, _underlyingMember, _hasDefaultValue, _defaultValue, null, _isDefined);

        #endregion
    }
}