using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Serialization
{
    public interface ISerializationFormatBase : IHasName
    {
        bool IsSerialization { get; }
    }

    public interface ISerializationFormatBase<out TRequest, in TResult> : ISerializationFormatBase
    {
    }

    // ReSharper disable once TypeParameterCanBeVariant
    public interface ISerializationFormat<out TRequest, TResult> : ISerializationFormatBase<TRequest, TResult>
    {
    }

    public interface IDeserializationFormat<out TRequest, in TResult> : ISerializationFormatBase<TRequest, TResult>
    {
    }
}