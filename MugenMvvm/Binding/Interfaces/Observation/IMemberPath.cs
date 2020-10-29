using System.Collections.Generic;

namespace MugenMvvm.Bindings.Interfaces.Observation
{
    public interface IMemberPath
    {
        string Path { get; }

        IReadOnlyList<string> Members { get; }
    }
}