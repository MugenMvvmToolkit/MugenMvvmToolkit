﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MugenMvvm.Models;

namespace MugenMvvm.UnitTests.Models.Internal
{
    public class TestNotifyPropertyChangedModel : NotifyPropertyChangedBase
    {
        private string? _property;

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
    }
}