using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace MugenMvvm.Binding.Members.Descriptors
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct BindableAccessorDescriptor<TTarget, TValue> where TTarget : class
    {
        #region Fields

        public readonly string Name;

        #endregion

        #region Constructors

        public BindableAccessorDescriptor(string name)
        {
            Should.NotBeNull(name, nameof(name));
            Name = name;
        }

        #endregion

        #region Methods

        [Pure]
        public BindableAccessorDescriptor<TNewSource, TValue> Override<TNewSource>()
            where TNewSource : class
        {
            return new BindableAccessorDescriptor<TNewSource, TValue>(Name);
        }

        [Pure]
        public BindableAccessorDescriptor<TTarget, TNewType> Cast<TNewType>()
        {
            return new BindableAccessorDescriptor<TTarget, TNewType>(Name);
        }

        public static implicit operator BindableAccessorDescriptor<TTarget, TValue>(string name)
        {
            return new BindableAccessorDescriptor<TTarget, TValue>(name);
        }

        public static implicit operator string(BindableAccessorDescriptor<TTarget, TValue> member)
        {
            return member.Name;
        }

        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}