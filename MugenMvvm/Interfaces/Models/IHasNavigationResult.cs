namespace MugenMvvm.Interfaces.Models
{
    public interface IHasNavigationResult<out TResult>
    {
        TResult Result { get; }
    }
}