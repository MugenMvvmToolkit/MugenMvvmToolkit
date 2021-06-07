using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Internal
{
    public class TestHasServiceModel<T> : IHasService<T> where T : class
    {
        public T Service { get; set; } = default!;

        public T? ServiceOptional { get; set; }

        T? IHasService<T>.GetService(bool optional) => optional ? ServiceOptional : Service;
    }
}