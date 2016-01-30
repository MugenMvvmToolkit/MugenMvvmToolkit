using System;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Test.TestModels
{
    public class ObserverMock : DisposableObject, IObserver
    {
        #region Fields

        private bool _isAlive = true;

        #endregion

        #region Properties

        public Action Update { get; set; }

        public IBindingPathMembers PathMembers { get; set; }

        public Func<bool, IBindingPathMembers> GetPathMembers { get; set; }

        public Func<bool, bool> IsValid { get; set; }

        public Func<bool, object> GetActualSource { get; set; }

        #endregion

        #region Methods

        public void RaiseValueChanged(ValueChangedEventArgs args = null)
        {
            if (args == null)
                args = ValueChangedEventArgs.FalseEventArgs;
            var handler = ValueChanged;
            if (handler != null) handler(this, args);
        }

        #endregion

        #region Implementation of IObserver

        public bool IsAlive
        {
            get { return _isAlive; }
            set { _isAlive = value; }
        }

        public IBindingPath Path { get; set; }

        public bool IsTrackChangesEnabled { get; set; }

        public object Source { get; set; }

        IBindingPathMembers IObserver.GetPathMembers(bool throwOnError)
        {
            if (GetPathMembers != null)
                return GetPathMembers(throwOnError);
            if (PathMembers == null)
                return UnsetBindingPathMembers.Instance;
            return PathMembers;
        }

        public event EventHandler<IObserver, ValueChangedEventArgs> ValueChanged;

        void IObserver.Update()
        {
            Update();
        }

        bool IObserver.Validate(bool throwOnError)
        {
            return IsValid(throwOnError);
        }

        object IObserver.GetActualSource(bool throwOnError)
        {
            return GetActualSource(throwOnError);
        }

        #endregion
    }
}
