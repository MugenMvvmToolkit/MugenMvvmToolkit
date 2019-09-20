using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace MugenMvvm.Binding.Members
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct BindableMember<TSource, TValue> where TSource : class
    {
        #region Fields

        public readonly string Path;

        #endregion

        #region Constructors

        public BindableMember(string path)
        {
            Should.NotBeNull(path, nameof(path));
            Path = path;
        }

        #endregion

        #region Methods

        [Pure]
        public BindableMember<TNewSource, TValue> Override<TNewSource>()
            where TNewSource : class
        {
            return new BindableMember<TNewSource, TValue>(Path);
        }

        [Pure]
        public BindableMember<TSource, TNewType> Cast<TNewType>()
        {
            return new BindableMember<TSource, TNewType>(Path);
        }

        public static implicit operator BindableMember<TSource, TValue>(string path)
        {
            return new BindableMember<TSource, TValue>(path);
        }

        public static implicit operator string(BindableMember<TSource, TValue> member)
        {
            return member.Path;
        }

        public override string ToString()
        {
            return Path;
        }

        #endregion
    }
}