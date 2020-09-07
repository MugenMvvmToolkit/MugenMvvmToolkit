using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MugenMvvm.Models;

namespace MugenMvvm.UnitTest.Models.Internal
{
    public class TestNotifyPropertyChangedModel : NotifyPropertyChangedBase
    {
        #region Fields

        private string? _property;

        #endregion

        #region Properties

        public string? Property
        {
            get => _property;
            set
            {
                if (_property == value)
                    return;
                _property = value;
                OnPropertyChanged();
            }
        }

        public Action<PropertyChangedEventArgs>? OnPropertyChangedInternalHandler { get; set; }

        public Action<bool>? OnEndSuspendHandler { get; set; }

        #endregion

        #region Methods

        public new void OnPropertyChanged([CallerMemberName] string? propertyName = null) => base.OnPropertyChanged(propertyName);

        public new void OnPropertyChanged(PropertyChangedEventArgs args) => base.OnPropertyChanged(args);

        public new void ClearPropertyChangedSubscribers() => base.ClearPropertyChangedSubscribers();

        protected override void OnPropertyChangedInternal(PropertyChangedEventArgs args)
        {
            OnPropertyChangedInternalHandler?.Invoke(args);
            base.OnPropertyChangedInternal(args);
        }

        protected override void OnEndSuspend(bool isDirty)
        {
            OnEndSuspendHandler?.Invoke(isDirty);
            base.OnEndSuspend(isDirty);
        }

        #endregion
    }
}