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
    public sealed class AndroidAttachedValueStorageProvider : AttachedValueStorageProviderBase, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = InternalComponentPriority.MetadataOwnerAttachedValueProvider;

        #endregion

        #region Methods

        protected override bool IsSupported(IAttachedValueManager attachedValueManager, object item, IReadOnlyMetadataContext? metadata) => item is Object v && ViewExtensions.IsSupportAttachedValues(v);

        protected override IDictionary<string, object?>? GetAttachedDictionary(object item, bool optional)
        {
            var attachedValues = (AttachedValueHolder?) ViewExtensions.GetAttachedValues((Object) item);
            if (optional || attachedValues != null)
                return attachedValues?.Values;
            attachedValues = new AttachedValueHolder();
            ViewExtensions.SetAttachedValues((Object) item, attachedValues);
            return attachedValues.Values;
        }

        protected override bool ClearInternal(object item)
        {
            var attachedValues = (AttachedValueHolder?) ViewExtensions.GetAttachedValues((Object) item);
            if (attachedValues != null)
            {
                attachedValues.Values.Clear();
                return true;
            }

            return false;
        }

        #endregion

        #region Nested types

        private sealed class AttachedValueHolder : Object
        {
            #region Fields

            public readonly SortedList<string, object?> Values;

            #endregion

            #region Constructors

            public AttachedValueHolder()
            {
                Values = new SortedList<string, object?>(3, StringComparer.Ordinal);
            }

            public AttachedValueHolder(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
            {
                Values = new SortedList<string, object?>(3, StringComparer.Ordinal);
            }

            #endregion
        }

        #endregion
    }
}