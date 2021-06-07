using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Tests.Internal
{
    public class TestValueHolder<T> : IValueHolder<T> where T : class
    {
        public T? Value { get; set; }
    }
}