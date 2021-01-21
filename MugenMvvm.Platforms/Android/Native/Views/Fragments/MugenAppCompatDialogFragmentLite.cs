﻿using System.Collections.Generic;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Android.Native.Views.Fragments
{
    public partial class MugenAppCompatDialogFragmentLite : IValueHolder<IDictionary<string, object?>>, IValueHolder<IWeakReference>
    {
        IDictionary<string, object?>? IValueHolder<IDictionary<string, object?>>.Value { get; set; }

        IWeakReference? IValueHolder<IWeakReference>.Value { get; set; }
    }
}