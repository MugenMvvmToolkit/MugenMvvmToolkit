using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;

namespace MugenMvvm.Binding.Infrastructure.Core
{
    public sealed class DataBinding : DataBindingBase, ISingleSourceDataBinding
    {
        #region Constructors

        public DataBinding(IBindingPathObserver target, IBindingPathObserver source)
            : base(target)
        {
            Should.NotBeNull(source, nameof(source));
            Source = source;
        }

        #endregion

        #region Properties

        public override IBindingPathObserver[] Sources => new[] { Source };

        public IBindingPathObserver Source { get; }

        #endregion

        #region Methods

        protected override object? GetSourceValue(IBindingMemberInfo lastMember)
        {
            return Source.GetLastMember(Metadata).GetLastMemberValue(metadata: Metadata);
        }

        protected override bool UpdateSourceInternal(out object? newValue)
        {
            return SetTargetValue(Source, out newValue);
        }

        #endregion
    }
}