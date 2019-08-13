using System.Runtime.InteropServices;
using MugenMvvm.Binding.Interfaces.Members;

namespace MugenMvvm.Binding.Members
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct AttachedMemberRegistration
    {
        #region Fields

        public readonly string Name;
        public readonly IBindingMemberInfo Member;

        #endregion

        #region Constructors

        public AttachedMemberRegistration(string name, IBindingMemberInfo member)
        {
            Name = name;
            Member = member;
        }

        #endregion
    }
}