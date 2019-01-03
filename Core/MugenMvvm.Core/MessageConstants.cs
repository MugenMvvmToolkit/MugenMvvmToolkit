namespace MugenMvvm
{
    internal static class MessageConstants
    {
        #region Fields

        public const string MessageSentFromToFormat3 = "'{0}' message was sent from '{1}' to '{2}'";
        public const string StaticDelegateCannotBeWeak = "The static delegate cannot be converted to weak delegate.";
        public const string AnonymousDelegateCannotBeWeak = "The anonymous delegate cannot be converted to weak delegate.";
        public const string CommandCannotBeExecutedString = "The method Execute in RelayCommand cannot be executed because the CanExecute method returns a false value.";
        public const string EnumIsNotValidFormat2 = "'{0}' is not a valid in {1}";
        public const string CapacityShouldBeGreaterOrEqual = "The Capacity should be greater or equal than collection.";
        public const string IndexMustBeWithinBounds = "Index must be within the bounds of the collection.";
        public const string UnhandledEnumFormat1 = "Unhandled enum - '{0}'";
        public const string DuplicateViewMappingFormat3 = "The mapping already exist for the '{0}' to the '{1}' with name '{2}'";
        public const string ArgumentCannotBeNull = "Argument '{0}' cannot be null or empty";
        public const string ArgumentIsNotValid = "Argument '{0}' is not valid";
        public const string ArgumentShouldBeOfType = "Type '{0}' should be of type '{1}', but is not.";
        public const string WrapperTypeShouldBeNonAbstractFormat1 = "The wrapper type '{0}' must be non abstract";
        public const string WrapperTypeNotSupportedFormat1 = "There are no wrapper type for type '{0}'.";
        public const string ViewCreatedFormat2 = "The view {0} for the view-model {1} was created.";
        public const string DuplicateInterfaceFormat3 = "The '{0}' can implement an interface '{1}' only once. The '{0}' with type '{2}', implement it more that once.";
        public const string TraceViewModelLifecycleFormat3 = "{0} ({1}) - {2};";

        #endregion
    }
}