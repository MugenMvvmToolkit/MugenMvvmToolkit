using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.UnitTests.Internal.Internal
{
    public class TestValueHolder<T> : IValueHolder<T> where T : class
    {
        public T? Value { get; set; }
    }
}