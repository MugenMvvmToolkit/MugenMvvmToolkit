namespace MugenMvvm.Constants
{
    public static class CommandComponentPriority
    {
        public const int CommandProvider = 0;
        public const int ConditionEvent = 0;
        public const int Notifier = 0;
        public const int Executor = -1000;
        public const int CommandCleaner = 0;
    }
}