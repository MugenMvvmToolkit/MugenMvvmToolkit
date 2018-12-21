using JetBrains.Annotations;

namespace MugenMvvm.Models.Events
{
    public struct ValueChangedEventArgs<T>
    {
        #region Constructors

        public ValueChangedEventArgs(T oldValue, T newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        #endregion

        #region Properties

        public T OldValue { get; }
        
        public T NewValue { get; }

        #endregion
    }
}