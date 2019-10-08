using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace MugenMvvm.Binding.Members.Descriptors
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct BindablePropertyDescriptor<TSource, TValue> where TSource : class
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
        public BindablePropertyDescriptor<TSource, TNewType> Cast<TNewType>()
        {
            return new BindablePropertyDescriptor<TSource, TNewType>(Name);
        }

        public static implicit operator BindablePropertyDescriptor<TSource, TValue>(string path)
        {
            return new BindablePropertyDescriptor<TSource, TValue>(path);
        }

        public static implicit operator string(BindablePropertyDescriptor<TSource, TValue> member)
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