namespace MugenMvvm.Binding.Constants
{
    internal static class BindingMessageConstant
    {
        #region Fields

        public const string PossibleReasons = "\nPossible reasons:\n";
        public const string CannotParseBinaryExpressionFormat2 = "Cannot parse binary expression '{0} {1} ' right expression expected";
        public const string CannotParseConditionExpressionFormat1 = "Cannot parse condition expression '{0} ? ' expected expression";
        public const string CannotParseConditionExpressionFormat2 = "Cannot parse condition expression '{0} ? {1} : ' expected expression";
        public const string CannotParseConditionExpressionExpectedTokenFormat2 = "Cannot parse condition expression '{0} ? {1} ' expected token ':'";
        public const string CannotParseDigitExpressionFormat1 = "Cannot parse digit expression '{0}'";
        public const string CannotParseArgumentExpressionsExpectedFormat2 = "Cannot parse argument expressions '{0}' expected ',' or '{1}' token";
        public const string CannotParseArgumentExpressionsExpectedExpressionFormat1 = "Cannot parse argument expressions '{0}' expected expression";
        public const string CannotParseLambdaExpressionExpectedExpressionFormat1 = "Cannot parse lambda expression '({0}) => ' expected body expression";
        public const string CannotParseLambdaExpressionExpectedTokenFormat1 = "Cannot parse lambda expression '({0})' expected '=>' token";
        public const string CannotParseMethodExpressionExpectedTokenFormat1 = "Cannot parse method expression '{0}' expected '(' token";
        public const string CannotParseParenExpressionExpectedToken = "Cannot parse paren expression expected ')' token";
        public const string CannotParseUnaryExpressionExpectedExpressionFormat1 = "Cannot parse unary expression '{0}' expected expression";
        public const string CannotParseStringExpressionInvalidEscapeSequenceFormat2 = "Cannot parse string '{0}' invalid escape sequence '\\{1}'";
        public const string CannotParseInterpolatedStringExpressionExpectedExpressionFormat1 = "Cannot parse interpolated string $'{0}' expected expression";
        public const string CannotParseInterpolatedStringExpressionExpectedTokensFormat1 = "Cannot parse interpolated string $'{0}' expected ':' or '}}' token";
        public const string CannotParseInterpolatedStringExpressionExpectedTokenFormat1 = "Cannot parse interpolated string $'{0}' expected '}}' token";
        public const string CannotParseInterpolatedStringExpressionEmptyFormatFormat1 = "Cannot parse interpolated string $'{0}' empty format specifier";
        public const string CannotParseStringExpressionExpectedTokenFormat2 = "Cannot parse string '{0}' expected '{1}' token";

        public const string BindingMemberMustBeWritableFormat4 = "The binding member must be writable, if it uses the SetValue method, path '{0}', type '{1}', member type '{2}', underlying member '{3}'";
        public const string BindingMemberMustBeReadableFormat4 = "The binding member must be readable, if it uses the GetValue method, path '{0}', type '{1}', member type '{2}', underlying member '{3}'";
        public const string InvalidBindingMemberFormat2 = "The binding member cannot be obtained from the path '{0}' on the '{1}'.";
        public const string UnexpectedExpressionTyperFormat3 = "Unexpected expression type '{0}', expected '{1}', node: '{2}'";
        public const string CannotParseExpressionFormat2 = "Cannot parse expression '{0}' {1}";
        public const string CannotCompileExpressionFormat1 = "Cannot compile expression '{0}'";
        public const string DuplicateLambdaParameterFormat1 = "The lambda parameter '{0}' is defined more than once";
        public const string ExpressionNodeCannotBeNullFormat1 = "The expression node on type '{0}' cannot be null";
        public const string CannotResolveTypeFormat1 = "Cannot resolve type '{0}'";
        public const string CannotResolveResourceFormat1 = "Cannot resolve resource '{0}'";
        public const string CannotUseExpressionExpected = "Cannot use expression '{0}' expected type '{1}'";
        public const string CannotParseBindingParameterFormat3 = "Cannot parse binding parameter '{0}' expected parameter value '{1}' current value '{2}'";

        #endregion
    }
}