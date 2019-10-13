using MugenMvvm.Enums;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Internal
{
    public interface ITracer : IComponent<IMugenApplication>//todo review
    {
        bool CanTrace(TraceLevel level);

        void Trace(TraceLevel level, string message);
    }
}