using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Components.Internal
{
    public class TestComponent<T> : IComponent<T>, IHasPriority where T : class
    {
        #region Properties

        public int Priority { get; set; }

        #endregion
    }
}