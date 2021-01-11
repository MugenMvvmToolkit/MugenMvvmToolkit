using MugenMvvm.Collections;

namespace MugenMvvm.Bindings.Interfaces.Observation
{
    public interface IMemberPath
    {
        string Path { get; }

        ItemOrIReadOnlyList<string> Members { get; }
    }
}