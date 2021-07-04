namespace MugenMvvm.Constants
{
    public static class ValidationComponentPriority
    {
        public const int ValidatorProvider = 0;
        public const int ValidatorRulesProvider = 0;
        public const int ValidatorErrorManager = 0;
        public const int PropertyChangedObserver = 0;
        public const int RuleValidationHandler = 0;
        public const int ChildValidatorAdapter = 0;
        public const int CycleHandlerDecorator = 1000;
        public const int AsyncBehaviorDecorator = 100;
    }
}