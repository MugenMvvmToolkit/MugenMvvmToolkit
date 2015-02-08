namespace MugenMvvmToolkit.Interfaces
{
    /// <summary>
    ///     Provides a method that can be used to initialize the current instance.
    /// </summary>
    public interface IHasDisplayCallback
    {
        void WillDisplay();

        void DisplayingEnded();
    }
}