namespace MugenMvvm.Bindings.Constants
{
    public static class BindingComponentPriority
    {
        #region Fields

        public const int EventHandler = 0;
        public const int ParameterHandler = 1000;
        public const int Delay = 10000;
        public const int Mode = -100;

        public const int PostInitializer = -1000;

        public const int BindingParameterPreInitializer = 100;
        public const int BindingInitializer = 10;
        public const int BindingParameterInitializer = -10;
        public const int BindingParameterPostInitializer = -100;

        public const int ExpressionParser = 0;
        public const int ExpressionPriorityDecorator = 1;
        public const int BindingHolder = 0;

        #endregion
    }
}