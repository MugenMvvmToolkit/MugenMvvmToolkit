using System.Collections.Generic;
using Android.Runtime;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Android.Native.Views
{
    public partial class ViewWrapper : IWeakReference, IValueHolder<IDictionary<string, object?>>
    {
        #region Fields

        private bool _isAlive = true;

        #endregion

        #region Properties

        bool IWeakItem.IsAlive => _isAlive;

        object? IWeakReference.Target
        {
            get
            {
                if (_isAlive)
                    return this;
                return null;
            }
        }

        //https://forums.xamarin.com/discussion/356/binding-a-java-library-clarifications-anyone
        //note in this case it's ok to keep state object only for mono objects we shouldn't create java objects from mono runtime
        [Preserve(Conditional = true)]
        IDictionary<string, object?>? IValueHolder<IDictionary<string, object?>>.Value { get; set; }

        #endregion

        #region Implementation of interfaces

        void IWeakReference.Release()
        {
            _isAlive = false;
            ((IValueHolder<IDictionary<string, object?>>)this).Value?.Clear();
        }

        #endregion
    }
}