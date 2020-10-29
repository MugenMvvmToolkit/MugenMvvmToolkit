using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace MugenMvvm.Bindings.Members.Descriptors
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct BindableEventDescriptor<TTarget> where TTarget : class
    {
        #region Fields

        public readonly string Name;

        #endregion

        #region Constructors

        public BindableEventDescriptor(string name)
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
        public BindableEventDescriptor<TNewTarget> Override<TNewTarget>() where TNewTarget : class => new BindableEventDescriptor<TNewTarget>(Name);

        public static implicit operator BindableEventDescriptor<TTarget>(string name) => new BindableEventDescriptor<TTarget>(name);

        public static implicit operator string(BindableEventDescriptor<TTarget> member) => member.Name;

        public override string ToString() => Name;

        #endregion
    }
}