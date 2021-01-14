using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace MugenMvvm.Bindings.Members.Descriptors
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct BindableEventDescriptor<TTarget> where TTarget : class
    {
        public readonly string Name;

        public BindableEventDescriptor(string name)
        {
            Should.NotBeNull(name, nameof(name));
            Name = name;
        }

        public bool IsStatic => typeof(TTarget) == typeof(Type);

        [Pure]
        public BindableEventDescriptor<TNewTarget> Override<TNewTarget>() where TNewTarget : class => new(Name);

        public static implicit operator BindableEventDescriptor<TTarget>(string name) => new(name);

        public static implicit operator string(BindableEventDescriptor<TTarget> member) => member.Name;

        public override string ToString() => Name;
    }
}