using System;
using System.ComponentModel;
using MugenMvvm.Attributes;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Collections
{
    public interface ISelectedItemTracker<T> : INotifyPropertyChanged, IDisposable
    {
        [Preserve(Conditional = true)]
        T? SelectedItem { get; set; }

        bool SetSelectedItem(T? value, IReadOnlyMetadataContext? metadata);
    }
}