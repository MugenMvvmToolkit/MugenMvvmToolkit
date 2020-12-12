namespace MugenMvvm.Interfaces.Models
{
    public interface IHasResult<out TResult>
    {
        TResult Result { get; }
    }
}