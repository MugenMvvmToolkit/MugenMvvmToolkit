using System;
using System.Collections.Generic;
using Android.Runtime;
using MugenMvvm.Android.Native.Views;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal.Components;
using Object = Java.Lang.Object;

namespace MugenMvvm.Android.Internal
{
    public sealed class AndroidAttachedValueStorageProvider : AttachedValueStorageProviderBase<Object>, IHasPriority
    {
        public int Priority { get; init; } = InternalComponentPriority.MetadataOwnerAttachedValueProvider;

        protected override bool IsSupported(IAttachedValueManager attachedValueManager, object item, IReadOnlyMetadataContext? metadata) =>
            item is Object v && ViewMugenExtensions.IsSupportAttachedValues(v);

        protected override IDictionary<string, object?>? GetAttachedDictionary(Object item, bool optional)
        {
            var attachedValues = (AttachedValueHolder?) ViewMugenExtensions.GetAttachedValues(item);
            if (optional || attachedValues != null)
                return attachedValues?.Values;
            attachedValues = new AttachedValueHolder();
            ViewMugenExtensions.SetAttachedValues(item, attachedValues);
            return attachedValues.Values;
        }

        protected override bool ClearInternal(Object item)
        {
            var attachedValues = (AttachedValueHolder?) ViewMugenExtensions.GetAttachedValues(item);
            if (attachedValues != null)
            {
                attachedValues.Values.Clear();
                return true;
            }

            return false;
        }

        private sealed class AttachedValueHolder : Object
        {
            public readonly SortedList<string, object?> Values;

            public AttachedValueHolder()
            {
                Values = new SortedList<string, object?>(3, StringComparer.Ordinal);
            }

            public AttachedValueHolder(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
            {
                Values = new SortedList<string, object?>(3, StringComparer.Ordinal);
            }
        }
    }
}