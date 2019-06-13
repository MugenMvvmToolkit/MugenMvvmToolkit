using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace MugenMvvm.Binding.Infrastructure.Members
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct BindingMemberDescriptor<TSource, TValue> where TSource : class
    {
        #region Fields

        public readonly string Path;

        #endregion

        #region Constructors

        public BindingMemberDescriptor(string path)
        {
            Should.NotBeNull(path, nameof(path));
            Path = path;
        }

        #endregion

        #region Methods

        [Pure]
        public BindingMemberDescriptor<TNewSource, TValue> Override<TNewSource>()
            where TNewSource : class
        {
            return new BindingMemberDescriptor<TNewSource, TValue>(Path);
        }

        [Pure]
        public BindingMemberDescriptor<TSource, TNewType> Cast<TNewType>()
        {
            return new BindingMemberDescriptor<TSource, TNewType>(Path);
        }

        public static implicit operator BindingMemberDescriptor<TSource, TValue>(string path)
        {
            return new BindingMemberDescriptor<TSource, TValue>(path);
        }

        public static implicit operator string(BindingMemberDescriptor<TSource, TValue> descriptor)
        {
            return descriptor.Path;
        }

        public override string ToString()
        {
            return Path;
        }

        #endregion
    }
}