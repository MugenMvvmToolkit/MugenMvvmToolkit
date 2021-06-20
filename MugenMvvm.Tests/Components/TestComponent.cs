using MugenMvvm.Interfaces.Models;
using MugenMvvm.Tests.Internal;

namespace MugenMvvm.UnitTests.Components.Internal
{
    public class TestComponent<T> : TestDisposableComponent<T>, IHasPriority where T : class
    {
        public int Priority { get; set; }
    }
}