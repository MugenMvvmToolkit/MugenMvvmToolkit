using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Mediators;

namespace MugenMvvmToolkit.Interfaces.Views
{
    public interface IViewControllerView : IView
    {
        /// <summary>
        ///     Gets the current <see cref="IMvvmViewControllerMediator" />.
        /// </summary>
        [NotNull]
        IMvvmViewControllerMediator Mediator { get; }
    }
}