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

        #endregion
    }
}