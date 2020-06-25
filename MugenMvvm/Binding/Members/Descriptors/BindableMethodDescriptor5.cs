using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Members.Descriptors
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct BindableMethodDescriptor<TTarget, TArg1, TArg2, TArg3, TArg4, TArg5, TReturn> where TTarget : class
    {
        #region Fields

        public readonly string Name;

        #endregion

        #region Constructors

        public BindableMethodDescriptor(string name)
        {
            Should.NotBeNull(name, nameof(name));
            Name = name;
        }

        #endregion

        #region Properties

        public bool IsStatic => typeof(TTarget) == typeof(Type);

        public BindableMethodDescriptor<TTarget, TReturn> RawMethod => this;

        public Type[] Types => Default.Types<TArg1, TArg2, TArg3, TArg4, TArg5>();

        #endregion

        #region Methods

        [Pure]
        public BindableMethodDescriptor<TNewTarget, TArg1, TArg2, TArg3, TArg4, TArg5, TReturn> Override<TNewTarget>() where TNewTarget : class => Name;

        public static implicit operator BindableMethodDescriptor<TTarget, TArg1, TArg2, TArg3, TArg4, TArg5, TReturn>(string name) =>
            new BindableMethodDescriptor<TTarget, TArg1, TArg2, TArg3, TArg4, TArg5, TReturn>(name);

        public static implicit operator string(BindableMethodDescriptor<TTarget, TArg1, TArg2, TArg3, TArg4, TArg5, TReturn> member) => member.Name;

        public static implicit operator BindableMethodDescriptor<TTarget, TReturn>(BindableMethodDescriptor<TTarget, TArg1, TArg2, TArg3, TArg4, TArg5, TReturn> member) =>
            new BindableMethodDescriptor<TTarget, TReturn>(member.Name, member.Types);

        public override string ToString() => Name;

        #endregion
    }
}