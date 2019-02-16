namespace MugenMvvm.Constants
{
    public static class PresenterConstants
    {
        #region Fields

        public const int NavigationPresenterPriority = -1;
        public const int MultiViewModelPresenterPriority = 0; //todo check
        public const int GenericNavigationPresenterPriority = 1;
        public const int CloseHandlerPresenterPriority = int.MinValue + 100;

        #endregion
    }
}