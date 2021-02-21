using System;
using System.Collections.Generic;
using System.Windows;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal.Components;

namespace MugenMvvm.Windows.Internal
{
    public sealed class DependencyObjectAttachedValueStorageProvider : AttachedValueStorageProviderBase<DependencyObject>, IHasPriority
    {
        private static readonly DependencyProperty DictionaryProperty = DependencyProperty.RegisterAttached(
            "AttachedDictionary", typeof(object), typeof(DependencyObject), new PropertyMetadata(null));

        public int Priority { get; set; } = InternalComponentPriority.MetadataOwnerAttachedValueProvider;

        protected override IDictionary<string, object?>? GetAttachedDictionary(DependencyObject item, bool optional)
        {
            var value = item.GetValue(DictionaryProperty);
            if (!optional && value == null)
            {
                value = new SortedList<string, object?>(3, StringComparer.Ordinal);
                item.SetValue(DictionaryProperty, value);
            }

            return (IDictionary<string, object?>?) value;
        }

        protected override bool ClearInternal(DependencyObject item)
        {
            item.ClearValue(DictionaryProperty);
            return true;
        }
    }
}