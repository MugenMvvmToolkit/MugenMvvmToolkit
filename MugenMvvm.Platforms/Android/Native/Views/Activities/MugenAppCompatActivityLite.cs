using System.Collections.Generic;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Android.Native.Views.Activities
{
    public partial class MugenAppCompatActivityLite : IValueHolder<IDictionary<string, object?>>, IValueHolder<IWeakReference>
    {
        #region Properties

        IDictionary<string, object?>? IValueHolder<IDictionary<string, object?>>.Value { get; set; }

        IWeakReference? IValueHolder<IWeakReference>.Value { get; set; }

        #endregion
    }
}