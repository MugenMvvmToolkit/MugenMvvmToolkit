﻿namespace MugenMvvm.Constants
{
    public static class PresenterComponentPriority
    {
        #region Fields

        public const int Presenter = 0;
        public const int CallbackDecorator = 1000;
        public const int ViewPresenterDecorator = 200;
        public const int ViewModelPresenterMediatorProvider = 0;

        #endregion
    }
}