namespace MugenMvvm.Interfaces.Internal
{
    public interface IValueHolder<TValue> where TValue : class
    {
        TValue? Value { get; set; }
    }
}