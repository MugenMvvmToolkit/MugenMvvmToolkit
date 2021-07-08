using System;
using System.Windows;
using System.Windows.Data;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Threading;

namespace MugenMvvm.Windows.Bindings
{
    public sealed class DependencyPropertyListener : DependencyObject, IThreadDispatcherHandler
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(object), typeof(DependencyPropertyListener), new PropertyMetadata(default, OnValueChanged));

        private WeakEventListener _listener;

        public object Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public void SetListener(IEventListener listener, object source, string member)
        {
            Should.BeValid(_listener.IsEmpty, nameof(listener));
            BindingOperations.SetBinding(this, ValueProperty, new Binding
            {
                Path = new PropertyPath(member, Array.Empty<object>()),
                Source = source,
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                ValidatesOnNotifyDataErrors = false,
                ValidatesOnDataErrors = false,
                ValidatesOnExceptions = false,
                NotifyOnValidationError = false,
                NotifyOnSourceUpdated = false,
                NotifyOnTargetUpdated = false
            });

            _listener = listener.ToWeak();
        }

        public void Clear()
        {
            if (MugenService.ThreadDispatcher.CanExecuteInline(ThreadExecutionMode.Main))
                Clear(this);
            else
                MugenService.ThreadDispatcher.Execute(ThreadExecutionMode.Main, this, null);
        }

        private static void Clear(DependencyPropertyListener listener)
        {
            if (!listener._listener.IsEmpty)
            {
                listener._listener = default;
                listener.ClearValue(ValueProperty);
            }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var listener = (DependencyPropertyListener)d;
            if (!listener._listener.TryHandle(d, EventArgs.Empty, null))
                Clear(listener);
        }

        void IThreadDispatcherHandler.Execute(object? state) => Clear(this);
    }
}