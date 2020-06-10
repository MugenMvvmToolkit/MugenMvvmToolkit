using System;
using MugenMvvm.Binding.Interfaces.Members;

namespace MugenMvvm.UnitTest.Binding.Members.Internal
{
    public class TestParameterInfo : IParameterInfo
    {
        #region Properties

        public object? UnderlyingParameter { get; set; }

        public string Name { get; set; } = null!;

        public bool HasDefaultValue { get; set; }

        public Type ParameterType { get; set; } = default!;

        public object? DefaultValue { get; set; }

        public Func<Type, bool>? IsDefined { get; set; }

        #endregion

        #region Implementation of interfaces

        bool IParameterInfo.IsDefined(Type type)
        {
            return IsDefined?.Invoke(type) ?? false;
        }

        #endregion
    }
}