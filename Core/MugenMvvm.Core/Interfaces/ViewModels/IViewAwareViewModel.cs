using MugenMvvm.Attributes;

namespace MugenMvvm.Interfaces.ViewModels
{
    public interface IViewAwareViewModel<TView> : IViewModel where TView : class
    {
        [Preserve(Conditional = true)]
        TView View { get; set; }
    }
}