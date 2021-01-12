namespace MugenMvvm.Bindings.Constants
{
    internal static class BindingInternalConstant
    {
        #region Fields

        public const string ItemTemplate = nameof(ItemTemplate);
        public const string ItemTemplateSelector = nameof(ItemTemplateSelector);
        public const string ContentTemplate = nameof(ContentTemplate);
        public const string ContentTemplateSelector = nameof(ContentTemplateSelector);
        public const string StableIdProvider = nameof(StableIdProvider);
        public const string ItemsSource = nameof(ItemsSource);

        public const string ChangedEventPostfix = "Changed";
        public const string ChangeEventPostfix = "Change";
        public const string PropertyChangedObserverMember = "~p";
        public const string EventPrefixObserverMember = "~e";
        public const string IndexerGetterName = "get_Item";
        public const string IndexerSetterName = "set_Item";
        public const string ArrayGetterName = "Get";
        public const string ArraySetterName = "Set";
        public const string BindPrefix = "@#b";
        public const string AttachedPropertyPrefix = "$#p";
        public const string AttachedEventPrefix = "$#e";
        public const string AttachedMethodPrefix = "$#m";
        public const string WrapMemberPrefix = "$#-";

        public const string AttachedHandlerEventPrefix = "$#1";
        public const string AttachedHandlerPropertyPrefix = "$#2";
        public const string AttachedHandlerMethodPrefix = "$#3";

        public const string RootObserver = "@$r$";

        #endregion
    }
}