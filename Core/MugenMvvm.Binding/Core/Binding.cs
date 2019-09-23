using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;

namespace MugenMvvm.Binding.Core
{
    public sealed class Binding : BindingBase
    {
        #region Constructors

        public Binding(IMemberPathObserver target, IMemberPathObserver source)
            : base(target, source)
        {
        }

        #endregion

        #region Methods

        protected override object? GetSourceValue(IBindingMemberInfo lastMember)
        {
            return ((IMemberPathObserver)SourceRaw).GetLastMember(Metadata).GetLastMemberValue(Metadata);
        }

        #endregion
    }
}