using MugenMvvm.Enums;

namespace MugenMvvm.Interfaces.Internal
{
    public interface ITracer
    {
        bool CanTrace(TraceLevel level);

        void Trace(TraceLevel level, string message);
    }
}