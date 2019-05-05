using MugenMvvm.Enums;

namespace MugenMvvm.Interfaces.IoC
{
    public interface IIocParameter
    {
        string Name { get; }

        object Value { get; }

        IocParameterType ParameterType { get; }
    }
}