namespace MugenMvvm.Constants
{
    internal static class MessageConstant
    {
        #region Fields

        public const string StaticDelegateCannotBeWeak = "The static delegate cannot be converted to weak delegate";
        public const string AnonymousDelegateCannotBeWeak = "The anonymous delegate cannot be converted to weak delegate";
        public const string CommandCannotBeExecutedString = "The method Execute in command cannot be executed because the CanExecute method returns a false value";
        public const string EnumIsNotValidFormat2 = "'{0}' is not a valid in {1}";
        public const string CapacityShouldBeGreaterOrEqual = "The Capacity should be greater or equal than collection";
        public const string IndexMustBeWithinBounds = "Index must be within the bounds of the collection";
        public const string UnhandledEnumFormat1 = "Unhandled enum - '{0}'";
        public const string ArgumentCannotBeNull = "Argument '{0}' cannot be null or empty";
        public const string ArgumentNotValid = "Argument '{0}' is not valid";
        public const string ArgumentShouldBeOfType = "Type '{0}' should be of type '{1}', but is not";
        public const string WrapperTypeNotSupportedFormat1 = "There are no wrapper type for type '{0}'";
        public const string IntOutOfRangeCollection = "Index must be within the bounds of the collection";
        public const string ShouldMethodBeSupportedFormat1 = "The method {0} has not been implemented by this class";
        public const string PresenterCannotShowRequestFormat2 = "Presenter cannot show request '{0}'-'{1}'";
        public const string FieldOrPropertyNotFoundFormat2 = "Cannot get field/property by name '{0}' on type '{1}'";
        public const string ObjectInitializedFormat2 = "The '{0}' is already initialized '{1}'";
        public const string ShouldSupportOnlyFieldsReadonlyFields = "supports only properties (non-readonly) and fields";
        public const string CannotResolveService = "Cannot resolve '{0}'";
        public const string ObjectDisposedFormat1 = "Cannot perform the operation, because the current '{0}' is disposed.";
        public const string ObjectNotInitializedFormat2 = "The '{0}' is not initialized {1}";
        public const string CannotGetComponentFormat2 = "Cannot get component {0} owner {1}";
        public const string AmbiguousMappingMatchFound = "Ambiguous mapping match found";
        public const string ObjectNotInitializedOrRequestNotSupportedFormat4 = "The '{0}' is not initialized ({1}) or request '{2}' is not supported {3}";

        #endregion
    }
}