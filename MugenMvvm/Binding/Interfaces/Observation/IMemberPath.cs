using System.Collections.Generic;

namespace MugenMvvm.Binding.Interfaces.Observation
{
    public interface IMemberPath
    {
        string Path { get; }

        IReadOnlyList<string> Members { get; }
    }
}