using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;

namespace MugenMvvm.Binding.Core
{
    public sealed class DataBinding : DataBindingBase
    {
        #region Constructors

        public DataBinding(IBindingPathObserver target, IBindingPathObserver source)
            : base(target, source)
        {
        }

        #endregion

        #region Methods

        protected override object? GetSourceValue(IBindingMemberInfo lastMember)
        {
            return ((IBindingPathObserver)SourceRaw).GetLastMember(Metadata).GetLastMemberValue(metadata: Metadata);
        }

        #endregion
    }
}