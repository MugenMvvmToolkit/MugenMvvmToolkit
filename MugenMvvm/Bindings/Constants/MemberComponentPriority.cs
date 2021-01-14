using MugenMvvm.Constants;

namespace MugenMvvm.Bindings.Constants
{
    public static class MemberComponentPriority
    {
        public const int Attached = 10;
        public const int Instance = 0;
        public const int Extension = -10;
        public const int Dynamic = -20;

        public const int RequestHandlerDecorator = 100;
        public const int Cache = ComponentPriority.Cache;
        public const int IndexerAccessorDecorator = 100;
        public const int MethodAccessorDecorator = 100;
        public const int Selector = 0;
    }
}