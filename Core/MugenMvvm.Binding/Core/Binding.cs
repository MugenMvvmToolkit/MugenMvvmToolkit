using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;

namespace MugenMvvm.Binding.Core
{
    public sealed class Binding : BindingBase
    {
        #region Constructors

        public Binding(IBindingPathObserver target, IBindingPathObserver source)
            : base(target, source)
        {
        }

        #endregion

        #region Methods

        protected override object? GetSourceValue(IBindingMemberInfo lastMember)
        {
            return ((IBindingPathObserver)SourceRaw).GetLastMember(Metadata).GetLastMemberValue(Metadata);
        }

        #endregion
    }
}