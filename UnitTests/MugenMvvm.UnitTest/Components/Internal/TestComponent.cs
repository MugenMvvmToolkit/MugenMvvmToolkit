using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.UnitTest.Models;
using MugenMvvm.UnitTest.Models.Internal;

namespace MugenMvvm.UnitTest.Components.Internal
{
    public class TestComponent<T> : TestDisposable, IComponent<T>, IHasPriority where T : class
    {
        #region Properties

        public int Priority { get; set; }

        #endregion
    }
}