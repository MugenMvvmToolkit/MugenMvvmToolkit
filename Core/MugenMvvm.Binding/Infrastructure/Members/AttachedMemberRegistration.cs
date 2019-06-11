using MugenMvvm.Binding.Interfaces.Members;

namespace MugenMvvm.Binding.Infrastructure.Members
{
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