using MugenMvvm.Binding.Interfaces.Observers;

namespace MugenMvvm.Binding.Interfaces.Core
{
    public interface ISingleSourceDataBinding : IDataBinding
    {
        IBindingPathObserver Source { get; }
    }
}