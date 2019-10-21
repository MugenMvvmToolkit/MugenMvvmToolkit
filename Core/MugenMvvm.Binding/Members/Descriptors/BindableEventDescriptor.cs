using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace MugenMvvm.Binding.Members.Descriptors
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

        #region Methods

        [Pure]
        public BindableEventDescriptor<TNewSource> Override<TNewSource>()
            where TNewSource : class
        {
            return new BindableEventDescriptor<TNewSource>(Name);
        }

        public static implicit operator BindableEventDescriptor<TTarget>(string path)
        {
            return new BindableEventDescriptor<TTarget>(path);
        }

        public static implicit operator string(BindableEventDescriptor<TTarget> member)
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