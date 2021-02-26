using System;
using System.Collections.Generic;
using Avalonia;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal.Components;

namespace MugenMvvm.Avalonia.Internal
{
    public sealed class AvaloniaObjectAttachedValueStorageProvider : AttachedValueStorageProviderBase<IAvaloniaObject>, IHasPriority
    {
        private static readonly AttachedProperty<IDictionary<string, object?>> DictionaryProperty =
            AvaloniaProperty.RegisterAttached<AvaloniaObjectAttachedValueStorageProvider, IAvaloniaObject, IDictionary<string, object?>>("AttachedDictionary");

        public int Priority { get; set; } = InternalComponentPriority.MetadataOwnerAttachedValueProvider;

        protected override IDictionary<string, object?>? GetAttachedDictionary(IAvaloniaObject item, bool optional)
        {
            var value = item.GetValue(DictionaryProperty);
            if (!optional && value == null)
            {
                value = new SortedList<string, object?>(3, StringComparer.Ordinal);
                item.SetValue(DictionaryProperty, value);
            }

            return value;
        }

        protected override bool ClearInternal(IAvaloniaObject item)
        {
            item.ClearValue(DictionaryProperty);
            return true;
        }
    }
}