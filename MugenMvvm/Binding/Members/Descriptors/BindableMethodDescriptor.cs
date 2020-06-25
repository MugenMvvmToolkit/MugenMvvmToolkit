using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace MugenMvvm.Binding.Members.Descriptors
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct BindableMethodDescriptor<TTarget, TReturn> where TTarget : class
    {
        #region Fields

        public readonly string Name;
        public readonly Type[] Types;

        #endregion

        #region Constructors

        public BindableMethodDescriptor(string name, Type[] types)
        {
            Should.NotBeNull(name, nameof(name));
            Should.NotBeNull(types, nameof(types));
            Name = name;
            Types = types;
        }

        #endregion

        #region Properties

        public bool IsStatic => typeof(TTarget) == typeof(Type);

        #endregion

        #region Methods

        [Pure]
        public BindableMethodDescriptor<TNewTarget, TReturn> Override<TNewTarget>() where TNewTarget : class => new BindableMethodDescriptor<TNewTarget, TReturn>(Name, Types);

        public static implicit operator string(BindableMethodDescriptor<TTarget, TReturn> member) => member.Name;

        public override string ToString() => Name;

        #endregion
    }
}