namespace MugenMvvm.Interfaces.Serialization
{
    public interface ISerializationFormatBase
    {
        bool IsSerialization { get; }

        string Name { get; }
    }

    public interface ISerializationFormatBase<out TRequest, in TResult> : ISerializationFormatBase
    {
    }

    public interface ISerializationFormat<out TRequest, in TResult> : ISerializationFormatBase<TRequest, TResult>
    {
    }

    public interface IDeserializationFormat<out TRequest, in TResult> : ISerializationFormatBase<TRequest, TResult>
    {
    }
}