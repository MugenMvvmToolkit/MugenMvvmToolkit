using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Models
{
    public class TestHasServiceModel<T> : IHasService<T> where T : class
    {
        #region Properties

        public T Service { get; set; }

        #endregion
    }
}