using System.Collections.Generic;

namespace MugenMvvm.Binding.Interfaces.Observers
{
    public interface IMemberPath
    {
        string Path { get; }

        IReadOnlyList<string> Members { get; }
    }
}