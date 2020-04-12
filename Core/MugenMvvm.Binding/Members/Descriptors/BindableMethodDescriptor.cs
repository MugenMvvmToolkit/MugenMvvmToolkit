using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace MugenMvvm.Binding.Members.Descriptors
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct BindableMethodDescriptor<TTarget> where TTarget : class
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
        public BindableMethodDescriptor<TNewSource> Override<TNewSource>()
            where TNewSource : class
        {
            return new BindableMethodDescriptor<TNewSource>(Name);
        }

        public static implicit operator BindableMethodDescriptor<TTarget>(string name)
        {
            return new BindableMethodDescriptor<TTarget>(name);
        }

        public static implicit operator string(BindableMethodDescriptor<TTarget> member)
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