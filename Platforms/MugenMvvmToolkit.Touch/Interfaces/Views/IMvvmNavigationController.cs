using System;
using System.ComponentModel;

namespace MugenMvvmToolkit.Interfaces.Views
{
    public interface IMvvmNavigationController : IMvvmViewController
    {
        event EventHandler<CancelEventArgs> ShouldPopViewController;

        event EventHandler<EventArgs> DidPopViewController;
    }
}