using System.Collections.Generic;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Android.Native.Views.Activities
{
    public partial class MugenActivity : IValueHolder<IDictionary<string, object?>>, IValueHolder<IWeakReference>
    {
        IDictionary<string, object?>? IValueHolder<IDictionary<string, object?>>.Value { get; set; }

        IWeakReference? IValueHolder<IWeakReference>.Value { get; set; }
    }
}