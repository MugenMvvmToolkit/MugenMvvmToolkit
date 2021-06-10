using MugenMvvm.Constants;

namespace MugenMvvm.Bindings.Constants
{
    public static class BindingComponentPriority
    {
        public const int Delay = 10000;
        public const int ParameterHandler = 1000;
        public const int EventHandler = 0;
        public const int Mode = -100;

        public const int MacrosPreInitializer = 1000;
        public const int MacrosPostInitializer = -10000;

        public const int ExpressionParser = 0;
        public const int BuilderPriorityDecorator = 10;
        public const int BindingHolder = 0;
        public const int BuilderCache = ComponentPriority.Cache;
        public const int BuilderExceptionDecorator = 1000;

        public const int BindingInitializer = 100;
        public const int ParameterInitializer = -100;
        public const int ParameterPostInitializer = -1000;
        public const int LifecyclePostInitializer = -10000;
    }
}