using System.Threading.Tasks;

namespace MugenMvvmToolkit.Interfaces.Callbacks
{
    /// <summary>
    ///     Represents the navigation operation.
    /// </summary>
    public interface INavigationOperation : IAsyncOperation<bool>
    {
        /// <summary>
        ///     Gets the navigation task, this task will be completed when navigation will be completed.
        /// </summary>
        Task NavigationCompletedTask { get; }
    }
}