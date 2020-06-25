using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Internal.Internal
{
    public class TestHasServiceModel<T> : IHasService<T>, IHasOptionalService<T> where T : class
    {
        #region Properties

        public T Service { get; set; } = default!;

        T? IHasOptionalService<T>.Service => ServiceOptional;

        public T? ServiceOptional { get; set; }

        #endregion
    }
}