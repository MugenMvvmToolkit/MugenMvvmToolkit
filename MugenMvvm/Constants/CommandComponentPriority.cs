namespace MugenMvvm.Constants
{
    public static class CommandComponentPriority
    {
        #region Fields

        public const int CommandProvider = 0;
        public const int ConditionEvent = 0;
        public const int Executor = ComponentPriority.PostInitializer;

        #endregion
    }
}