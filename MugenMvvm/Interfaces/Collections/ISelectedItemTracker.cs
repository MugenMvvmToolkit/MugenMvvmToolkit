using System;
using System.ComponentModel;
using MugenMvvm.Attributes;

namespace MugenMvvm.Interfaces.Collections
{
    public interface ISelectedItemTracker<T> : INotifyPropertyChanged, IDisposable
    {
        [Preserve(Conditional = true)]
        T? SelectedItem { get; set; }

        bool SetSelectedItem(T? value);
    }
}