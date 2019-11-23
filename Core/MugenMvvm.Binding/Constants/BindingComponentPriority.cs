namespace MugenMvvm.Binding.Constants
{
    public static class BindingComponentPriority
    {
        #region Fields

        public const int Delay = 0;
        public const int EventHandler = 0;
        public const int ParameterHandler = 1000;
        public const int Mode = -100;

        public const int ComponentProvider = 0;
        public const int ExpressionBuilder = 0;
        public const int ExpressionPriorityProvider = 0;
        public const int BindingHolder = 0;
        public const int BindingInitializer = 0;

        #endregion
    }
}