using System;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public class ObserverProviderMock : IObserverProvider
    {
        #region Properties

        public Func<object, IBindingPath, bool, IObserver> Observe { get; set; }

        public Func<object, IBindingPath, bool, IDataContext, IObserver> ObserveWithContext { get; set; }

        #endregion

        #region Implementation of IObserverProvider

        IObserver IObserverProvider.Observe(object target, IBindingPath path, bool ignoreAttachedMembers, IDataContext context)
        {
            if (ObserveWithContext == null)
                return Observe(target, path, ignoreAttachedMembers);
            return ObserveWithContext(target, path, ignoreAttachedMembers, context);
        }

        #endregion
    }
}
