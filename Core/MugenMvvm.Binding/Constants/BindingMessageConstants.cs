namespace MugenMvvm.Binding.Constants
{
    internal static class BindingMessageConstants
    {
        #region Fields

        public const string BindingMemberMustBeWritableFormat4 =
            "The binding member must be writable, if it uses the SetValue method, path '{0}', type '{1}', member type '{2}', underlying member '{3}'";

        public const string BindingMemberMustBeReadableFormat4 =
            "The binding member must be readable, if it uses the GetValue method, path '{0}', type '{1}', member type '{2}', underlying member '{3}'";

        internal const string InvalidBindingMemberFormat2 = "The binding member cannot be obtained from the path '{0}' on the '{1}'.";

        internal const string UnexpectedExpressionTyperFormat3 = "Unexpected expression type '{0}', expected '{1}', node: '{2}'";

        internal const string CannotParseExpressionFormat1 = "Cannot parse expression '{0}'";

        internal const string CannotCompileExpressionFormat1 = "Cannot compile expression '{0}'";

        internal const string DuplicateLambdaParameterFormat1 = "The lambda parameter '{0}' is defined more than once";

        internal const string ExpressionNodeCannotBeNullFormat1 = "The expression node on type '{0}' cannot be null";

        internal const string CannotResolveTypeFormat1 = "Cannot resolve type '{0}'";

        #endregion
    }
}