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

        #region Methods

        [Pure]
        public BindablePropertyDescriptor<TNewSource, TValue> Override<TNewSource>()
            where TNewSource : class
        {
            return new BindablePropertyDescriptor<TNewSource, TValue>(Name);
        }

        [Pure]
        public BindablePropertyDescriptor<TTarget, TNewType> Cast<TNewType>()
        {
            return new BindablePropertyDescriptor<TTarget, TNewType>(Name);
        }

        public static implicit operator BindablePropertyDescriptor<TTarget, TValue>(string path)
        {
            return new BindablePropertyDescriptor<TTarget, TValue>(path);
        }

        public static implicit operator string(BindablePropertyDescriptor<TTarget, TValue> member)
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