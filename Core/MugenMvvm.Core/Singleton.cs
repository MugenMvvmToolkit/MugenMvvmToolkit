namespace MugenMvvm
{
    public static class Singleton<TService>
        where TService : class
    {
        #region Fields

        private static TService? _service;
        private static bool _hasValue;

        #endregion

        #region Properties

        public static TService Instance
        {
            get
            {
                if (_service == null)
                    _service = default;
                return _service!;
            }
        }

        public static TService? InstanceOptional
        {
            get
            {
                if (_service == null)
                    _service = default;
                return _service;
            }
        }

        #endregion

        #region Methods

        public static void Initialize(TService service)
        {
            _service = service;
        }

        public static void ClearForUnitTest()
        {
        }

        #endregion
    }
}