using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace MugenMvvm.Binding.Members.Descriptors
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct BindableMethodDescriptor<TTarget, TReturn> where TTarget : class
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

        #region Methods

        [Pure]
        public BindableMethodDescriptor<TNewSource, TReturn> Override<TNewSource>()
            where TNewSource : class
        {
            return new BindableMethodDescriptor<TNewSource, TReturn>(Name);
        }

        public static implicit operator BindableMethodDescriptor<TTarget, TReturn>(string name)
        {
            return new BindableMethodDescriptor<TTarget, TReturn>(name);
        }

        public static implicit operator string(BindableMethodDescriptor<TTarget, TReturn> member)
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