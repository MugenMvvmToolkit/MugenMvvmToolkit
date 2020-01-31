using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Models
{
    public class TestHasServiceModel<T> : IHasService<T> where T : class
    {
        #region Properties

#pragma warning disable CS8618
        public T Service { get; set; }
#pragma warning restore CS8618

        #endregion
    }
}