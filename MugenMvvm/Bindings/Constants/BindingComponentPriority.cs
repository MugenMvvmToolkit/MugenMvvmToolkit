using MugenMvvm.Constants;

namespace MugenMvvm.Bindings.Constants
{
    public static class BindingComponentPriority
    {
        public const int EventHandler = 0;
        public const int ParameterHandler = 1000;
        public const int Delay = 10000;
        public const int Mode = -100;

        public const int MacrosPreInitializer = 100;
        public const int BindingInitializer = 10;
        public const int ParameterInitializer = -10;
        public const int ParameterPostInitializer = -100;
        public const int MacrosPostInitializer = -1000;
        public const int LifecyclePostInitializer = -1000;
        public const int ExpressionParser = 0;
        public const int BuilderPriorityDecorator = 1;
        public const int BindingHolder = 0;
        public const int BuilderCache = ComponentPriority.Cache;
        public const int BuilderExceptionDecorator = 100;
    }
}