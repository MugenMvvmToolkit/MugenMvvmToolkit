using System.Runtime.InteropServices;
using MugenMvvm.Binding.Interfaces.Members;

namespace MugenMvvm.Binding.Infrastructure.Members
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct AttachedMemberRegistration
    {
        #region Constructors

        public AttachedMemberRegistration(string name, IBindingMemberInfo member)
        {
            Name = name;
            Member = member;
        }

        #endregion

        #region Properties

        public string Name { get; }

        public IBindingMemberInfo Member { get; }

        #endregion
    }
}