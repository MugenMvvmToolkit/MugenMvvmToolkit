using MugenMvvm.Constants;

namespace MugenMvvm.Bindings.Constants
{
    public static class ObserverComponentPriority
    {
        #region Fields

        public const int EventObserverProvider = 1;
        public const int PropertyChangedObserverProvider = 0;
        public const int MemberPathObserverProvider = 0;
        public const int PathProvider = 0;
        public const int Cache = ComponentPriority.Cache;

        #endregion
    }
}