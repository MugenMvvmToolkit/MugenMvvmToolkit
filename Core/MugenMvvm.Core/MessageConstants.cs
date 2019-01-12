namespace MugenMvvm
{
    internal static class MessageConstants
    {
        #region Fields

        public const string MessageSentFromToFormat3 = "'{0}' message was sent from '{1}' to '{2}'";
        public const string StaticDelegateCannotBeWeak = "The static delegate cannot be converted to weak delegate";
        public const string AnonymousDelegateCannotBeWeak = "The anonymous delegate cannot be converted to weak delegate";
        public const string CommandCannotBeExecutedString = "The method Execute in RelayCommand cannot be executed because the CanExecute method returns a false value";
        public const string EnumIsNotValidFormat2 = "'{0}' is not a valid in {1}";
        public const string CapacityShouldBeGreaterOrEqual = "The Capacity should be greater or equal than collection";
        public const string IndexMustBeWithinBounds = "Index must be within the bounds of the collection";
        public const string UnhandledEnumFormat1 = "Unhandled enum - '{0}'";
        public const string DuplicateViewMappingFormat3 = "The mapping already exist for the '{0}' to the '{1}' with name '{2}'";
        public const string ArgumentCannotBeNull = "Argument '{0}' cannot be null or empty";
        public const string ArgumentIsNotValid = "Argument '{0}' is not valid";
        public const string ArgumentShouldBeOfType = "Type '{0}' should be of type '{1}', but is not";
        public const string WrapperTypeShouldBeNonAbstractFormat1 = "The wrapper type '{0}' must be non abstract";
        public const string WrapperTypeNotSupportedFormat1 = "There are no wrapper type for type '{0}'";
        public const string ViewCreatedFormat2 = "The view {0} for the view-model {1} was created";
        public const string DuplicateInterfaceFormat3 = "The '{0}' can implement an interface '{1}' only once. The '{0}' with type '{2}', implement it more that once";
        public const string TraceViewModelLifecycleFormat3 = "{0} ({1}) - {2}";
        public const string TraceNavigationFormat5 = "{0}({1}) from '{2}' to '{3}', type '{4}";
        public const string IntOutOfRangeCollection = "Index must be within the bounds of the collection";
        public const string ShouldMethodBeSupportedFormat1 = "The method {0} has not been implemented by this class";
        public const string PresenterCannotShowRequestFormat1 = "Presenter cannot show request '{0}'";
        public const string PresenterCannotHandleRequestFormat1 = "Presenter cannot handle request '{0}', cannot obtain viewmodel using NavigationMetadata.ViewModel key please provide viewmodel";
        public const string TraceViewModelPresenterFormat3 = "The {0} request {1} is handled by {2}";
        public const string FieldOrPropertyNotFoundFormat2 = "Cannot get field/property by name '{0}' on type '{1}'";
        public const string NavigatingResultHasCallback = "The NavigatingResult already has completion callback";
        public const string ObjectInitializedFormat3 = "The '{0}' is already initialized, type '{1}' {2}";
        public const string ViewNotFoundFormat2 = "Unable to find a suitable '{0}' for the '{1}'";
        public const string CannotCloseMediator = "The view is closed, before close the view you should show it.";
        public const string ShouldSupportOnlyFieldsReadonlyFields = "supports only properties (non-readonly) and fields";
        public const string IoCCannotFindBindingFormat1 = "Cannot find binding for type {0}";
        public const string IoCCyclicalDependencyFormat1 = "A cyclical dependency was detected for type {0}";
        public const string IoCCannotFindConstructorFormat1 = "Cannot find constructor for type {0}";
        public const string IoCMoreThatOneBindingFormat1 = "Cannot activate type {0} found more that one binding";
        public const string ObjectDisposedFormat1 = "Cannot perform the operation, because the current '{0}' is disposed.";

        #endregion
    }
}