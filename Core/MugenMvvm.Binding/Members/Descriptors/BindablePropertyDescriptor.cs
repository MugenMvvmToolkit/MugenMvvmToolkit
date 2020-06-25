using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace MugenMvvm.Binding.Members.Descriptors
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct BindablePropertyDescriptor<TTarget, TValue> where TTarget : class
    {
        #region Fields

        public readonly string Name;

        #endregion

        #region Constructors

        public BindablePropertyDescriptor(string name)
        {
            Should.NotBeNull(name, nameof(name));
            Name = name;
        }

        #endregion

        #region Properties

        public bool IsStatic => typeof(TTarget) == typeof(Type);

        #endregion

        #region Methods

        [Pure]
        public BindablePropertyDescriptor<TNewTarget, TValue> Override<TNewTarget>() where TNewTarget : class => new BindablePropertyDescriptor<TNewTarget, TValue>(Name);

        [Pure]
        public BindablePropertyDescriptor<TTarget, TNewType> ChangeType<TNewType>() => new BindablePropertyDescriptor<TTarget, TNewType>(Name);

        public static implicit operator BindablePropertyDescriptor<TTarget, TValue>(string name) => new BindablePropertyDescriptor<TTarget, TValue>(name);

        public static implicit operator string(BindablePropertyDescriptor<TTarget, TValue> member) => member.Name;

        public override string ToString() => Name;

        #endregion
    }
}