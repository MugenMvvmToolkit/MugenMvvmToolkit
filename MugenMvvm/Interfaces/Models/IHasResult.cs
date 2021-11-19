using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Models
{
    public interface IHasResult<TResult>
    {
        Optional<TResult> Result { get; }
    }
}