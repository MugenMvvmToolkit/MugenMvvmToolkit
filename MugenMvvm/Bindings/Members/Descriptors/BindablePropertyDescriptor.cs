using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace MugenMvvm.Bindings.Members.Descriptors
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct BindablePropertyDescriptor<TTarget, TValue> where TTarget : class
    {
        public readonly string Name;

        public BindablePropertyDescriptor(string name)
        {
            Should.NotBeNull(name, nameof(name));
            Name = name;
        }

        public bool IsStatic => typeof(TTarget) == typeof(Type);

        [Pure]
        public BindablePropertyDescriptor<TNewTarget, TValue> Override<TNewTarget>() where TNewTarget : class => new(Name);

        [Pure]
        public BindablePropertyDescriptor<TTarget, TNewType> ChangeType<TNewType>() => new(Name);

        public static implicit operator BindablePropertyDescriptor<TTarget, TValue>(string name) => new(name);

        public static implicit operator string(BindablePropertyDescriptor<TTarget, TValue> member) => member.Name;

        public override string ToString() => Name;
    }
}