using MugenMvvm.Enums;

namespace MugenMvvm.Interfaces.IoC
{
    public interface IIoCParameter
    {
        string Name { get; }

        object Value { get; }

        IoCParameterType ParameterType { get; }
    }
}