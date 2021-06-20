using MugenMvvm.Interfaces.Models;
using MugenMvvm.Tests.Internal;

namespace MugenMvvm.Tests.Components
{
    public class TestComponent<T> : TestDisposableComponent<T>, IHasPriority where T : class
    {
        public int Priority { get; set; }
    }
}