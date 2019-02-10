using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Views.Infrastructure
{
    public interface IViewManagerResult<T> : IHasMetadata<IReadOnlyMetadataContext> where T : class
    {
        Task<T> WaitAsync();
    }
}